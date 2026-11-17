using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// RUN 탭 [Model Load] 의 레시피 선택 창. 화면이 작아서 디자이너 없이 코드로 만든다.
    /// 사용법: string name = RecipeSelectForm.Select(recipePath, currentName);  (취소하면 null)
    /// </summary>
    public class RecipeSelectForm : Form
    {
        private readonly ListBox listRecipe = new ListBox();

        public static string Select(string recipePath, string currentName)
        {
            using (RecipeSelectForm form = new RecipeSelectForm(RecipeManager.GetNames(recipePath), currentName))
            {
                if (form.ShowDialog() != DialogResult.OK) return null;
                return form.listRecipe.SelectedItem as string;
            }
        }

        private RecipeSelectForm(List<string> names, string currentName)
        {
            Text = "Model Load";
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
