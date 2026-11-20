using System;
using System.Globalization;
using System.IO;
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

        private const string SettingsFilter = "System Settings (*.xml)|*.xml|All Files (*.*)|*.*";

        /// <summary>
        /// 파일 창을 처음 열 폴더: System Path (없는 폴더면 실행 파일 폴더)
        /// </summary>
        private string SystemFolder()
        {
            string folder = txtSystemPath.Text.Trim();
            if (!Directory.Exists(folder)) folder = app.System.SystemPath;
            if (!Directory.Exists(folder)) folder = Application.StartupPath;
            return folder;
        }

        /// <summary>
        /// LOAD: System Path 에서 설정 파일을 골라 화면에 읽어 온다.
        /// 화면에만 읽어 오므로, 실제로 적용하려면 SAVE 를 누른다.
        /// </summary>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            string path;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "불러올 설정 파일 선택";
                dialog.Filter = SettingsFilter;
                dialog.InitialDirectory = SystemFolder();
                if (dialog.ShowDialog() != DialogResult.OK) return;
                path = dialog.FileName;
            }

            try
            {
                ShowSettings(SystemSettings.LoadFrom(path));
                app.WriteLog("SYSTEM: 설정 파일 읽음 - " + path + " (SAVE 해야 적용됩니다)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("설정 파일을 읽을 수 없습니다:\n" + ex.Message, "LOAD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// SAVE: System Path 에서 저장할 파일을 고른다.
        /// 고른 파일에 저장하고, 지금 실행 중인 설정과 실행 파일 옆 system.xml(다음 실행 때 사용)에도 적용한다.
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

            string path;
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "설정 저장";
                dialog.Filter = SettingsFilter;
                dialog.DefaultExt = "xml";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                dialog.InitialDirectory = SystemFolder();
                dialog.FileName = "system.xml";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                path = dialog.FileName;
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
                app.SaveSystem();   // 실행 중 설정 + 실행 파일 옆 system.xml
                s.SaveTo(path);     // 고른 파일
                ShowSettings(s);    // 비어 있던 경로가 기본값으로 채워졌을 수 있음
                app.WriteLog("SYSTEM: 설정 저장 - " + path);
                MessageBox.Show("저장되었습니다:\n" + path, "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
