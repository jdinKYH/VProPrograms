using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace VProEditAll
{
    /// <summary>
    /// 메인 화면: VisionPro vpp 편집기 (QuickBuild / Job / ToolGroup / ToolBlock)
    ///
    /// 화면 구성
    ///   상단 1줄 : [vpp 파일 로드] [파일 이름] [다른 이름으로 저장] [저장]
    ///   상단 2줄 : 형식 / Job 선택 / 저장 옵션
    ///   가운데   : VisionPro 편집 화면 (파일을 열 때마다 새로 만듦)
    ///   하단     : 상태줄
    ///
    /// 읽는 순서 (아래 #region 순서와 같음)
    ///   1. UI 구성     : 화면 만들기
    ///   2. 불러오기    : [로드] 버튼을 누르면 일어나는 일
    ///   3. 편집 화면   : VisionPro 편집 화면 연결/해제
    ///   4. 저장        : [저장] / [다른 이름으로 저장]
    ///   5. 닫기 / 정리 : 다른 파일을 열거나 프로그램을 끌 때
    /// </summary>
    public partial class MainForm : Form
    {
        private VppDocument _doc;           // 현재 불러온 파일 (없으면 null)
        private Control _editor;            // 현재 편집 화면 (CogToolGroupEditV2 또는 CogToolBlockEditV2)

        // Job 콤보에 목록을 채우는 중인지 표시.
        // 콤보에 항목을 넣으면 "선택 바뀜" 이벤트가 저절로 발생하는데,
        // 채우는 중에는 그 이벤트를 무시하려고 쓴다. (CboJob_SelectedIndexChanged 참고)
        private bool _fillingJobs;

        // 카메라(AcqFifo/GigE) 설정이 있는 파일을 한 번이라도 불러왔는지.
        // true 이면 프로그램 종료 시 카메라 연결을 해제한다. (ReleaseCameras 참고)
        private bool _cameraFileLoaded;

        // 불러오기/저장 작업 중인지 표시.
        // 작업 중에 창 닫기(X)가 처리되면, 닫힌 화면에 파일을 불러오거나
        // 카메라 해제 뒤에 카메라 파일을 불러오는 일이 생길 수 있어서, 작업 중에는 종료를 막는다.
        private bool _working;

        // ----- 상단 1줄: 파일 -----
        private Panel topPanel;
        private Button btnLoad;
        private TextBox txtFileName;
        private Button btnSaveAs;
        private Button btnSave;

        // ----- 상단 2줄: 형식 / Job / 저장 옵션 -----
        private Label lblKind;
        private Label lblJob;
        private ComboBox cboJob;
        private Label lblSaveOption;
        private ComboBox cboSaveOption;

        private Panel panelEditorContainer;   // 편집 화면을 넣는 영역
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolTip _toolTip;

        // 저장 옵션 콤보의 항목 순서
        private const int SaveOptionAll = 0;
        private const int SaveOptionMinimum = 1;

        // 파일 선택 창의 파일 종류
        private const string VppFileFilter = "VisionPro Files (*.vpp)|*.vpp|All Files (*.*)|*.*";

        public MainForm()
        {
            InitializeComponentCustom();
        }

        #region 1. UI 구성

        /// <summary>
        /// 화면에 들어갈 버튼·콤보 등을 코드로 만들어 배치한다.
        /// </summary>
        private void InitializeComponentCustom()
        {
            this.Text = "VisionPro vpp Editor (QuickBuild / Job / ToolGroup / ToolBlock)";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;

            _toolTip = new ToolTip();

            topPanel = new Panel() { Dock = DockStyle.Top, Height = 84 };

            // 1줄: [파일 로드] [파일 이름 .........] [다른 이름으로 저장] [저장]
            // Button3D = 입체감 있게 그린 버튼 (높이 32 는 그림자 2px 포함)
            btnLoad = new Button3D() { Text = "vpp 파일 로드", Left = 12, Top = 9, Width = 180, Height = 32 };
            btnLoad.Click += BtnLoad_Click;

            txtFileName = new TextBox() { Left = 200, Top = 15, Width = 600, ReadOnly = true, TabStop = false };

            btnSaveAs = new Button3D() { Text = "다른 이름으로 저장", Top = 9, Width = 140, Height = 32, Enabled = false };
            btnSaveAs.Click += BtnSaveAs_Click;

            btnSave = new Button3D() { Text = "저장", Top = 9, Width = 100, Height = 32, Enabled = false };
            btnSave.Click += BtnSave_Click;

            // 2줄: 형식: [QuickBuild]  Job: [콤보]  저장 옵션: [콤보]
            lblKind = new Label() { Left = 12, Top = 50, Width = 180, Height = 24, TextAlign = ContentAlignment.MiddleLeft, BorderStyle = BorderStyle.FixedSingle, Text = "형식: -" };

            lblJob = new Label() { Left = 200, Top = 50, Width = 40, Height = 24, TextAlign = ContentAlignment.MiddleRight, Text = "Job:" };
            cboJob = new ComboBox() { Left = 244, Top = 51, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cboJob.SelectedIndexChanged += CboJob_SelectedIndexChanged;

            lblSaveOption = new Label() { Left = 480, Top = 50, Width = 70, Height = 24, TextAlign = ContentAlignment.MiddleRight, Text = "저장 옵션:" };
            cboSaveOption = new ComboBox() { Left = 554, Top = 51, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            cboSaveOption.Items.Add("전체 (이미지·결과 포함) - All");        // SaveOptionAll
            cboSaveOption.Items.Add("최소 (이미지·결과 제외) - Minimum");    // SaveOptionMinimum
            cboSaveOption.SelectedIndex = SaveOptionAll;   // 기본: All
            _toolTip.SetToolTip(cboSaveOption, "Minimum 은 파일이 작아지지만, 저장 후 다시 열어 결과가 같은지 확인하세요.");

            topPanel.Controls.AddRange(new Control[] { btnLoad, txtFileName, btnSaveAs, btnSave, lblKind, lblJob, cboJob, lblSaveOption, cboSaveOption });
            topPanel.Resize += TopPanel_Resize;

            panelEditorContainer = new Panel() { Dock = DockStyle.Fill };

            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel() { Text = "파일을 불러오세요. (vpp 파일은 프로그램 코드를 포함할 수 있으니 믿을 수 있는 파일만 여세요)", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            statusStrip.Items.Add(lblStatus);

            // Dock 순서 주의: Fill(가운데) 을 먼저 추가해야 Top/Bottom 에 가려지지 않는다.
            this.Controls.Add(panelEditorContainer);
            this.Controls.Add(topPanel);
            this.Controls.Add(statusStrip);

            this.FormClosing += MainForm_FormClosing;

            LayoutTopPanel();
            ShowEmptyEditor();   // 처음에는 빈 편집 화면을 보여 줌
        }

        /// <summary>
        /// 창 크기가 바뀌면 상단 버튼 위치를 다시 계산한다.
        /// </summary>
        private void TopPanel_Resize(object sender, EventArgs e)
        {
            LayoutTopPanel();
        }

        /// <summary>
        /// 상단 1줄 배치: 저장 버튼들은 오른쪽 끝에 붙이고, 파일 이름 칸은 남은 폭을 모두 쓴다.
        /// </summary>
        private void LayoutTopPanel()
        {
            const int margin = 12;
            const int gap = 8;

            btnSave.Left = topPanel.ClientSize.Width - margin - btnSave.Width;
            btnSaveAs.Left = btnSave.Left - gap - btnSaveAs.Width;

            txtFileName.Left = btnLoad.Right + gap;
            txtFileName.Width = Math.Max(50, btnSaveAs.Left - gap - txtFileName.Left);
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        /// <summary>
        /// 시간이 걸리는 작업 중에는 대기 커서를 보이고 상단 버튼을 막는다.
        /// message 를 주면 상태줄에도 표시한다.
        /// </summary>
        private void SetBusy(bool busy, string message = null)
        {
            this.UseWaitCursor = busy;
            if (busy) Cursor.Current = Cursors.WaitCursor;
            else Cursor.Current = Cursors.Default;

            topPanel.Enabled = !busy;
            if (message != null) SetStatus(message);
            Application.DoEvents();   // 상태줄 글자를 바로 화면에 그리기 위해
        }

        /// <summary>
        /// 파일이 열려 있는지에 따라 상단 표시와 버튼을 갱신한다.
        /// </summary>
        private void UpdateUi()
        {
            bool loaded = _doc != null;
            btnSave.Enabled = loaded;
            btnSaveAs.Enabled = loaded;

            if (loaded)
            {
                txtFileName.Text = Path.GetFileName(_doc.FilePath);
                _toolTip.SetToolTip(txtFileName, _doc.FilePath);
                lblKind.Text = "형식: " + VppInspector.DisplayName(_doc.Kind);
            }
            else
            {
                txtFileName.Text = string.Empty;
                _toolTip.SetToolTip(txtFileName, string.Empty);
                lblKind.Text = "형식: -";

                _fillingJobs = true;   // 목록을 지우는 동안 콤보 선택 이벤트 무시
                cboJob.Items.Clear();
                _fillingJobs = false;
                cboJob.Enabled = false;
            }
        }

        #endregion

        #region 2. 불러오기

        /// <summary>
        /// [vpp 파일 로드] 버튼.
        /// </summary>
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            _working = true;
            try
            {
                LoadNewFile();
            }
            finally
            {
                _working = false;   // 중간에 return 하거나 오류가 나도 반드시 실행됨
            }
        }

        /// <summary>
        /// 새 파일 불러오기 전체 흐름. 단계마다 실패하거나 취소하면 거기서 멈춘다.
        /// [안전] 아래 순서를 바꾸지 말 것:
        ///   ③④ (바이트 확인·경고) 가 ⑥ (VisionPro 로 불러오기) 보다 먼저여야
        ///   카메라·I/O 에 연결되기 전에 경고할 수 있다.
        /// </summary>
        private void LoadNewFile()
        {
            // ① 열려 있는 파일 저장 확인
            if (!ConfirmCloseCurrent()) return;

            // ② 파일 선택
            string path = SelectVppFile();
            if (path == null) return;

            // ③ 바이트로만 형식/위험 요소 판정 (카메라/I/O 연결 없음)
            VppFileInfo info = InspectFile(path);
            if (info == null) return;

            if (info.Kind == VppKind.Unsupported)
            {
                ShowUnsupportedMessage(info);
                return;
            }

            // ④ 카메라 / I/O 경고 (장비 PC 에서 쓰는 것을 전제로 함)
            if (!ConfirmHardwareWarning(path, info))
            {
                SetStatus("불러오기 취소");
                return;
            }

            // ⑤ 이전 파일 정리 → 빈 편집 화면으로 지운다
            //    (②~④ 에서 취소하면 여기까지 오지 않으므로 현재 파일이 그대로 남음)
            CloseCurrent();
            this.Refresh();   // 불러오는 동안(7~13초) 빈 화면이 보이도록 먼저 다시 그림

            // ⑥ VisionPro 로 불러오기
            if (!LoadFile(path, info)) return;

            // ⑦ 편집 화면 연결 / 결과 표시
            UpdateUi();
            FillJobs();
            ShowLoadResult(info);
        }

        /// <summary>
        /// ② 파일 선택 창. 취소하면 null.
        /// </summary>
        private string SelectVppFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = VppFileFilter;
                dialog.Title = "편집할 vpp 파일 선택 (믿을 수 있는 파일만 여세요)";
                if (_doc != null) dialog.InitialDirectory = Path.GetDirectoryName(_doc.FilePath);

                if (dialog.ShowDialog() != DialogResult.OK) return null;
                return dialog.FileName;
            }
        }

        /// <summary>
        /// ③ 파일을 바이트로만 읽어 형식/카메라/I/O 를 확인한다. 읽지 못하면 null.
        /// </summary>
        private VppFileInfo InspectFile(string path)
        {
            VppFileInfo info = null;
            string errorMessage = null;

            SetBusy(true, "파일 형식 확인 중...");
            try
            {
                info = VppInspector.Inspect(path);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            SetBusy(false);

            if (errorMessage != null)
            {
                SetStatus("파일 형식 확인 실패");
                MessageBox.Show($"파일을 읽을 수 없습니다:\n{errorMessage}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return info;
        }

        private void ShowUnsupportedMessage(VppFileInfo info)
        {
            SetStatus("지원하지 않는 형식: " + info.RootTypeName);
            MessageBox.Show($"지원하지 않는 형식입니다:\n{info.RootTypeName}\n\nQuickBuild / Job / ToolGroup / ToolBlock 파일만 편집할 수 있습니다.", "미지원 형식", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// ④ 카메라 / I/O 설정이 있는 파일이면 불러오기 전에 경고한다. 계속하면 true.
        /// </summary>
        private bool ConfirmHardwareWarning(string path, VppFileInfo info)
        {
            List<string> items = new List<string>();
            if (info.HasGigE) items.Add("GigE 카메라 설정");
            if (info.HasAcqFifo) items.Add("영상 취득(AcqFifo) 설정");
            if (info.HasIOSettings) items.Add("I/O·통신(Discrete, TCP/IP) 설정");

            // 경고할 것이 없으면 바로 진행 (형식을 모르는 파일은 위험할 수 있으므로 경고)
            if (items.Count == 0 && info.Kind != VppKind.Unknown) return true;

            string kindText = $"형식: {VppInspector.DisplayName(info.Kind)}";
            if (info.Kind == VppKind.Unknown) kindText = "형식을 미리 판정할 수 없는 파일입니다.";

            string itemText = "카메라/I/O 설정 포함 여부를 확인할 수 없습니다.";
            if (items.Count > 0) itemText = "포함된 설정:\n - " + string.Join("\n - ", items);

            string message =
                $"{Path.GetFileName(path)}\n{kindText}\n\n{itemText}\n\n" +
                "불러오면 카메라 또는 I/O·통신에 연결을 시도할 수 있습니다.\n" +
                "장비 PC에서는 QuickBuild / 생산 프로그램을 먼저 종료하세요.\n" +
                "(같은 카메라를 두 프로그램이 동시에 쓰면 연결이 끊어질 수 있습니다.)\n" +
                "※ 카메라는 파일을 닫아도 해제되지 않고, 이 편집기를 종료해야 해제됩니다.\n" +
                "※ vpp 파일은 프로그램 코드(스크립트)를 포함할 수 있습니다. 믿을 수 있는 파일만 여세요.\n\n" +
                "계속 불러올까요?";

            DialogResult answer = MessageBox.Show(message, "카메라 / I/O 주의", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            return answer == DialogResult.Yes;
        }

        /// <summary>
        /// ⑥ VisionPro 로 파일을 불러와 _doc 에 넣는다. 성공하면 true.
        /// </summary>
        private bool LoadFile(string path, VppFileInfo info)
        {
            // 불러오는 순간 카메라에 연결될 수 있으므로, 불러오기 "전에" 표시해 둔다.
            // (불러오기가 실패해도 카메라는 이미 연결되었을 수 있음 → 종료 시 해제 필요)
            if (info.HasAcqFifo || info.HasGigE || info.Kind == VppKind.Unknown) _cameraFileLoaded = true;

            string errorMessage = null;

            SetBusy(true, $"불러오는 중... {Path.GetFileName(path)}");
            try
            {
                _doc = VppDocument.Load(path);
            }
            catch (Exception ex)
            {
                _doc = null;
                errorMessage = FullMessage(ex);
            }
            SetBusy(false);

            if (errorMessage != null)
            {
                SetStatus("불러오기 실패");
                UpdateUi();
                MessageBox.Show($"파일 불러오기 중 오류 발생:\n{errorMessage}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ⑦ 불러온 결과를 상태줄에 표시하고, 스크립트 오류가 있으면 알린다.
        /// </summary>
        private void ShowLoadResult(VppFileInfo info)
        {
            StringBuilder status = new StringBuilder();
            status.Append($"형식: {VppInspector.DisplayName(_doc.Kind)}");

            if (_doc.Kind == VppKind.QuickBuild || _doc.Kind == VppKind.Job)
            {
                status.Append($" / Job {_doc.JobCount}개");
            }

            if (_doc.IOEnabled == true) status.Append(" / I/O 사용: 켜짐(주의)");
            else if (_doc.IOEnabled == false) status.Append(" / I/O 사용: 꺼짐");
            // null(QuickBuild 가 아님)이면 표시하지 않음

            if (info.Kind != VppKind.Unknown && info.Kind != _doc.Kind)
            {
                status.Append($" / ※ 파일 판정({VppInspector.DisplayName(info.Kind)})과 실제 형식이 다름");
            }

            if (_cameraFileLoaded) status.Append(" / 카메라 사용 중: 편집기를 종료해야 해제됩니다.");

            // 스크립트 컴파일 오류 확인 (VisionPro 는 이 오류를 불러오기 오류로 알려 주지 않음)
            List<string> scriptErrors = GetScriptErrorsSafe();
            if (scriptErrors.Count > 0)
            {
                status.Insert(0, $"※ 스크립트 오류 {scriptErrors.Count}건 / ");
                ShowScriptErrors(scriptErrors);
            }

            SetStatus(status.ToString());
        }

        /// <summary>
        /// 스크립트 오류 목록. 확인 중 오류가 나면 빈 목록 (불러오기 자체는 성공했으므로).
        /// </summary>
        private List<string> GetScriptErrorsSafe()
        {
            try
            {
                return _doc.GetScriptErrors();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 스크립트 오류를 최대 10건까지 보여 준다.
        /// </summary>
        private void ShowScriptErrors(List<string> scriptErrors)
        {
            const int maxShow = 10;
            int showCount = Math.Min(maxShow, scriptErrors.Count);
            string list = string.Join("\n - ", scriptErrors.GetRange(0, showCount));

            string more = string.Empty;
            if (scriptErrors.Count > maxShow) more = $"\n ... 외 {scriptErrors.Count - maxShow}건";

            MessageBox.Show(
                $"스크립트 컴파일 오류가 있습니다 ({scriptErrors.Count}건):\n - {list}{more}\n\n" +
                "편집은 가능하지만, 이 상태로 저장하면 오류가 있는 스크립트가 그대로 저장됩니다.\n" +
                "편집 화면의 \"스크립트 작성/편집\"에서 오류 내용을 확인하세요.",
                "스크립트 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Job 콤보 채우기 (QuickBuild/Job 파일만 항목이 있음) 후 첫 번째 대상의 편집 화면 연결
        /// </summary>
        private void FillJobs()
        {
            _fillingJobs = true;   // 채우는 동안 콤보 선택 이벤트 무시
            cboJob.Items.Clear();
            for (int i = 0; i < _doc.JobCount; i++)
            {
                cboJob.Items.Add($"{i}: {_doc.GetJobName(i)}");
            }
            cboJob.Enabled = _doc.JobCount > 1;     // Job 이 여러 개일 때만 선택 가능
            if (cboJob.Items.Count > 0) cboJob.SelectedIndex = 0;
            _fillingJobs = false;

            BindEditor(_doc.GetEditSubject(0));
        }

        /// <summary>
        /// Job 콤보에서 다른 Job 을 고르면 그 Job 의 편집 화면으로 바꾼다.
        /// </summary>
        private void CboJob_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_fillingJobs) return;
            if (_doc == null) return;
            if (cboJob.SelectedIndex < 0) return;

            BindEditor(_doc.GetEditSubject(cboJob.SelectedIndex));
        }

        #endregion

        #region 3. 편집 화면

        /// <summary>
        /// 대상에 맞는 편집 화면을 새로 만들어 연결한다.
        /// - CogToolBlock → CogToolBlockEditV2 (Subject 를 CogToolBlock 형으로 연결해야 Inputs/Outputs·스크립트가 보임)
        /// - CogToolGroup → CogToolGroupEditV2
        /// </summary>
        private void BindEditor(CogToolGroup subject)
        {
            DetachEditor();

            if (subject == null)
            {
                SetStatus("편집할 ToolGroup/ToolBlock 을 찾을 수 없습니다.");
                return;
            }

            // 편집 화면을 만들다 오류가 나도 프로그램이 꺼지지 않게 한다.
            try
            {
                CogToolBlock toolBlock = subject as CogToolBlock;   // ToolBlock 이 아니면 null
                if (toolBlock != null)
                {
                    CogToolBlockEditV2 toolBlockEditor = new CogToolBlockEditV2();
                    AddEditor(toolBlockEditor);
                    toolBlockEditor.Subject = toolBlock;
                }
                else
                {
                    CogToolGroupEditV2 toolGroupEditor = new CogToolGroupEditV2();
                    AddEditor(toolGroupEditor);
                    toolGroupEditor.Subject = subject;
                }

                HideFileButtons(_editor);
            }
            catch (Exception ex)
            {
                ShowEmptyEditor();
                SetStatus("편집 화면 표시 실패");
                MessageBox.Show($"편집 화면을 표시할 수 없습니다:\n{FullMessage(ex)}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 편집 화면을 가운데 영역에 꽉 차게 넣고 _editor 로 기억한다.
        /// </summary>
        private void AddEditor(Control editor)
        {
            editor.Dock = DockStyle.Fill;
            _editor = editor;
            panelEditorContainer.Controls.Add(editor);
        }

        /// <summary>
        /// 파일이 없을 때 보여 주는 빈 편집 화면 (왼쪽 빈 도구 트리 + 오른쪽 빈 디스플레이)
        /// - 빈 화면에서도 툴바 "열기/저장"을 누를 수 있으므로 숨긴다.
        ///   (상단 [로드] 의 경고 과정을 거치지 않고 파일이 열리는 것을 막기 위해)
        /// </summary>
        private void ShowEmptyEditor()
        {
            DetachEditor();
            try
            {
                CogToolGroupEditV2 emptyEditor = new CogToolGroupEditV2();
                AddEditor(emptyEditor);
                emptyEditor.Subject = null;
                HideFileButtons(emptyEditor);
            }
            catch
            {
                DetachEditor();   // 빈 화면 표시 실패는 무시 (편집 기능과 무관)
            }
        }

        /// <summary>
        /// 편집 화면과 편집 대상(Subject)의 연결을 끊는다.
        /// </summary>
        private void ClearEditorSubject()
        {
            CogToolBlockEditV2 toolBlockEditor = _editor as CogToolBlockEditV2;
            CogToolGroupEditV2 toolGroupEditor = _editor as CogToolGroupEditV2;

            if (toolBlockEditor != null) toolBlockEditor.Subject = null;
            else if (toolGroupEditor != null) toolGroupEditor.Subject = null;
        }

        /// <summary>
        /// 편집 화면의 연결을 끊고 화면에서 없앤다.
        /// </summary>
        private void DetachEditor()
        {
            if (_editor == null) return;
            try
            {
                ClearEditorSubject();
                panelEditorContainer.Controls.Remove(_editor);
                _editor.Dispose();
            }
            catch
            {
                // 화면 정리 중 오류는 무시
            }
            _editor = null;
        }

        /// <summary>
        /// 편집 화면 툴바에서 숨길 버튼 (툴팁 문구 기준, 한글/영문)
        /// [안전] 열기 / 저장 / 다른 이름으로 저장 은 숨긴다.
        ///   - 편집 화면 자체 저장 버튼은 상단 저장(임시 파일·백업)을 거치지 않는다.
        ///   - QuickBuild/Job 파일에서 이 버튼으로 저장하면 ToolGroup 만 따로 저장되어 파일 형식이 바뀐다.
        /// ※ "실행", "루프" 버튼은 테스트에 필요해서 숨기지 않는다 (사용자 요청).
        ///   대신 실행하면 실제 카메라로 영상을 취득하므로 생산 프로그램을 먼저 종료해야 한다.
        /// </summary>
        private static readonly string[] HiddenButtonTips =
        {
            "열기", "저장", "다른 이름으로 저장",
            "Open", "Save", "Save As"
        };

        /// <summary>
        /// 편집 화면 툴바에서 파일 버튼을 숨긴다.
        /// 화면 안의 모든 컨트롤을 차례로 들어가며 툴바를 찾는다 (자기 자신을 다시 부르는 재귀 호출).
        /// ※ 버튼 툴팁 문구로 찾는 방식이라 VisionPro 언어 설정에 따라 달라질 수 있다.
        /// </summary>
        private void HideFileButtons(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                ToolBar toolBar = child as ToolBar;
                if (toolBar != null)
                {
                    foreach (ToolBarButton button in toolBar.Buttons)
                    {
                        if (IsHiddenButton(button.ToolTipText)) button.Visible = false;
                    }
                }

                HideFileButtons(child);   // 이 컨트롤 안쪽도 찾아본다
            }
        }

        /// <summary>
        /// 툴팁 문구가 숨길 버튼에 해당하는지.
        /// 정확히 같거나, "저장 (Ctrl+S)" 처럼 뒤에 단축키 등이 붙은 경우도 해당한다.
        /// </summary>
        private static bool IsHiddenButton(string toolTipText)
        {
            string tip = string.Empty;
            if (toolTipText != null) tip = toolTipText.Trim();

            foreach (string hidden in HiddenButtonTips)
            {
                if (tip.Equals(hidden, StringComparison.OrdinalIgnoreCase)) return true;
                if (tip.StartsWith(hidden + " ", StringComparison.OrdinalIgnoreCase)) return true;
                if (tip.StartsWith(hidden + "(", StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        #endregion

        #region 4. 저장

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_doc == null) return;

            _working = true;
            try
            {
                string backupPath;
                if (SaveCurrent(_doc.FilePath, out backupPath))
                {
                    ShowSaveDoneMessage(backupPath);
                }
            }
            finally
            {
                _working = false;
            }
        }

        private void BtnSaveAs_Click(object sender, EventArgs e)
        {
            if (_doc == null) return;

            _working = true;
            try
            {
                string newPath = SelectSavePath();
                if (newPath == null) return;

                string backupPath;
                if (SaveCurrent(newPath, out backupPath))
                {
                    UpdateUi();   // 현재 파일이 새 파일로 바뀜
                    ShowSaveDoneMessage(backupPath);
                }
            }
            finally
            {
                _working = false;
            }
        }

        /// <summary>
        /// 다른 이름으로 저장할 경로 선택 창. 취소하면 null.
        /// </summary>
        private string SelectSavePath()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = VppFileFilter;
                dialog.Title = "다른 이름으로 저장";
                dialog.DefaultExt = "vpp";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;   // 같은 이름이 있으면 덮어쓸지 묻기
                dialog.InitialDirectory = Path.GetDirectoryName(_doc.FilePath);
                dialog.FileName = Path.GetFileName(_doc.FilePath);

                if (dialog.ShowDialog() != DialogResult.OK) return null;
                return dialog.FileName;
            }
        }

        private void ShowSaveDoneMessage(string backupPath)
        {
            string backupText = string.Empty;
            if (backupPath != null) backupText = $"\n백업: {Path.GetFileName(backupPath)}";

            MessageBox.Show($"저장되었습니다:\n{Path.GetFileName(_doc.FilePath)}{backupText}", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 선택한 저장 옵션으로 저장한다. 성공하면 true.
        /// backupPath: 만들어진 백업(.bak) 파일 경로 (새 파일이면 null)
        /// </summary>
        private bool SaveCurrent(string path, out string backupPath)
        {
            backupPath = null;

            bool isMinimum = cboSaveOption.SelectedIndex == SaveOptionMinimum;
            CogSerializationOptionsConstants options = CogSerializationOptionsConstants.All;
            if (isMinimum) options = CogSerializationOptionsConstants.Minimum;

            // Minimum 저장은 입력 이미지·결과가 영구히 빠지므로 한 번 더 확인
            if (isMinimum && !ConfirmMinimumSave()) return false;

            string errorMessage = null;

            SetBusy(true, $"저장 중... {Path.GetFileName(path)}");
            try
            {
                backupPath = _doc.Save(path, options);
            }
            catch (Exception ex)
            {
                errorMessage = FullMessage(ex);
            }
            SetBusy(false);

            if (errorMessage != null)
            {
                SetStatus("저장 실패 (원본 파일은 변경되지 않았습니다)");
                MessageBox.Show($"저장 중 오류 발생:\n{errorMessage}\n\n원본 파일은 변경되지 않았습니다.", "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string optionText = "All";
            if (isMinimum) optionText = "Minimum";

            string backupText = string.Empty;
            if (backupPath != null) backupText = $" / 백업: {Path.GetFileName(backupPath)}";

            SetStatus($"저장 완료: {Path.GetFileName(_doc.FilePath)} ({optionText}){backupText}");
            return true;
        }

        private bool ConfirmMinimumSave()
        {
            DialogResult answer = MessageBox.Show(
                "저장 옵션이 \"최소(Minimum)\"입니다.\n\n" +
                "툴의 입력 이미지와 마지막 실행 결과가 빠진 채로 저장됩니다.\n" +
                "(학습 패턴·학습 이미지는 유지됩니다.)\n\n계속 저장할까요?",
                "저장 옵션 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            return answer == DialogResult.Yes;
        }

        #endregion

        #region 5. 닫기 / 정리

        /// <summary>
        /// 열려 있는 파일이 있으면 저장할지 묻는다 (변경 여부와 관계없이 항상).
        /// 계속 진행해도 되면 true, [취소] 를 누르거나 저장에 실패하면 false.
        /// </summary>
        private bool ConfirmCloseCurrent()
        {
            if (_doc == null) return true;

            DialogResult answer = MessageBox.Show(
                $"현재 파일을 저장할까요?\n{Path.GetFileName(_doc.FilePath)}",
                "저장 확인", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (answer == DialogResult.Cancel) return false;
            if (answer == DialogResult.No) return true;

            // [예]: 저장. 저장에 실패하면 닫지 않는다.
            string unusedBackupPath;
            return SaveCurrent(_doc.FilePath, out unusedBackupPath);
        }

        /// <summary>
        /// 현재 파일을 닫는다.
        /// [안전] 정리 순서를 바꾸지 말 것: 편집 화면 Subject 끊기 → 정지 → Shutdown/Dispose → 편집 화면 해제
        /// showEmpty: 닫은 뒤 빈 편집 화면을 보일지 (프로그램 종료 중에는 false)
        /// </summary>
        private void CloseCurrent(bool showEmpty = true)
        {
            ClearEditorSubject();

            if (_doc != null)
            {
                _doc.Dispose();   // StopRunning → Shutdown / Dispose
                _doc = null;
            }

            DetachEditor();
            if (showEmpty) ShowEmptyEditor();
            UpdateUi();
        }

        /// <summary>
        /// 카메라(프레임 그래버) 연결 해제 - 프로그램 종료 시에만 호출한다.
        /// [안전] 이 함수를 지우지 말 것.
        ///   카메라 파일을 불러온 프로그램이 카메라 연결을 끊지 않고 종료하면
        ///   R6025(pure virtual function call) 오류창이 뜨고 멈춘다 (시험으로 확인).
        /// - 카메라 파일을 불러온 경우에만 실행한다. (CogFrameGrabbers 를 만드는 것만으로 카메라를 검색하기 때문)
        /// - 파일을 닫을 때는 호출하지 않는다.
        ///   시험해 보니 파일을 닫고 연결을 끊어도 GigE 카메라는 이 프로그램이 종료되어야 해제되었다.
        /// </summary>
        private void ReleaseCameras()
        {
            if (!_cameraFileLoaded) return;
            try
            {
                CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
                foreach (ICogFrameGrabber frameGrabber in frameGrabbers)
                {
                    frameGrabber.Disconnect(false);
                }
            }
            catch
            {
                // [안전] 해제 중 오류는 무시
            }
            _cameraFileLoaded = false;
        }

        /// <summary>
        /// 프로그램을 끌 때: 저장 확인 → 파일 정리 → 카메라 해제
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 불러오기/저장 중에는 종료하지 않는다 (_working 설명 참고)
            if (_working)
            {
                e.Cancel = true;
                SetStatus("작업 중에는 종료할 수 없습니다. 작업이 끝난 뒤 다시 종료하세요.");
                return;
            }

            if (!ConfirmCloseCurrent())
            {
                e.Cancel = true;   // [취소] 를 누르면 종료하지 않음
                return;
            }

            // [안전] 파일 정리에서 오류가 나도 카메라 해제는 반드시 실행되도록 따로 처리한다.
            try
            {
                CloseCurrent(false);   // 종료 중이므로 빈 화면은 만들지 않음
            }
            catch
            {
                // 종료 중 정리 오류는 무시
            }

            ReleaseCameras();   // 안에서 오류를 처리함
        }

        /// <summary>
        /// 오류 메시지 + 그 안에 들어 있는 원인 오류 메시지까지 모두 이어 붙인다 (스크립트 컴파일 오류 등 확인용).
        /// </summary>
        private static string FullMessage(Exception ex)
        {
            StringBuilder text = new StringBuilder(ex.Message);
            Exception inner = ex.InnerException;
            while (inner != null)
            {
                text.Append("\n→ ").Append(inner.Message);
                inner = inner.InnerException;
            }
            return text.ToString();
        }

        #endregion
    }
}
