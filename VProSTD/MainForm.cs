using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// 메인 화면
    ///
    /// 화면 구성 (위에서 아래로)
    ///   타이틀 바 : 로고 / Vision Inspection / Camera·PLC 표시등 / Recipe / 날짜·시간
    ///   탭 버튼   : RUN / RECIPE / CONNECT CAMERA / SYSTEM ............ EXIT
    ///   페이지    : 선택한 탭의 화면 (Pages 폴더의 UserControl)
    ///   상태줄    : 마지막 메시지
    ///
    /// 화면 배치는 MainForm.Designer.cs (Visual Studio 디자이너) 에서 고친다.
    /// 검사·레시피·카메라 동작은 Core/VisionApp.cs 에 있다.
    /// </summary>
    public partial class MainForm : Form
    {
        private VisionApp app;

        // 탭마다 화면(페이지)을 하나씩 만들어 두고, 탭을 누르면 그 화면만 보이게 한다.
        private RunPage runPage;
        private RecipePage recipePage;
        private CameraPage cameraPage;
        private SystemPage systemPage;

        public MainForm()
        {
            InitializeComponent();

            LoadLogo();
            UpdateClock();
            timerClock.Start();

            // 프로그램 중심 객체 (system.xml 을 읽음)
            app = new VisionApp();
            app.Confirm = ConfirmMessage;
            app.Warn = WarnMessage;
            app.LogAdded += (text, kind) => SetStatus(text);
            app.RecipeChanged += () => SetRecipeName(app.RecipeName);
            app.CameraChanged += () => SetCameraLamp(app.IsCameraConnected);

            CreatePages();
            ShowPage(runPage, btnTabRun);   // 처음에는 RUN 탭
            SetPlcLamp(false);              // PLC 는 아직 연결 기능 없음

            this.Shown += MainForm_Shown;
        }

        /// <summary>
        /// 화면이 뜬 뒤: 마지막으로 쓰던 레시피를 불러오고, 저장된 카메라에 자동으로 연결한다.
        /// (레시피를 먼저 불러와야 카메라 연결 때 레시피 노출이 적용된다)
        /// </summary>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            app.WriteLog("SYSTEM: Vision Inspection 시작");

            Cursor = Cursors.WaitCursor;
            LoadLastRecipe();
            ConnectSavedCamera();
            Cursor = Cursors.Default;
        }

        private void LoadLastRecipe()
        {
            string last = app.System.LastRecipe;
            if (string.IsNullOrEmpty(last)) return;
            if (!RecipeManager.GetNames(app.System.RecipePath).Contains(last)) return;

            try
            {
                app.LoadRecipe(last);
            }
            catch (Exception ex)
            {
                app.WriteLog("마지막 레시피를 불러오지 못했습니다: " + ex.Message, LogKind.Ng);
            }
        }

        /// <summary>
        /// 저장된 카메라 자동 연결. 실패하면 작업자가 알 수 있게 창을 띄운다 (연결 설정은 엔지니어 담당).
        /// </summary>
        private void ConnectSavedCamera()
        {
            try
            {
                app.ConnectSavedCamera();
            }
            catch (Exception ex)
            {
                app.WriteLog("CAMERA: 자동 연결 실패 - " + ex.Message, LogKind.Ng);
                Cursor = Cursors.Default;
                WarnMessage("카메라에 자동으로 연결하지 못했습니다.\n" + ex.Message + "\n\n" +
                            "카메라 전원·케이블을 확인하고, 다른 프로그램(QuickBuild 등)이 카메라를 쓰고 있지 않은지 확인하세요.\n" +
                            "계속 안 되면 엔지니어에게 알리세요.");
            }
        }

        private bool ConfirmMessage(string message)
        {
            DialogResult answer = MessageBox.Show(message, "확인", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            return answer == DialogResult.Yes;
        }

        private void WarnMessage(string message)
        {
            MessageBox.Show(message, "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #region 타이틀 바

        /// <summary>
        /// 실행 파일 옆 Resources\logo.png 를 읽어 로고 자리에 보여 준다.
        /// 파일이 없으면 글자로 대신 보여 준다.
        /// </summary>
        private void LoadLogo()
        {
            string logoPath = Path.Combine(Application.StartupPath, "Resources", "logo.png");

            if (File.Exists(logoPath))
            {
                picLogo.Image = Image.FromFile(logoPath);
            }
            else
            {
                Label logoText = new Label();
                logoText.Text = "NANOCORE";
                logoText.ForeColor = Theme.Text;
                logoText.Font = new Font("맑은 고딕", 20F, FontStyle.Bold);
                logoText.Bounds = picLogo.Bounds;
                logoText.TextAlign = ContentAlignment.MiddleLeft;
                panelTitle.Controls.Add(logoText);
                picLogo.Visible = false;
            }
        }

        /// <summary>
        /// 날짜와 시간을 2줄로 표시한다.
        /// </summary>
        private void UpdateClock()
        {
            DateTime now = DateTime.Now;
            lblDateTime.Text = now.ToString("yyyy-MM-dd") + "\r\n" + now.ToString("HH:mm:ss");
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        /// <summary>
        /// 타이틀 바의 Recipe 표시를 바꾼다.
        /// </summary>
        public void SetRecipeName(string recipeName)
        {
            if (string.IsNullOrEmpty(recipeName)) recipeName = "-";
            lblRecipe.Text = recipeName;
        }

        /// <summary>
        /// 표시등 켜기/끄기
        /// </summary>
        public void SetCameraLamp(bool connected)
        {
            lampCamera.IsOn = connected;
        }

        public void SetPlcLamp(bool connected)
        {
            lampPlc.IsOn = connected;
        }

        #endregion

        #region 탭 / 페이지

        /// <summary>
        /// 탭 4개의 화면을 만들어 페이지 자리에 넣어 둔다 (처음에는 모두 숨김).
        /// </summary>
        private void CreatePages()
        {
            runPage = new RunPage();
            recipePage = new RecipePage();
            cameraPage = new CameraPage();
            systemPage = new SystemPage();

            AddPage(runPage);
            AddPage(recipePage);
            AddPage(cameraPage);
            AddPage(systemPage);

            runPage.Init(app);
            recipePage.Init(app);
            cameraPage.Init(app);
            systemPage.Init(app);
        }

        private void AddPage(UserControl page)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            panelPage.Controls.Add(page);
        }

        /// <summary>
        /// 선택한 페이지만 보이게 하고, 해당 탭 버튼을 파란색으로 표시한다.
        /// AUTO RUN 중에는 RUN 탭에서 다른 탭으로 갈 수 없다.
        /// </summary>
        private void ShowPage(UserControl page, Button tabButton)
        {
            if (app.IsAutoRunning && page != runPage)
            {
                SetStatus("AUTO RUN 중에는 다른 탭으로 이동할 수 없습니다. STOP 을 먼저 누르세요.");
                return;
            }

            runPage.Visible = page == runPage;
            recipePage.Visible = page == recipePage;
            cameraPage.Visible = page == cameraPage;
            systemPage.Visible = page == systemPage;

            SetTabColor(btnTabRun, tabButton == btnTabRun);
            SetTabColor(btnTabRecipe, tabButton == btnTabRecipe);
            SetTabColor(btnTabCamera, tabButton == btnTabCamera);
            SetTabColor(btnTabSystem, tabButton == btnTabSystem);
        }

        private void SetTabColor(Button tabButton, bool selected)
        {
            if (selected)
            {
                tabButton.BackColor = Theme.Blue;
                tabButton.FlatAppearance.BorderColor = Theme.Blue;
            }
            else
            {
                tabButton.BackColor = Theme.Button;
                tabButton.FlatAppearance.BorderColor = Theme.Border;
            }
        }

        private void btnTabRun_Click(object sender, EventArgs e)
        {
            ShowPage(runPage, btnTabRun);
        }

        private void btnTabRecipe_Click(object sender, EventArgs e)
        {
            ShowPage(recipePage, btnTabRecipe);
        }

        private void btnTabCamera_Click(object sender, EventArgs e)
        {
            ShowPage(cameraPage, btnTabCamera);
        }

        private void btnTabSystem_Click(object sender, EventArgs e)
        {
            ShowPage(systemPage, btnTabSystem);
        }

        #endregion

        #region 상태줄 / 종료

        public void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();   // 확인은 MainForm_FormClosing 에서 한다
        }

        /// <summary>
        /// 프로그램을 끌 때 (EXIT 버튼, Alt+F4 모두 여기로 온다): 확인 → 정리 → 카메라 해제
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (app.IsAutoRunning)
            {
                e.Cancel = true;
                SetStatus("AUTO RUN 중에는 종료할 수 없습니다. STOP 을 먼저 누르세요.");
                return;
            }

            DialogResult answer = MessageBox.Show("프로그램을 종료할까요?\n(저장하지 않은 레시피 변경은 사라집니다)", "종료 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            timerClock.Stop();
            cameraPage.StopLive();   // Live 취득을 먼저 멈춘 뒤 카메라를 정리한다
            app.WriteLog("SYSTEM: 종료");

            // [안전] 레시피 정리 → 카메라 설정 정리 → 카메라 해제 (안에서 오류를 무시하며 끝까지 진행)
            app.Shutdown();
        }

        #endregion
    }
}
