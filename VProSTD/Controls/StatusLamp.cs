using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// 연결 상태 표시등: 테두리 상자 안에 [● 글자] 를 그린다.
    /// IsOn 이 true 이면 초록, false 이면 회색.
    /// </summary>
    public class StatusLamp : Control
    {
        private bool _isOn;

        public StatusLamp()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Input;
            ForeColor = Theme.Text;
        }

        /// <summary>
        /// 켜짐(연결됨) 여부. 값을 바꾸면 다시 그린다.
        /// </summary>
        [DefaultValue(false)]
        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                _isOn = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            // 테두리
            using (Pen pen = new Pen(Theme.Border))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // 왼쪽 동그라미
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int size = Height / 3;
            int x = 12;
            int y = (Height - size) / 2;

            Color lampColor = Theme.LampOff;
            if (_isOn) lampColor = Theme.OkText;

            using (SolidBrush brush = new SolidBrush(lampColor))
            {
                g.FillEllipse(brush, x, y, size, size);
            }

            // 오른쪽 글자
            Rectangle textArea = new Rectangle(x + size + 10, 0, Width - (x + size + 10), Height);
            TextRenderer.DrawText(g, Text, Font, textArea, ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        protected override void OnTextChanged(System.EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }
    }
}
