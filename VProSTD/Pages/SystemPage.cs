using System;
using System.Globalization;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// [SYSTEM] 탭: 프로그램 공통 설정 (실행 파일 옆 system.xml)
    ///   Pixel Size / 폴더 경로 5개 / 이미지 저장 형식 / LOAD / SAVE
    /// </summary>
    public partial class SystemPage : UserControl
    {
        private VisionApp app;

        public SystemPage()
        {
            InitializeComponent();
        }

        public void Init(VisionApp visionApp)
        {
            app = visionApp;
            ShowSettings(app.System);
        }

        /// <summary>
        /// 설정 값 → 화면
        /// </summary>
        private void ShowSettings(SystemSettings s)
        {
            txtPixelX.Text = s.PixelX.ToString(CultureInfo.InvariantCulture);
            txtPixelY.Text = s.PixelY.ToString(CultureInfo.InvariantCulture);

            txtBasePath.Text = s.BasePath;
            txtSystemPath.Text = s.SystemPath;
            txtRecipePath.Text = s.RecipePath;
            txtLogPath.Text = s.LogPath;
            txtResultPath.Text = s.ResultPath;

            rdoJpg.Checked = s.SaveAsJpg;
            rdoBmp.Checked = !s.SaveAsJpg;
            chkOriginSave.Checked = s.OriginSave;
            chkResultSave.Checked = s.ResultSave;
        }

        /// <summary>
        /// LOAD: 파일에 저장된 값으로 화면을 되돌린다 (저장하지 않은 변경은 버림).
        /// </summary>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                ShowSettings(SystemSettings.Load());
                app.WriteLog("SYSTEM: 설정 다시 읽음 (SAVE 해야 적용됩니다)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("system.xml 을 읽을 수 없습니다:\n" + ex.Message, "LOAD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// SAVE: 화면 값 → 설정 → system.xml
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            double pixelX;
            double pixelY;
            if (!double.TryParse(txtPixelX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out pixelX) ||
                !double.TryParse(txtPixelY.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out pixelY))
            {
                MessageBox.Show("Pixel X / Y 에는 숫자를 입력하세요. (예: 0.0125)", "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SystemSettings s = app.System;
            s.PixelX = pixelX;
            s.PixelY = pixelY;
            s.BasePath = txtBasePath.Text.Trim();
            s.SystemPath = txtSystemPath.Text.Trim();
            s.RecipePath = txtRecipePath.Text.Trim();
            s.LogPath = txtLogPath.Text.Trim();
            s.ResultPath = txtResultPath.Text.Trim();
            s.SaveAsJpg = rdoJpg.Checked;
            s.OriginSave = chkOriginSave.Checked;
            s.ResultSave = chkResultSave.Checked;

            try
            {
                app.SaveSystem();
                ShowSettings(s);   // 비어 있던 경로가 기본값으로 채워졌을 수 있음
                app.WriteLog("SYSTEM: 설정 저장");
                MessageBox.Show("저장되었습니다.", "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장할 수 없습니다:\n" + ex.Message, "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region 폴더 선택 버튼 ("...")

        private void SelectFolder(TextBox target)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = target.Text;
                if (dialog.ShowDialog() == DialogResult.OK) target.Text = dialog.SelectedPath;
            }
        }

        private void btnBasePath_Click(object sender, EventArgs e) { SelectFolder(txtBasePath); }
        private void btnSystemPath_Click(object sender, EventArgs e) { SelectFolder(txtSystemPath); }
        private void btnRecipePath_Click(object sender, EventArgs e) { SelectFolder(txtRecipePath); }
        private void btnLogPath_Click(object sender, EventArgs e) { SelectFolder(txtLogPath); }
        private void btnResultPath_Click(object sender, EventArgs e) { SelectFolder(txtResultPath); }

        #endregion
    }
}
