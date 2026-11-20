using System.Drawing;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// 엔지니어 모드 비밀번호 창. 화면이 작아서 디자이너 없이 코드로 만든다.
    ///   string pw = PasswordForm.AskPassword(owner);     비밀번호 입력 (취소하면 null)
    ///   string pw = PasswordForm.AskNewPassword(owner);  새 비밀번호 정하기 (두 번 입력, 취소하면 null)
    /// </summary>
    public class PasswordForm : Form
    {
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtConfirm = new TextBox();
        private readonly Label lblMessage = new Label();
        private readonly bool isNew;

        public static string AskPassword(IWin32Window owner)
        {
            using (PasswordForm form = new PasswordForm(false))
            {
                if (form.ShowDialog(owner) != DialogResult.OK) return null;
                return form.txtPassword.Text;
            }
        }

        public static string AskNewPassword(IWin32Window owner)
        {
            using (PasswordForm form = new PasswordForm(true))
            {
                if (form.ShowDialog(owner) != DialogResult.OK) return null;
                return form.txtPassword.Text;
            }
        }

        private PasswordForm(bool newPassword)
        {
            isNew = newPassword;

            Text = newPassword ? "엔지니어 비밀번호 정하기" : "엔지니어 모드";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = new Font("맑은 고딕", 12F);

            int y = 16;
            Label lblGuide = new Label();
            lblGuide.Bounds = new Rectangle(16, y, 408, newPassword ? 72 : 48);
            lblGuide.ForeColor = Theme.SubText;
            lblGuide.Font = new Font("맑은 고딕", 10F);
            lblGuide.Text = newPassword
                ? "엔지니어 비밀번호가 아직 없습니다. 새 비밀번호를 정하세요.\n(" + EngineerPassword.MinLength + "자 이상. 잊으면 system.xml 에서 지우고 다시 정해야 합니다)"
                : "레시피·카메라·설정을 바꾸는 엔지니어 모드입니다.\n엔지니어 비밀번호를 입력하세요.";
            Controls.Add(lblGuide);
            y += lblGuide.Height + 8;

            y = AddPasswordBox(newPassword ? "새 비밀번호" : "비밀번호", txtPassword, y);
            if (newPassword) y = AddPasswordBox("한 번 더", txtConfirm, y);

            lblMessage.Bounds = new Rectangle(16, y, 408, 26);
            lblMessage.ForeColor = Theme.NgText;
            lblMessage.Font = new Font("맑은 고딕", 10F);
            Controls.Add(lblMessage);
            y += 34;

            Button btnOk = MakeButton("OK", 16, y, Theme.Blue);
            btnOk.Click += (s, e) => TryAccept();
            Button btnCancel = MakeButton("CANCEL", 224, y, Theme.Button);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            ClientSize = new Size(440, y + 52 + 16);
        }

        private int AddPasswordBox(string caption, TextBox box, int y)
        {
            Label label = new Label();
            label.Text = caption;
            label.Bounds = new Rectangle(16, y + 4, 110, 28);
            Controls.Add(label);

            box.Bounds = new Rectangle(130, y, 294, 32);
            box.UseSystemPasswordChar = true;
            box.BackColor = Theme.Input;
            box.ForeColor = Theme.Text;
            box.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(box);
            return y + 44;
        }

        private void TryAccept()
        {
            if (isNew)
            {
                if (txtPassword.Text.Length < EngineerPassword.MinLength)
                {
                    lblMessage.Text = EngineerPassword.MinLength + "자 이상 입력하세요.";
                    return;
                }
                if (txtPassword.Text != txtConfirm.Text)
                {
                    lblMessage.Text = "두 번 입력한 비밀번호가 다릅니다.";
                    txtConfirm.SelectAll();
                    txtConfirm.Focus();
                    return;
                }
            }
            else if (txtPassword.Text.Length == 0)
            {
                lblMessage.Text = "비밀번호를 입력하세요.";
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private static Button MakeButton(string text, int left, int top, Color backColor)
        {
            Button button = new Button();
            button.Text = text;
            button.Bounds = new Rectangle(left, top, 200, 52);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = Theme.Text;
            button.FlatAppearance.BorderColor = Theme.Border;
            button.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            return button;
        }
    }
}
