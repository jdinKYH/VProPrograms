using Cognex.VisionPro;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VProEditorGroup1
{
    public partial class MainForm : Form
    {
        private CogJobManager _jobManager;
        private object _targetSubject; // ToolBlock 혹은 ToolGroup 등 유연한 바인딩용
        private string _currentFilePath = string.Empty;

        // 유연한 에디터 컨트롤 (ToolGroupEditV2 또는 ToolBlockEditV2 선택 가능)
        private CogToolGroupEditV2 _toolGroupEditor; // QuickBuild Job 내부는 보통 ToolGroup 구조

        private Button btnLoad;
        private Button btnSave;
        private Button btnSaveAs;       // 261105@Claude 다른 이름으로 저장 버튼
        private TextBox txtFileName;    // 261105@Claude 로드한 파일 이름 표시
        private ToolTip _toolTip;       // 261105@Claude 파일 전체 경로 툴팁
        private Panel topPanel;         // 261105@Claude 상단 버튼 영역 (레이아웃 계산용)
        private Panel panelEditorContainer;


        public MainForm()
        {
            InitializeComponentCustom();
        }


        /// <summary>
        /// UI 컨트롤 초기화 및 배치
        /// </summary>
        private void InitializeComponentCustom()
        {
            this.Text = "VisionPro Job Editor (QuickBuild 독립형 - Group 대응)";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;

            topPanel = new Panel() { Dock = DockStyle.Top, Height = 50 };

            btnLoad = new Button() { Text = "Job(ToolGroup) 파일 로드", Left = 12, Top = 10, Width = 180, Height = 30 };
            btnLoad.Click += BtnLoad_Click;

            // 261105@Claude 로드한 파일 이름 표시용 텍스트박스 (읽기 전용)
            txtFileName = new TextBox() { Left = 200, Top = 15, Width = 600, ReadOnly = true, TabStop = false };

            // 261105@Claude "변경사항 저장" -> "저장" (우측 끝, 현재 파일에 덮어쓰기)
            btnSave = new Button() { Text = "저장", Top = 10, Width = 100, Height = 30 };
            btnSave.Enabled = false;
            btnSave.Click += BtnSave_Click;

            // 261105@Claude "다른 이름으로 저장" 버튼 추가 ("저장" 왼쪽)
            btnSaveAs = new Button() { Text = "다른 이름으로 저장", Top = 10, Width = 140, Height = 30 };
            btnSaveAs.Enabled = false;
            btnSaveAs.Click += BtnSaveAs_Click;

            // 261105@Claude 버튼 눌림 효과 적용
            ApplyPressEffect(btnLoad);
            ApplyPressEffect(btnSave);
            ApplyPressEffect(btnSaveAs);

            _toolTip = new ToolTip();

            topPanel.Controls.Add(btnLoad);
            topPanel.Controls.Add(txtFileName);
            topPanel.Controls.Add(btnSaveAs);
            topPanel.Controls.Add(btnSave);
            topPanel.Resize += (s, e) => LayoutTopPanel();   // 261105@Claude 창 크기 변경 시 재배치

            panelEditorContainer = new Panel() { Dock = DockStyle.Fill };
            this.Controls.Add(panelEditorContainer);
            this.Controls.Add(topPanel);

            // QuickBuild Job 내부 구조에 최적화된 CogToolGroupEditV2 사용
            _toolGroupEditor = new CogToolGroupEditV2();
            _toolGroupEditor.Dock = DockStyle.Fill;
            panelEditorContainer.Controls.Add(_toolGroupEditor);

            this.FormClosing += MainForm_FormClosing;

            LayoutTopPanel();   // 261105@Claude 초기 배치
        }

        /// <summary>
        /// 261105@Claude 상단 패널 배치: [로드][파일이름 ...][다른 이름으로 저장][저장]
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

        /// <summary>
        /// 261105@Claude 버튼 마우스 오버/눌림 시 색상 변화로 눌린 효과 표시
        /// </summary>
        private void ApplyPressEffect(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(240, 240, 240);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(160, 160, 160);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 241, 251);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 210, 240);
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 261105@Claude 텍스트박스에 현재 파일 이름 표시 (툴팁은 전체 경로)
        /// </summary>
        private void UpdateFileNameDisplay(string filePath)
        {
            txtFileName.Text = string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetFileName(filePath);
            _toolTip.SetToolTip(txtFileName, filePath ?? string.Empty);
        }

        /// <summary>
        /// [로드 버튼] .vpp 파일 선택 및 로드
        /// </summary>
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "VisionPro Job Files (*.vpp)|*.vpp|All Files (*.*)|*.*";
                openFileDialog.Title = "편집할 Job(.vpp) 파일 선택";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _currentFilePath = openFileDialog.FileName;
                    LoadJobFile(_currentFilePath);
                }
            }
        }


        /// <summary>
        /// .vpp 파일 역직렬화 및 툴 추출
        /// toolBlock01.vpp 파일을 예시로, CogJobManager 또는 CogToolBlock 객체를 로드하고, 에디터에 바인딩
        /// </summary>
        private void LoadJobFile(string filePath)
        {
            // =================================================================================================
            // 261026@jdin "ToolGroup 용" vpp 파일을 로드한다. 
            // => CogAcqFifoTool이 포함되어 있다.
            // =================================================================================================
            try
            {
                if (_toolGroupEditor != null)
                {
                    _toolGroupEditor.Subject = null;
                }

                object loadedObject = CogSerializer.LoadObjectFromFile(filePath);

                if (loadedObject is CogJobManager jobManager)
                {
                    _jobManager = jobManager;
                    if (_jobManager.JobCount > 0)
                    {
                        CogJob job = _jobManager.Job(0);

                        // 지적하신 대로 job.VisionTool은 ToolGroup 구조를 가짐
                        // 필요에 따라 CogToolGroup 또는 내부 CogToolBlock 추출
                        _targetSubject = job.VisionTool as CogToolGroup;

                        // 만약 내부의 특정 CogToolBlock을 직접 꺼내서 편집하고 싶다면 아래와 같이 탐색 가능
                        /*
                        if (_targetSubject is CogToolGroup toolGroup)
                        {
                            foreach (ICogTool tool in toolGroup.Tools)
                            {
                                if (tool is CogToolBlock block)
                                {
                                    // 원하는 블록을 찾아 바인딩 가능
                                }
                            }
                        }
                        */
                    }
                }
                else if (loadedObject is CogToolGroup toolGroupObj)
                {
                    _targetSubject = toolGroupObj;
                    _jobManager = null;
                }
                else if (loadedObject is CogToolBlock toolBlockObj)
                {
                    _targetSubject = toolBlockObj;
                    _jobManager = null;
                }

                // 에디터에 바인딩
                if (_targetSubject != null && _targetSubject is CogToolGroup targetGroup)
                {
                    _toolGroupEditor.Subject = targetGroup;
                    btnSave.Enabled = true;
                    btnSaveAs.Enabled = true;           // 261105@Claude
                    UpdateFileNameDisplay(filePath);    // 261105@Claude
                    MessageBox.Show($"파일 로드 성공 (ToolGroup 구조):\n{Path.GetFileName(filePath)}", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("불러온 파일에서 편집 가능한 ToolGroup 구조를 확인할 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSave.Enabled = false;
                    btnSaveAs.Enabled = false;          // 261105@Claude
                    UpdateFileNameDisplay(null);        // 261105@Claude
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Job 파일 로드 중 오류 발생:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = false;
                btnSaveAs.Enabled = false;              // 261105@Claude
                UpdateFileNameDisplay(null);            // 261105@Claude
            }
        }

        /// <summary>
        /// [저장 버튼] 수정본을 원본 .vpp 파일로 덮어쓰기
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath)) return;

            // 261105@Claude 저장 로직을 SaveToFile로 분리 (다른 이름으로 저장과 공용)
            if (SaveToFile(_currentFilePath))
            {
                MessageBox.Show("Job 파일이 성공적으로 수정 및 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 261105@Claude [다른 이름으로 저장 버튼] 저장 위치/이름 지정 후 저장, 이후 현재 파일을 새 파일로 전환
        /// </summary>
        private void BtnSaveAs_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "VisionPro Job Files (*.vpp)|*.vpp|All Files (*.*)|*.*";
                saveFileDialog.Title = "다른 이름으로 저장";
                saveFileDialog.DefaultExt = "vpp";
                saveFileDialog.AddExtension = true;
                saveFileDialog.OverwritePrompt = true;

                if (!string.IsNullOrEmpty(_currentFilePath))
                {
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(_currentFilePath);
                    saveFileDialog.FileName = Path.GetFileName(_currentFilePath);
                }

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

                if (SaveToFile(saveFileDialog.FileName))
                {
                    _currentFilePath = saveFileDialog.FileName;
                    UpdateFileNameDisplay(_currentFilePath);
                    MessageBox.Show($"Job 파일이 저장되었습니다:\n{Path.GetFileName(_currentFilePath)}", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// 261105@Claude 지정 경로로 Job(또는 ToolGroup/ToolBlock) 저장. 성공 시 true
        /// </summary>
        private bool SaveToFile(string filePath)
        {
            try
            {
                object targetToSave = _jobManager != null ? (object)_jobManager : _targetSubject;
                if (targetToSave == null) return false;

                CogSerializer.SaveObjectToFile(
                    targetToSave,
                    filePath,
                    typeof(BinaryFormatter),    // 정상 동작
                    CogSerializationOptionsConstants.All
                );
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Job 파일 저장 중 오류 발생:\n{ex.Message}", "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 자원 해제 (메모리 누수 방지)
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_toolGroupEditor != null)
                {
                    _toolGroupEditor.Subject = null;
                    _toolGroupEditor.Dispose();
                    _toolGroupEditor = null;
                }

                _jobManager = null;
                _targetSubject = null;
            }
            catch 
            {
                // 예외 무시, 종료 시 자원 해제 중 발생할 수 있는 오류는 무시
            }
        }
    }
}
