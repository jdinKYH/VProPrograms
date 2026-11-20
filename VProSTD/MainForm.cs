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
            EnableTitleDrag(panelTitle);   // 테두리 없는 창이라 타이틀 바를 끌어서 옮길 수 있게 한다
            MaximizeBox = false;           // 타이틀 바 더블클릭이 Windows "최대화" 로 바뀌지 않게 (최대화되면 옮길 수 없음)
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
            SetEngineerMode(false);         // 항상 작업자 모드로 시작
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

        // 창 옮기기: 마우스를 누른 곳을 Windows 에 "제목 표시줄" 로 알려 주면 Windows 가 창을 옮겨 준다
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 2;

        /// <summary>
        /// 타이틀 바(와 그 안의 로고·글자·표시등)를 끌면 창이 움직이게 한다.
        /// 더블클릭하면 지금 화면의 왼쪽 위로 돌아간다 (화면 밖으로 밀려났을 때).
        /// </summary>
        private void EnableTitleDrag(Control control)
        {
            control.MouseDown += TitleBar_MouseDown;
            foreach (Control child in control.Controls)
            {
                EnableTitleDrag(child);
            }
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (e.Clicks == 2)
            {
                Location = Screen.FromControl(this).WorkingArea.Location;
                return;
            }

            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
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

            // 작업자 모드에서는 RUN 탭만. 다른 탭은 엔지니어 모드로 바꾼 뒤에 연다
            if (page != runPage && !engineerMode)
            {
                if (!RequestEngineerMode()) return;
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

        #region 작업자 / 엔지니어 모드

        // 작업자 모드: RUN 탭만 쓸 수 있다 (레시피·카메라·설정 변경은 엔지니어 모드에서만).
        // 프로그램은 항상 작업자 모드로 시작한다.
        private bool engineerMode;

        /// <summary>
        /// 엔지니어 비밀번호를 묻는다. 맞으면 엔지니어 모드로 바꾸고 true.
        /// 비밀번호가 아직 없으면(처음, 또는 system.xml 에서 지웠을 때) 새 비밀번호를 정하게 한다.
        /// </summary>
        private bool RequestEngineerMode()
        {
            if (!EngineerPassword.IsSet(app.System))
            {
                string newPassword = PasswordForm.AskNewPassword(this);
                if (newPassword == null) return false;

                try
                {
                    EngineerPassword.Set(app.System, newPassword);
                    app.System.Save();
                }
                catch (Exception ex)
                {
                    // 저장하지 못했으면 다음 실행 때 다시 정해야 하므로 알린다 (이번에는 엔지니어 모드로 들어감)
                    app.WriteLog("SYSTEM: 엔지니어 비밀번호 저장 실패 - " + ex.Message, LogKind.Ng);
                    MessageBox.Show("비밀번호를 system.xml 에 저장하지 못했습니다:\n" + ex.Message, "엔지니어 비밀번호",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                app.WriteLog("SYSTEM: 엔지니어 비밀번호를 새로 정함");
                SetEngineerMode(true);
                return true;
            }

            string password = PasswordForm.AskPassword(this);
            if (password == null) return false;

            if (!EngineerPassword.Check(app.System, password))
            {
                app.WriteLog("SYSTEM: 엔지니어 비밀번호 틀림", LogKind.Warning);
                MessageBox.Show("비밀번호가 맞지 않습니다.", "엔지니어 모드", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            SetEngineerMode(true);
            return true;
        }

        /// <summary>
        /// 모드 버튼과 잠긴 탭 표시를 바꾼다.
        /// </summary>
        private void SetEngineerMode(bool on)
        {
            bool changed = engineerMode != on;
            engineerMode = on;

            if (on)
            {
                btnMode.Text = "엔지니어 모드";
                btnMode.BackColor = Theme.Amber;
                btnMode.ForeColor = Theme.Background;
                btnMode.FlatAppearance.BorderColor = Theme.Amber;
                btnMode.FlatAppearance.MouseOverBackColor = Theme.Amber;
            }
            else
            {
                btnMode.Text = "작업자 모드";
                btnMode.BackColor = Theme.Button;
                btnMode.ForeColor = Theme.Text;
                btnMode.FlatAppearance.BorderColor = Theme.Border;
                btnMode.FlatAppearance.MouseOverBackColor = Theme.ButtonHover;
            }

            // 작업자 모드에서는 잠긴 탭 글자를 흐리게 (누르면 엔지니어 모드 확인 창이 뜸)
            Color tabText = on ? Theme.Text : Theme.SubText;
            btnTabRecipe.ForeColor = tabText;
            btnTabCamera.ForeColor = tabText;
            btnTabSystem.ForeColor = tabText;

            if (changed) app.WriteLog(on ? "SYSTEM: 엔지니어 모드" : "SYSTEM: 작업자 모드");
        }

        /// <summary>
        /// 모드 버튼: 작업자 → 엔지니어 (확인), 엔지니어 → 작업자 (RUN 탭으로 이동)
        /// </summary>
        private void btnMode_Click(object sender, EventArgs e)
        {
            if (engineerMode)
            {
                SetEngineerMode(false);
                ShowPage(runPage, btnTabRun);
            }
            else
            {
                RequestEngineerMode();
            }
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
