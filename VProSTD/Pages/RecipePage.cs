using Cognex.VisionPro.ToolBlock;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// [RECIPE] 탭: 레시피 관리 + ToolBlock 편집
    ///   왼쪽   : ToolBlock 편집 화면 (CogToolBlockEditV2)
    ///   오른쪽 : 레시피 목록, LOAD / COPY / NEW / SAVE / DELETE
    /// 목록과 모든 버튼은 SYSTEM 탭의 Recipe Path 폴더를 쓴다.
    /// </summary>
    public partial class RecipePage : UserControl
    {
        private VisionApp app;
        private CogToolBlockEditV2 editor;

        public RecipePage()
        {
            InitializeComponent();
        }

        public void Init(VisionApp visionApp)
        {
            app = visionApp;
            app.RecipeChanged += OnRecipeChanged;
            app.RecipeUnloading += OnRecipeUnloading;
            app.SystemChanged += RefreshList;   // SYSTEM 탭에서 Recipe Path 를 바꾸면 목록을 다시 읽음

            // 편집 화면은 Cognex 컨트롤이라 디자이너 대신 코드로 넣는다
            editor = new CogToolBlockEditV2();
            editor.Dock = DockStyle.Fill;
            panelEditor.Controls.Add(editor);
            HideFileButtons(editor);

            RefreshList();
        }

        #region 편집 화면 연결

        private void OnRecipeChanged()
        {
            editor.Subject = app.Job != null ? app.Job.ToolBlock : null;
            HideFileButtons(editor);
            RefreshList();
        }

        /// <summary>
        /// [안전] ToolBlock 을 정리하기 전에 편집 화면 연결을 먼저 끊는다.
        /// </summary>
        private void OnRecipeUnloading()
        {
            editor.Subject = null;
        }

        /// <summary>
        /// 편집 화면 툴바에서 숨길 버튼 (툴팁 문구 기준, 한글/영문)
        /// [안전] 열기 / 저장 / 다른 이름으로 저장은 숨긴다.
        ///   편집 화면 자체 저장은 레시피 저장(임시 파일 → 교체 → .bak)을 거치지 않기 때문.
        /// </summary>
        private static readonly string[] HiddenButtonTips = { "열기", "저장", "다른 이름으로 저장", "Open", "Save", "Save As" };

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
                HideFileButtons(child);   // 안쪽 컨트롤도 찾아본다
            }
        }

        private static bool IsHiddenButton(string toolTipText)
        {
            string tip = (toolTipText ?? "").Trim();
            foreach (string hidden in HiddenButtonTips)
            {
                if (tip.Equals(hidden, StringComparison.OrdinalIgnoreCase)) return true;
                if (tip.StartsWith(hidden + " ", StringComparison.OrdinalIgnoreCase)) return true;
                if (tip.StartsWith(hidden + "(", StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        #endregion

        #region 레시피 목록

        /// <summary>
        /// 레시피 목록을 다시 읽고, 현재 레시피를 선택해 둔다.
        /// </summary>
        private void RefreshList()
        {
            List<string> names = RecipeManager.GetNames(app.System.RecipePath);
            listRecipe.Items.Clear();
            foreach (string name in names) listRecipe.Items.Add(name);

            if (app.RecipeName != null) listRecipe.SelectedItem = app.RecipeName;
        }

        /// <summary>
        /// 목록에서 고른 레시피 이름. 고르지 않았으면 안내하고 null.
        /// </summary>
        private string SelectedName()
        {
            string name = listRecipe.SelectedItem as string;
            if (name == null) MessageBox.Show("목록에서 레시피를 고르세요.", "RECIPE");
            return name;
        }

        /// <summary>
        /// 새 이름 입력 창. 취소하거나 이름이 잘못되면 null.
        /// </summary>
        private string AskNewName(string title, string defaultName)
        {
            string name = RecipeManager.CleanName(Interaction.InputBox("레시피 이름을 입력하세요. (파일 이름, .vpp 는 붙이지 않아도 됨)", title, defaultName));
            if (name.Length == 0) return null;   // 취소

            string problem = RecipeManager.CheckName(app.System.RecipePath, name);
            if (problem != null)
            {
                MessageBox.Show(problem, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            return name;
        }

        #endregion

        #region 버튼

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string name = SelectedName();
            if (name == null) return;

            try
            {
                app.LoadRecipe(name);
            }
            catch (Exception ex)
            {
                app.WriteLog("레시피 불러오기 실패: " + ex.Message, LogKind.Ng);
                MessageBox.Show("레시피를 불러올 수 없습니다:\n" + ex.Message, "LOAD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            string source = SelectedName();
            if (source == null) return;

            string name = AskNewName("COPY", source + "_copy");
            if (name == null) return;

            try
            {
                RecipeManager.Copy(app.System.RecipePath, source, name);
                app.WriteLog("RECIPE: '" + source + "' → '" + name + "' 복사");
                RefreshList();
                listRecipe.SelectedItem = name;
            }
            catch (Exception ex)
            {
                MessageBox.Show("복사할 수 없습니다:\n" + ex.Message, "COPY", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            string name = AskNewName("NEW", "");
            if (name == null) return;

            try
            {
                RecipeManager.Create(app.System.RecipePath, name);
                app.WriteLog("RECIPE: '" + name + "' 새로 만듦 (InputImage / Result 포함)");
                RefreshList();
                listRecipe.SelectedItem = name;
            }
            catch (Exception ex)
            {
                MessageBox.Show("만들 수 없습니다:\n" + ex.Message, "NEW", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult answer = MessageBox.Show("'" + name + "' 을(를) 지금 불러올까요?", "NEW", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes) btnLoad_Click(sender, e);
        }

        /// <summary>
        /// SAVE: Recipe Path 에서 저장할 .vpp 파일 이름을 고른다.
        ///   현재 레시피 파일 → 덮어쓰기 (.bak 남김)
        ///   다른 이름        → 다른 이름으로 저장 후 그 레시피로 바뀜
        ///   Recipe Path 밖   → 목록에 안 보이므로 저장하지 않음
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (app.RecipeName == null)
            {
                MessageBox.Show("불러온 레시피가 없습니다.", "SAVE");
                return;
            }

            string recipePath = Path.GetFullPath(app.System.RecipePath).TrimEnd('\\');
            string filePath;
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "레시피 저장 (Recipe Path: " + recipePath + ")";
                dialog.Filter = "VisionPro 레시피 (*.vpp)|*.vpp";
                dialog.DefaultExt = "vpp";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = false;   // 덮어쓰기 확인은 아래에서 레시피 기준으로 한다
                dialog.InitialDirectory = recipePath;
                dialog.FileName = app.RecipeName + RecipeManager.Extension;
                if (dialog.ShowDialog() != DialogResult.OK) return;
                filePath = Path.GetFullPath(dialog.FileName);
            }

            if (!string.Equals(Path.GetDirectoryName(filePath).TrimEnd('\\'), recipePath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Recipe Path 안에 저장하세요.\n" + recipePath, "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = RecipeManager.CleanName(Path.GetFileName(filePath));
            if (name.Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("파일 이름을 확인하세요.", "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 파일 위치로 비교 (Recipe Path 를 바꾼 뒤라면 이름이 같아도 다른 파일임)
            string targetFile = Path.GetFullPath(RecipeManager.FileOf(recipePath, name));
            bool sameRecipe = string.Equals(targetFile, Path.GetFullPath(app.RecipeFilePath), StringComparison.OrdinalIgnoreCase);
            bool otherRecipeExists = !sameRecipe && File.Exists(targetFile);

            string question = "'" + name + "' 에 저장할까요?\n(이전 파일은 .bak 으로 남습니다)";
            if (otherRecipeExists) question = "'" + name + "' 은(는) 다른 레시피입니다.\n현재 레시피로 덮어쓸까요?\n(이전 파일은 .bak 으로 남습니다)";

            DialogResult answer = MessageBox.Show(question, "SAVE", MessageBoxButtons.YesNo,
                otherRecipeExists ? MessageBoxIcon.Warning : MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            try
            {
                if (sameRecipe) app.SaveRecipe();
                else app.SaveRecipeAs(name);

                RefreshList();
                MessageBox.Show("저장되었습니다: " + name, "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                app.WriteLog("레시피 저장 실패: " + ex.Message, LogKind.Ng);
                MessageBox.Show("저장 중 오류:\n" + ex.Message + "\n\n원본 파일은 변경되지 않았습니다.", "SAVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string name = SelectedName();
            if (name == null) return;

            if (string.Equals(name, app.RecipeName, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("현재 불러온 레시피는 삭제할 수 없습니다.", "DELETE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult answer = MessageBox.Show("'" + name + "' 레시피를 삭제할까요?\n(" + name + ".vpp, .bak, 설정 파일)\n삭제하면 되돌릴 수 없습니다.", "DELETE",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            try
            {
                RecipeManager.Delete(app.System.RecipePath, name);
                app.WriteLog("RECIPE: '" + name + "' 삭제", LogKind.Warning);
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("삭제할 수 없습니다:\n" + ex.Message, "DELETE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
