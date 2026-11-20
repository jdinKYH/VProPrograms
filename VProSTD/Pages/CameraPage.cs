using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// [CONNECT CAMERA] 탭: 이미지 입력 설정
    ///   왼쪽   : 취득/불러온 이미지 확인 화면 (CogDisplay)
    ///   오른쪽 : 카메라 모델, SEARCH / 카메라 선택 / CONNECT / TRIGGER / LIVE, 노출, SAVE CAMERA,
    ///            File Mode (Single Image / Image Folder)
    /// 카메라 설정은 레시피와 따로 system.xml 에 저장한다 (SAVE CAMERA). 노출만 레시피별 (SAVE CAMERA 또는 레시피 SAVE 때 레시피 설정에 저장).
    /// </summary>
    public partial class CameraPage : UserControl
    {
        private VisionApp app;
        private CogDisplay display;

        // 코드에서 값을 바꾸는 중에는 이벤트(노출 변경, File Mode 변경)를 무시하기 위해 쓴다
        private bool updating;

        // Live Mode: 켜져 있으면 타이머로 계속 1장씩 취득해서 보여 준다 (TRIGGER 는 막음)
        private Timer timerLive;
        private bool live;

        public CameraPage()
        {
            InitializeComponent();
        }

        public void Init(VisionApp visionApp)
        {
            app = visionApp;
            app.CameraChanged += UpdateCameraInfo;
            app.RecipeChanged += OnRecipeChanged;

            // 이미지 화면은 Cognex 컨트롤이라 디자이너 대신 코드로 넣는다
            display = new CogDisplay();
            display.Dock = DockStyle.Fill;
            panelDisplay.Controls.Add(display);

            timerLive = new Timer();
            timerLive.Interval = 30;
            timerLive.Tick += timerLive_Tick;

            // 다른 탭으로 가면 Live 를 끈다 (RUN 검사와 카메라를 함께 쓰지 않게)
            VisibleChanged += (sender, e) => { if (!Visible) StopLive(); };

            OnRecipeChanged();
            UpdateCameraInfo();
        }

        private void OnRecipeChanged()
        {
            updating = true;
            chkFileMode.Checked = app.FileMode;
            updating = false;
            UpdateCameraInfo();   // 레시피가 바뀌면 연결 전 노출 표시도 바뀐다
        }

        /// <summary>
        /// 카메라 모델 / 노출 / 표시 갱신
        /// </summary>
        private void UpdateCameraInfo()
        {
            if (!app.IsCameraConnected) StopLive();

            updating = true;
            if (app.Camera == null)
            {
                string saved = app.System.Camera.Serial;
                lblCameraModel.Text = string.IsNullOrEmpty(saved) ? "..." : "저장: " + saved;

                // 연결 전에는 레시피에 저장된 노출을 보여 준다 (없으면 빈칸). CONNECT 하면 이 값이 카메라에 적용된다
                if (app.Recipe.ExposureMs > 0) ShowExposure(app.Recipe.ExposureMs);
                else ShowExposure(null);
            }
            else
            {
                lblCameraModel.Text = app.Camera.CameraName;
                ShowExposure(app.Camera.Exposure);
            }
            updating = false;
            UpdateButtons();
        }

        /// <summary>
        /// 노출 칸에 값 표시. null 이면 빈칸 (화면 초기값이 저장된 값처럼 보이지 않게).
        /// </summary>
        private void ShowExposure(double? exposure)
        {
            if (exposure == null)
            {
                numExposure.Text = "";
                return;
            }

            decimal value = (decimal)exposure.Value;
            if (value < numExposure.Minimum) value = numExposure.Minimum;
            if (value > numExposure.Maximum) value = numExposure.Maximum;
            numExposure.Value = value;
            // 값이 같으면 Value 를 넣어도 글자가 다시 써지지 않으므로 (빈칸이었을 때) 직접 쓴다
            numExposure.Text = value.ToString("0.00");
        }

        /// <summary>
        /// File Mode 를 켜면 파일 버튼을, 끄면 카메라 버튼을 쓸 수 있게 한다.
        /// </summary>
        private void UpdateButtons()
        {
            bool fileMode = chkFileMode.Checked;
            bool connected = app != null && app.IsCameraConnected;

            btnSingleImage.Enabled = fileMode;
            btnImageFolder.Enabled = fileMode;

            // Live 중에는 카메라를 바꾸거나 File Mode 로 바꿀 수 없고, TRIGGER 도 막는다
            chkFileMode.Enabled = !live;
            btnSearch.Enabled = !fileMode && !live;
            comboCamera.Enabled = !fileMode && !live;
            btnConnect.Enabled = !fileMode && !live;
            btnTrigger.Enabled = !fileMode && connected && !live;
            btnLive.Enabled = !fileMode && connected;
            numExposure.Enabled = !fileMode && connected && app.Camera.Exposure != null;
            btnCameraSave.Enabled = !fileMode && connected;
        }

        private void ShowImage(ICogImage image)
        {
            display.Image = image;
            display.Fit(false);
        }

        #region 카메라

        /// <summary>
        /// SEARCH: 연결된 카메라를 찾아 목록에 넣고, 저장된 카메라가 있으면 그것을 고른다.
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string message = "카메라를 검색합니다.\n\n" +
                             "장비 PC 에서는 QuickBuild / 생산 프로그램을 먼저 종료하세요.\n" +
                             "※ 카메라는 이 프로그램을 종료해야 해제됩니다.\n\n계속할까요?";
            if (!app.Confirm(message)) return;

            List<CameraInfo> cameras;
            Cursor = Cursors.WaitCursor;
            try
            {
                cameras = app.FindCameras();
            }
            catch (Exception ex)
            {
                app.WriteLog("CAMERA: 검색 실패 - " + ex.Message, LogKind.Ng);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            comboCamera.Items.Clear();
            foreach (CameraInfo camera in cameras)
            {
                comboCamera.Items.Add(camera);
                if (string.Equals(camera.Serial, app.System.Camera.Serial, StringComparison.OrdinalIgnoreCase)) comboCamera.SelectedItem = camera;
            }
            if (comboCamera.SelectedIndex < 0 && comboCamera.Items.Count > 0) comboCamera.SelectedIndex = 0;

            app.WriteLog("CAMERA: " + cameras.Count + " 대 찾음");
        }

        /// <summary>
        /// CONNECT: 목록에서 고른 카메라 (고르지 않았으면 저장된 카메라, 그것도 없으면 첫 번째 카메라)
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            CameraSettings saved = app.System.Camera;
            CameraSettings settings = new CameraSettings();
            settings.Type = saved.Type;
            settings.Serial = saved.Serial;
            settings.VideoFormat = saved.VideoFormat;
            settings.TimeoutMs = saved.TimeoutMs;

            CameraInfo selected = comboCamera.SelectedItem as CameraInfo;
            if (selected != null && !string.Equals(selected.Serial, saved.Serial, StringComparison.OrdinalIgnoreCase))
            {
                // 저장된 것과 다른 카메라: 영상 형식은 그 카메라의 첫 번째 것
                settings.Serial = selected.Serial;
                settings.VideoFormat = "";
            }

            try
            {
                app.ConnectCamera(settings);
            }
            catch (Exception ex)
            {
                app.WriteLog("CAMERA: 연결 실패 - " + ex.Message, LogKind.Ng);
                MessageBox.Show("카메라에 연결할 수 없습니다:\n" + ex.Message, "CONNECT", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            UpdateCameraInfo();
        }

        private void btnTrigger_Click(object sender, EventArgs e)
        {
            if (live) return;   // TRIGGER 는 Live OFF 일 때만

            try
            {
                ICogImage image = app.GrabOnly();
                ShowImage(image);
                app.SetRecipeImage(image);   // RECIPE 탭 편집 화면에서 바로 실행해 볼 수 있게
            }
            catch (Exception ex)
            {
                app.WriteLog("CAMERA: 취득 실패 - " + ex.Message, LogKind.Ng);
            }
        }

        /// <summary>
        /// LIVE: 한 번 누르면 ON, 다시 누르면 OFF
        /// </summary>
        private void btnLive_Click(object sender, EventArgs e)
        {
            if (live) StopLive();
            else StartLive();
        }

        private void StartLive()
        {
            if (live || app == null || !app.IsCameraConnected || chkFileMode.Checked) return;

            live = true;
            liveFirstImage = true;
            btnLive.Text = "LIVE ON";
            btnLive.BackColor = Theme.Blue;
            timerLive.Start();
            app.WriteLog("CAMERA: Live ON");
            UpdateButtons();
        }

        /// <summary>
        /// Live 끄기. 탭 이동, 카메라 연결 끊김, 프로그램 종료 때도 부른다 (꺼져 있으면 아무것도 안 함).
        /// </summary>
        public void StopLive()
        {
            if (!live) return;

            live = false;
            timerLive.Stop();
            btnLive.Text = "LIVE OFF";
            btnLive.BackColor = Theme.Button;
            if (app != null) app.WriteLog("CAMERA: Live OFF");
            UpdateButtons();
        }

        // Live 첫 장만 화면에 맞추고, 그 뒤로는 사용자가 확대한 상태를 유지한다
        private bool liveFirstImage;

        private void timerLive_Tick(object sender, EventArgs e)
        {
            if (!live) return;
            if (!app.IsCameraConnected)
            {
                StopLive();
                return;
            }

            try
            {
                ICogImage image = app.GrabOnly();
                display.Image = image;
                if (liveFirstImage)
                {
                    display.Fit(false);
                    liveFirstImage = false;
                }
            }
            catch (Exception ex)
            {
                // 취득이 실패하면 Live 를 끈다 (같은 오류가 계속 쌓이지 않게)
                app.WriteLog("CAMERA: Live 취득 실패 - " + ex.Message, LogKind.Ng);
                StopLive();
            }
        }

        /// <summary>
        /// 노출을 바꾸면 바로 카메라에 적용한다. 레시피별 값이므로 SAVE CAMERA 또는 RECIPE 탭 SAVE 때 레시피 설정에 저장된다.
        /// </summary>
        private void numExposure_ValueChanged(object sender, EventArgs e)
        {
            if (updating || app == null || app.Camera == null) return;

            try
            {
                app.SetExposure((double)numExposure.Value);
            }
            catch (Exception ex)
            {
                app.WriteLog("CAMERA: 노출 변경 실패 - " + ex.Message, LogKind.Warning);
            }
        }

        /// <summary>
        /// SAVE CAMERA: 지금 연결된 카메라(시리얼, 영상 형식)를 system.xml 에, 노출을 현재 레시피 설정에 저장한다.
        /// </summary>
        private void btnCameraSave_Click(object sender, EventArgs e)
        {
            try
            {
                double? exposure = app.SaveCameraSettings();
                string exposureText = exposure != null
                    ? "노출 " + exposure.Value + " ms → 레시피 '" + app.RecipeName + "' 에 저장"
                    : "노출은 저장하지 않았습니다 (불러온 레시피가 없음)";
                MessageBox.Show("카메라 설정을 저장했습니다.\n" + app.Camera.CameraName + "\n" + app.Camera.VideoFormat +
                                "\n\n" + exposureText, "SAVE CAMERA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장할 수 없습니다:\n" + ex.Message, "SAVE CAMERA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            UpdateCameraInfo();
        }

        #endregion

        #region File Mode

        private void chkFileMode_CheckedChanged(object sender, EventArgs e)
        {
            if (!updating && app != null)
            {
                app.FileMode = chkFileMode.Checked;
                if (chkFileMode.Checked) app.WriteLog("File Mode 켜짐 (이미지 파일로 검사)");
                else app.WriteLog("File Mode 꺼짐 (카메라로 검사)");
            }
            UpdateButtons();
        }

        private void btnSingleImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "검사할 이미지 파일 선택";
                dialog.Filter = "Image Files|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.idb;*.cdb|All Files (*.*)|*.*";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                app.Files.SetSingleFile(dialog.FileName);
                app.WriteLog("FILE: 이미지 1장 - " + dialog.FileName);
            }
            ShowFirstImage();
        }

        private void btnImageFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "검사할 이미지 폴더 선택";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                app.Files.SetFolder(dialog.SelectedPath);
                app.WriteLog("FILE: 이미지 폴더 - " + dialog.SelectedPath + " (" + app.Files.FileCount + " 개 파일)");
            }
            ShowFirstImage();
        }

        /// <summary>
        /// 고른 파일/폴더의 첫 이미지를 미리 보여 주고 레시피 입력에도 넣는다 (읽은 뒤 처음으로 되돌림).
        /// </summary>
        private void ShowFirstImage()
        {
            if (app.Files.FileCount == 0) return;
            try
            {
                string name;
                ICogImage image = app.Files.Next(out name);
                ShowImage(image);
                app.SetRecipeImage(image);   // RECIPE 탭 편집 화면에서 바로 실행해 볼 수 있게
            }
            catch (Exception ex)
            {
                app.WriteLog("FILE: 이미지를 읽을 수 없습니다 - " + ex.Message, LogKind.Warning);
            }
            app.Files.Rewind();
        }

        #endregion
    }
}
