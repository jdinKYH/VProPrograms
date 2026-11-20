using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// RUN 탭 [Load Recipe] 의 레시피 선택 창. 화면이 작아서 디자이너 없이 코드로 만든다.
    /// 사용법: string name = RecipeSelectForm.Select(recipePath, currentName);  (취소하면 null)
    /// </summary>
    public class RecipeSelectForm : Form
    {
        private readonly ListBox listRecipe = new ListBox();

        public static string Select(string recipePath, string currentName)
        {
            using (RecipeSelectForm form = new RecipeSelectForm(recipePath, RecipeManager.GetNames(recipePath), currentName))
            {
                if (form.ShowDialog() != DialogResult.OK) return null;
                return form.listRecipe.SelectedItem as string;
            }
        }

        private RecipeSelectForm(string recipePath, List<string> names, string currentName)
        {
            Text = "Load Recipe";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 520);
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = new Font("맑은 고딕", 12F);

            listRecipe.Bounds = new Rectangle(16, 16, 388, 420);
            listRecipe.BackColor = Theme.Input;
            listRecipe.ForeColor = Theme.Text;
            listRecipe.BorderStyle = BorderStyle.FixedSingle;
            listRecipe.Font = new Font("Consolas", 13F);
            listRecipe.IntegralHeight = false;
            foreach (string name in names) listRecipe.Items.Add(name);
            if (currentName != null) listRecipe.SelectedItem = currentName;
            listRecipe.DoubleClick += (s, e) => { if (listRecipe.SelectedItem != null) DialogResult = DialogResult.OK; };

            Button btnOk = MakeButton("LOAD", 16, Theme.Blue);
            btnOk.Click += (s, e) => { if (listRecipe.SelectedItem != null) DialogResult = DialogResult.OK; };

            Button btnCancel = MakeButton("CANCEL", 214, Theme.Button);
            btnCancel.DialogResult = DialogResult.Cancel;

            // 목록이 비어 있으면 빈 창 대신 이유와 필요한 폴더 구조를 보여 준다
            if (names.Count == 0)
            {
                Label lblEmpty = new Label();
                lblEmpty.Bounds = new Rectangle(17, 17, 386, 418);
                lblEmpty.BackColor = Theme.Input;
                lblEmpty.ForeColor = Theme.SubText;
                lblEmpty.Font = new Font("맑은 고딕", 10F);
                lblEmpty.Padding = new Padding(8);
                lblEmpty.Text = "불러올 레시피가 없습니다.\n\n" +
                                "Recipe Path:\n" + recipePath + "\n\n" +
                                (Directory.Exists(recipePath) ? "" : "(이 폴더가 없습니다)\n\n") +
                                "레시피는 Recipe Path 안의 ToolBlock .vpp 파일입니다.\n\n" +
                                "  <Recipe Path>\\<레시피 이름>.vpp\n" +
                                "  <Recipe Path>\\<레시피 이름>.recipe.xml (자동 생성)\n\n" +
                                "카메라 설정(CogAcqFifoTool) .vpp 는 목록에 나오지 않습니다.\n\n" +
                                "RECIPE 탭에서 새 레시피를 만들거나,\nToolBlock .vpp 를 Recipe Path 에 넣으세요.";
                Controls.Add(lblEmpty);
            }

            Controls.Add(listRecipe);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private static Button MakeButton(string text, int left, Color backColor)
        {
            Button button = new Button();
            button.Text = text;
            button.Bounds = new Rectangle(left, 452, 190, 52);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = Theme.Text;
            button.FlatAppearance.BorderColor = Theme.Border;
            button.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            return button;
        }
    }
}
