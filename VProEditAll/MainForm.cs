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
            cboSaveOption.Items.Add("전체 (이미지·결과 포함) - All");
            cboSaveOption.Items.Add("최소 (이미지·결과 제외) - Minimum");
            cboSaveOption.SelectedIndex = 0;   // 기본: All
            _toolTip.SetToolTip(cboSaveOption, "Minimum 은 파일이 작아지지만, 저장 후 다시 열어 결과가 같은지 확인하세요.");

            topPanel.Controls.AddRange(new Control[] { btnLoad, txtFileName, btnSaveAs, btnSave, lblKind, lblJob, cboJob, lblSaveOption, cboSaveOption });
            topPanel.Resize += (s, e) => LayoutTopPanel();   // 창 크기가 바뀌면 버튼 위치 다시 계산

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
        /// </summary>
        private void SetBusy(bool busy, string message = null)
        {
            this.UseWaitCursor = busy;
            Cursor.Current = busy ? Cursors.WaitCursor : Cursors.Default;
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
                _fillingJobs = true;
                cboJob.Items.Clear();
                _fillingJobs = false;
                cboJob.Enabled = false;
            }
        }

        #endregion

        #region 2. 불러오기

        /// <summary>
        /// [vpp 파일 로드] 버튼.
        /// [안전] 아래 순서를 바꾸지 말 것:
        ///   ① 열린 파일 저장 확인 → ② 파일 선택 → ③ 바이트로 형식 확인 (카메라 연결 없음)
        ///   → ④ 카메라/I/O 경고 → ⑤ 이전 파일 정리 → ⑥ VisionPro 로 불러오기 → ⑦ 결과 표시
        ///   ③④ 가 ⑥ 보다 먼저여야 카메라·I/O 에 연결되기 전에 경고할 수 있다.
        /// </summary>
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            // ① 열려 있는 파일 저장 확인
            if (!ConfirmCloseCurrent()) return;

            // ② 파일 선택
            string path;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "VisionPro Files (*.vpp)|*.vpp|All Files (*.*)|*.*";
                dlg.Title = "편집할 vpp 파일 선택 (믿을 수 있는 파일만 여세요)";
                if (_doc != null) dlg.InitialDirectory = Path.GetDirectoryName(_doc.FilePath);
                if (dlg.ShowDialog() != DialogResult.OK) return;
                path = dlg.FileName;
            }

            // ③ 바이트로만 형식/위험 요소 판정 (카메라/I/O 연결 없음)
            VppFileInfo info;
            try
            {
                SetBusy(true, "파일 형식 확인 중...");
                info = VppInspector.Inspect(path);
            }
            catch (Exception ex)
            {
                SetBusy(false, "파일 형식 확인 실패");
                MessageBox.Show($"파일을 읽을 수 없습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                SetBusy(false);
            }

            if (info.Kind == VppKind.Unsupported)
            {
                SetStatus("지원하지 않는 형식: " + info.RootTypeName);
                MessageBox.Show($"지원하지 않는 형식입니다:\n{info.RootTypeName}\n\nQuickBuild / Job / ToolGroup / ToolBlock 파일만 편집할 수 있습니다.", "미지원 형식", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ④ 카메라 / I/O 경고 (장비 PC 에서 쓰는 것을 전제로 함)
            if (!ConfirmHardwareWarning(path, info)) { SetStatus("불러오기 취소"); return; }

            // ⑤ 이전 파일 정리 → 빈 편집 화면으로 지운 뒤 불러오기
            //    (파일 선택·경고에서 취소하면 여기까지 오지 않으므로 현재 파일이 그대로 남음)
            CloseCurrent();
            this.Refresh();   // 불러오는 동안(7~13초) 빈 화면이 보이도록 먼저 다시 그림

            // ⑥ VisionPro 로 불러오기
            try
            {
                SetBusy(true, $"불러오는 중... {Path.GetFileName(path)}");
                if (info.HasAcqFifo || info.HasGigE || info.Kind == VppKind.Unknown) _cameraFileLoaded = true;
                _doc = VppDocument.Load(path);
            }
            catch (Exception ex)
            {
                _doc = null;
                SetBusy(false, "불러오기 실패");
                UpdateUi();
                MessageBox.Show($"파일 불러오기 중 오류 발생:\n{FullMessage(ex)}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                SetBusy(false);
            }

            // ⑦ 결과 표시
            UpdateUi();
            FillJobs();

            StringBuilder sb = new StringBuilder();
            sb.Append($"형식: {VppInspector.DisplayName(_doc.Kind)}");
            if (_doc.Kind == VppKind.QuickBuild || _doc.Kind == VppKind.Job) sb.Append($" / Job {_doc.JobCount}개");
            if (_doc.IOEnabled.HasValue) sb.Append(_doc.IOEnabled.Value ? " / I/O 사용: 켜짐(주의)" : " / I/O 사용: 꺼짐");
            if (info.Kind != VppKind.Unknown && info.Kind != _doc.Kind) sb.Append($" / ※ 파일 판정({VppInspector.DisplayName(info.Kind)})과 실제 형식이 다름");
            if (_cameraFileLoaded) sb.Append(" / 카메라 사용 중: 편집기를 종료해야 해제됩니다.");

            // 스크립트 컴파일 오류 확인 (VisionPro 는 이 오류를 불러오기 오류로 알려 주지 않음)
            List<string> scriptErrors = GetScriptErrorsSafe();
            if (scriptErrors.Count > 0)
            {
                sb.Insert(0, $"※ 스크립트 오류 {scriptErrors.Count}건 / ");
                const int maxShow = 10;
                string list = string.Join("\n - ", scriptErrors.GetRange(0, Math.Min(maxShow, scriptErrors.Count)));
                string more = scriptErrors.Count > maxShow ? $"\n ... 외 {scriptErrors.Count - maxShow}건" : string.Empty;
                MessageBox.Show(
                    $"스크립트 컴파일 오류가 있습니다 ({scriptErrors.Count}건):\n - {list}{more}\n\n" +
                    "편집은 가능하지만, 이 상태로 저장하면 오류가 있는 스크립트가 그대로 저장됩니다.\n" +
                    "편집 화면의 \"스크립트 작성/편집\"에서 오류 내용을 확인하세요.",
                    "스크립트 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            SetStatus(sb.ToString());
        }

        /// <summary>
        /// 스크립트 오류 목록. 확인 중 오류가 나면 빈 목록 (불러오기 자체는 성공했으므로).
        /// </summary>
        private List<string> GetScriptErrorsSafe()
        {
            try { return _doc.GetScriptErrors(); }
            catch { return new List<string>(); }
        }

        /// <summary>
        /// 카메라 / I/O 설정이 있는 파일이면 불러오기 전에 경고한다. 계속하면 true.
        /// </summary>
        private bool ConfirmHardwareWarning(string path, VppFileInfo info)
        {
            List<string> items = new List<string>();
            if (info.HasGigE) items.Add("GigE 카메라 설정");
            if (info.HasAcqFifo) items.Add("영상 취득(AcqFifo) 설정");
            if (info.HasIOSettings) items.Add("I/O·통신(Discrete, TCP/IP) 설정");
            if (items.Count == 0 && info.Kind != VppKind.Unknown) return true;   // 경고할 것 없음

            string kindText = info.Kind == VppKind.Unknown ? "형식을 미리 판정할 수 없는 파일입니다." : $"형식: {VppInspector.DisplayName(info.Kind)}";
            string itemText = items.Count > 0 ? "포함된 설정:\n - " + string.Join("\n - ", items) : "카메라/I/O 설정 포함 여부를 확인할 수 없습니다.";

            string msg =
                $"{Path.GetFileName(path)}\n{kindText}\n\n{itemText}\n\n" +
                "불러오면 카메라 또는 I/O·통신에 연결을 시도할 수 있습니다.\n" +
                "장비 PC에서는 QuickBuild / 생산 프로그램을 먼저 종료하세요.\n" +
                "(같은 카메라를 두 프로그램이 동시에 쓰면 연결이 끊어질 수 있습니다.)\n" +
                "※ 카메라는 파일을 닫아도 해제되지 않고, 이 편집기를 종료해야 해제됩니다.\n" +
                "※ vpp 파일은 프로그램 코드(스크립트)를 포함할 수 있습니다. 믿을 수 있는 파일만 여세요.\n\n" +
                "계속 불러올까요?";
            return MessageBox.Show(msg, "카메라 / I/O 주의", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
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
            if (_fillingJobs || _doc == null || cboJob.SelectedIndex < 0) return;
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
                if (subject is CogToolBlock toolBlock)
                {
                    CogToolBlockEditV2 ed = new CogToolBlockEditV2() { Dock = DockStyle.Fill };
                    _editor = ed;
                    panelEditorContainer.Controls.Add(ed);
                    ed.Subject = toolBlock;          // CogToolBlock 형 Subject 사용 (Inputs/Outputs, 스크립트)
                }
                else
                {
                    CogToolGroupEditV2 ed = new CogToolGroupEditV2() { Dock = DockStyle.Fill };
                    _editor = ed;
                    panelEditorContainer.Controls.Add(ed);
                    ed.Subject = subject;
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
        /// 파일이 없을 때 보여 주는 빈 편집 화면 (왼쪽 빈 도구 트리 + 오른쪽 빈 디스플레이)
        /// - 빈 화면에서도 툴바 "열기/저장"을 누를 수 있으므로 숨긴다.
        ///   (상단 [로드] 의 경고 과정을 거치지 않고 파일이 열리는 것을 막기 위해)
        /// </summary>
        private void ShowEmptyEditor()
        {
            DetachEditor();
            try
            {
                CogToolGroupEditV2 ed = new CogToolGroupEditV2() { Dock = DockStyle.Fill };
                _editor = ed;
                panelEditorContainer.Controls.Add(ed);
                ed.Subject = null;
                HideFileButtons(ed);
            }
            catch
            {
                DetachEditor();   // 빈 화면 표시 실패는 무시 (편집 기능과 무관)
            }
        }

        /// <summary>
        /// 편집 화면의 연결을 끊고 화면에서 없앤다.
        /// </summary>
        private void DetachEditor()
        {
            if (_editor == null) return;
            try
            {
                if (_editor is CogToolBlockEditV2 tbEd) tbEd.Subject = null;
                else if (_editor is CogToolGroupEditV2 tgEd) tgEd.Subject = null;
                panelEditorContainer.Controls.Remove(_editor);
                _editor.Dispose();
            }
            catch { }
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
        private void HideFileButtons(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is ToolBar bar)
                {
                    foreach (ToolBarButton b in bar.Buttons)
                    {
                        string tip = (b.ToolTipText ?? string.Empty).Trim();
                        foreach (string hidden in HiddenButtonTips)
                        {
                            // 정확히 같거나, "저장 (Ctrl+S)" 처럼 뒤에 단축키 등이 붙은 경우
                            if (tip.Equals(hidden, StringComparison.OrdinalIgnoreCase) ||
                                tip.StartsWith(hidden + " ", StringComparison.OrdinalIgnoreCase) ||
                                tip.StartsWith(hidden + "(", StringComparison.OrdinalIgnoreCase))
                            {
                                b.Visible = false;
                                break;
                            }
                        }
                    }
                }
                HideFileButtons(c);
            }
        }

        #endregion

        #region 4. 저장

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_doc == null) return;
            if (SaveCurrent(_doc.FilePath, out string backup))
            {
                string backupText = backup != null ? $"\n백업: {Path.GetFileName(backup)}" : string.Empty;
                MessageBox.Show($"저장되었습니다:\n{Path.GetFileName(_doc.FilePath)}{backupText}", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSaveAs_Click(object sender, EventArgs e)
        {
            if (_doc == null) return;

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "VisionPro Files (*.vpp)|*.vpp|All Files (*.*)|*.*";
                dlg.Title = "다른 이름으로 저장";
                dlg.DefaultExt = "vpp";
                dlg.AddExtension = true;
                dlg.OverwritePrompt = true;
                dlg.InitialDirectory = Path.GetDirectoryName(_doc.FilePath);
                dlg.FileName = Path.GetFileName(_doc.FilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                if (SaveCurrent(dlg.FileName, out string backup))
                {
                    UpdateUi();   // 현재 파일이 새 파일로 바뀜
                    string backupText = backup != null ? $"\n백업: {Path.GetFileName(backup)}" : string.Empty;
                    MessageBox.Show($"저장되었습니다:\n{Path.GetFileName(_doc.FilePath)}{backupText}", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// 선택한 저장 옵션으로 저장한다. 성공하면 true.
        /// backupPath: 만들어진 백업(.bak) 파일 경로 (새 파일이면 null)
        /// </summary>
        private bool SaveCurrent(string path, out string backupPath)
        {
            backupPath = null;
            CogSerializationOptionsConstants options = cboSaveOption.SelectedIndex == 1
                ? CogSerializationOptionsConstants.Minimum
                : CogSerializationOptionsConstants.All;

            // Minimum 저장은 입력 이미지·결과가 영구히 빠지므로 한 번 더 확인
            if (options == CogSerializationOptionsConstants.Minimum)
            {
                DialogResult r = MessageBox.Show(
                    "저장 옵션이 \"최소(Minimum)\"입니다.\n\n" +
                    "툴의 입력 이미지와 마지막 실행 결과가 빠진 채로 저장됩니다.\n" +
                    "(학습 패턴·학습 이미지는 유지됩니다.)\n\n계속 저장할까요?",
                    "저장 옵션 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (r != DialogResult.Yes) return false;
            }

            try
            {
                SetBusy(true, $"저장 중... {Path.GetFileName(path)}");
                backupPath = _doc.Save(path, options);
                SetStatus($"저장 완료: {Path.GetFileName(_doc.FilePath)} ({(options == CogSerializationOptionsConstants.All ? "All" : "Minimum")})" +
                          (backupPath != null ? $" / 백업: {Path.GetFileName(backupPath)}" : string.Empty));
                return true;
            }
            catch (Exception ex)
            {
                SetStatus("저장 실패 (원본 파일은 변경되지 않았습니다)");
                MessageBox.Show($"저장 중 오류 발생:\n{FullMessage(ex)}\n\n원본 파일은 변경되지 않았습니다.", "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                SetBusy(false);
            }
        }

        #endregion

        #region 5. 닫기 / 정리

        /// <summary>
        /// 열려 있는 파일이 있으면 저장할지 묻는다 (변경 여부와 관계없이 항상).
        /// 계속 진행해도 되면 true, [취소] 를 누르면 false.
        /// </summary>
        private bool ConfirmCloseCurrent()
        {
            if (_doc == null) return true;

            DialogResult r = MessageBox.Show(
                $"현재 파일을 저장할까요?\n{Path.GetFileName(_doc.FilePath)}",
                "저장 확인", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (r == DialogResult.Cancel) return false;
            if (r == DialogResult.Yes) return SaveCurrent(_doc.FilePath, out _);
            return true;
        }

        /// <summary>
        /// 현재 파일을 닫는다.
        /// [안전] 정리 순서를 바꾸지 말 것: 편집 화면 Subject 끊기 → 정지 → Shutdown/Dispose → 편집 화면 해제
        /// showEmpty: 닫은 뒤 빈 편집 화면을 보일지 (프로그램 종료 중에는 false)
        /// </summary>
        private void CloseCurrent(bool showEmpty = true)
        {
            if (_editor is CogToolBlockEditV2 tbEd) tbEd.Subject = null;
            else if (_editor is CogToolGroupEditV2 tgEd) tgEd.Subject = null;

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
                foreach (ICogFrameGrabber fg in frameGrabbers)
                {
                    fg.Disconnect(false);
                }
            }
            catch { }
            _cameraFileLoaded = false;
        }

        /// <summary>
        /// 프로그램을 끌 때: 저장 확인 → 파일 정리 → 카메라 해제
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmCloseCurrent())
            {
                e.Cancel = true;   // [취소] 를 누르면 종료하지 않음
                return;
            }

            try
            {
                CloseCurrent(false);   // 종료 중이므로 빈 화면은 만들지 않음
                ReleaseCameras();
            }
            catch
            {
                // 종료 중 정리 오류는 무시
            }
        }

        /// <summary>
        /// 오류 메시지 + 그 안에 들어 있는 원인 오류 메시지까지 모두 이어 붙인다 (스크립트 컴파일 오류 등 확인용).
        /// </summary>
        private static string FullMessage(Exception ex)
        {
            StringBuilder sb = new StringBuilder(ex.Message);
            Exception inner = ex.InnerException;
            while (inner != null)
            {
                sb.Append("\n→ ").Append(inner.Message);
                inner = inner.InnerException;
            }
            return sb.ToString();
        }

        #endregion
    }
}
