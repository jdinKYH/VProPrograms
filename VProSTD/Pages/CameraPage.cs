using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using System;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// [CONNECT CAMERA] 탭: 이미지 입력 설정
    ///   왼쪽   : 취득/불러온 이미지 확인 화면 (CogDisplay)
    ///   오른쪽 : 카메라 모델, CONNECT / TRIGGER, 노출, File Mode (Single Image / Image Folder)
    /// </summary>
    public partial class CameraPage : UserControl
    {
        private VisionApp app;
        private CogDisplay display;

        // 코드에서 값을 바꾸는 중에는 이벤트(노출 변경, File Mode 변경)를 무시하기 위해 쓴다
        private bool updating;

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

            OnRecipeChanged();
            UpdateCameraInfo();
        }

        private void OnRecipeChanged()
        {
            updating = true;
            chkFileMode.Checked = app.FileMode;
            updating = false;
            UpdateButtons();
        }

        /// <summary>
        /// 카메라 모델 / 노출 / 표시 갱신
        /// </summary>
        private void UpdateCameraInfo()
        {
            updating = true;
            if (app.Acq == null)
            {
                lblCameraModel.Text = "...";
            }
            else
            {
                lblCameraModel.Text = app.Acq.CameraName;
                double? exposure = app.Acq.Exposure;
                if (exposure != null)
                {
                    decimal value = (decimal)exposure.Value;
                    if (value < numExposure.Minimum) value = numExposure.Minimum;
                    if (value > numExposure.Maximum) value = numExposure.Maximum;
                    numExposure.Value = value;
                }
            }
            updating = false;
            UpdateButtons();
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

            btnConnect.Enabled = !fileMode;
            btnTrigger.Enabled = !fileMode && connected;
            numExposure.Enabled = !fileMode && connected && app.Acq.Exposure != null;
        }

        private void ShowImage(ICogImage image)
        {
            display.Image = image;
            display.Fit(false);
        }

        #region 카메라

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                app.ConnectCamera();
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
            try
            {
                ShowImage(app.GrabOnly());
            }
            catch (Exception ex)
            {
                app.WriteLog("CAMERA: 취득 실패 - " + ex.Message, LogKind.Ng);
            }
        }

        /// <summary>
        /// 노출을 바꾸면 바로 카메라에 적용한다. (레시피 SAVE 때 Acq.vpp 에 저장됨)
        /// </summary>
        private void numExposure_ValueChanged(object sender, EventArgs e)
        {
            if (updating || app == null || app.Acq == null) return;

            try
            {
                app.Acq.Exposure = (double)numExposure.Value;
            }
            catch (Exception ex)
            {
                app.WriteLog("CAMERA: 노출 변경 실패 - " + ex.Message, LogKind.Warning);
            }
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
        /// 고른 파일/폴더의 첫 이미지를 미리 보여 준다 (읽은 뒤 처음으로 되돌림).
        /// </summary>
        private void ShowFirstImage()
        {
            if (app.Files.FileCount == 0) return;
            try
            {
                string name;
                ShowImage(app.Files.Next(out name));
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
