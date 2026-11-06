using Cognex.VisionPro;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ToolBlock;
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

namespace VProEditor01
{
    public partial class MainForm : Form
    {
        private CogJobManager _jobManager;
        private CogToolBlock _targetToolBlock;
        private string _currentFilePath = string.Empty;

        // VisionPro 툴블록 에디터 컨트롤
        private CogToolBlockEditV2 _toolBlockEditor;

        // UI 컨트롤
        private Button btnLoad;
        private Button btnSave;
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
            this.Text = "VisionPro Job Editor (QuickBuild 독립형)";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 상단 버튼 패널
            Panel topPanel = new Panel() { Dock = DockStyle.Top, Height = 50 };

            btnLoad = new Button() { Text = "Job(ToolBlock) 파일 로드", Left = 12, Top = 10, Width = 180, Height = 30 };
            btnLoad.Click += BtnLoad_Click;

            btnSave = new Button() { Text = "변경사항 저장", Left = 200, Top = 10, Width = 120, Height = 30 };
            btnSave.Enabled = false; // 파일 로드 전 비활성화
            btnSave.Click += BtnSave_Click;

            topPanel.Controls.Add(btnLoad);
            topPanel.Controls.Add(btnSave);

            // 에디터 컨테이너 패널
            panelEditorContainer = new Panel() { Dock = DockStyle.Fill };

            this.Controls.Add(panelEditorContainer);
            this.Controls.Add(topPanel);

            // =======================================================
            //  Toolblock editorVisionPro 에디터 컨트롤 생성 및 도킹
            // =======================================================
            _toolBlockEditor = new CogToolBlockEditV2();
            _toolBlockEditor.Dock = DockStyle.Fill;
            panelEditorContainer.Controls.Add(_toolBlockEditor);

            this.FormClosing += MainForm_FormClosing;
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
            // toolBlock으로 만든 .vpp 파일을 로드하고, 에디터에 바인딩
            // =================================================================================================
            try
            {
                // 기존 에디터 바인딩 해제
                if (_toolBlockEditor != null)
                {
                    _toolBlockEditor.Subject = null;
                }

                // 1. 역직렬화 로드
                object loadedObject = CogSerializer.LoadObjectFromFile(filePath);

                // 2. 객체 구조 확인 (JobManager vs ToolBlock)
                if (loadedObject is CogJobManager jobManager)
                {
                    _jobManager = jobManager;
                    if (_jobManager.JobCount > 0)
                    {
                        CogJob job = _jobManager.Job(0);
                        _targetToolBlock = job.VisionTool as CogToolBlock;
                    }
                }
                else if (loadedObject is CogToolBlock toolBlock)
                {
                    _targetToolBlock = toolBlock;
                    _jobManager = null;
                }

                // 3. 에디터 컨트롤 연결
                if (_targetToolBlock != null)
                {
                    _toolBlockEditor.Subject = _targetToolBlock;
                    btnSave.Enabled = true;
                    MessageBox.Show($"파일 로드 성공:\n{Path.GetFileName(filePath)}", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("불러온 파일에서 편집 가능한 CogToolBlock을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSave.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Job 파일 로드 중 오류 발생:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = false;
            }
        }

        /// <summary>
        /// [저장 버튼] 수정본을 원본 .vpp 파일로 덮어쓰기
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("저장할 파일 경로가 지정되지 않았습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                object targetToSave = _jobManager != null ? (object)_jobManager : _targetToolBlock;

                if (targetToSave != null)
                {
                        // 바이너리 포맷으로 직렬화하여 파일로 저장
                         CogSerializer.SaveObjectToFile(
                        targetToSave,
                        _currentFilePath,
                        typeof(BinaryFormatter),    // 정상 동작.
                        CogSerializationOptionsConstants.All
                    );

                    MessageBox.Show("Job 파일이 성공적으로 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Job 파일 저장 중 오류 발생:\n{ex.Message}", "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 자원 해제 (메모리 누수 방지)
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_toolBlockEditor != null)
                {
                    _toolBlockEditor.Subject = null;
                    _toolBlockEditor.Dispose();
                    _toolBlockEditor = null;
                }

                _jobManager = null;
                _targetToolBlock = null;
            }
            catch
            {
                // 예외 무시
            }
        }
    }
}
