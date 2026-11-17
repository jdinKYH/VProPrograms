using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// 카운트 카드: 위에 작은 제목(Title), 아래에 큰 숫자(Value) 를 그린다.
    /// AccentColor 를 지정하면 그 색으로 테두리를 2px 로 그린다 (NG = 빨강, YIELD = 주황).
    /// </summary>
    public class CountCard : Control
    {
        private string _title = "TITLE";
        private string _value = "0";
        private Color _valueColor = Color.FromArgb(230, 232, 235);
        private Color _accentColor = Color.Empty;

        public CountCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Panel;
        }

        /// <summary>위쪽 작은 제목 (예: TOTAL COUNT)</summary>
        [DefaultValue("TITLE")]
        public string Title
        {
            get { return _title; }
            set { _title = value; Invalidate(); }
        }

        /// <summary>아래쪽 큰 숫자 (예: 1,420)</summary>
        [DefaultValue("0")]
        public string Value
        {
            get { return _value; }
            set { _value = value; Invalidate(); }
        }

        /// <summary>숫자 색</summary>
        public Color ValueColor
        {
            get { return _valueColor; }
            set { _valueColor = value; Invalidate(); }
        }

        /// <summary>강조 테두리 색. 비워 두면(Empty) 일반 테두리.</summary>
        public Color AccentColor
        {
            get { return _accentColor; }
            set { _accentColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            // 테두리: 강조색이 있으면 2px, 없으면 일반 1px
            if (_accentColor.IsEmpty)
            {
                using (Pen pen = new Pen(Theme.Border))
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
            else
            {
                using (Pen pen = new Pen(_accentColor, 2))
                    g.DrawRectangle(pen, 1, 1, Width - 2, Height - 2);
            }

            // 작은 제목
            using (Font titleFont = new Font("맑은 고딕", 9F, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, _title, titleFont, new Point(10, 8), Theme.SubText);
            }

            // 큰 숫자 (고정폭 글꼴)
            using (Font valueFont = new Font("Consolas", 20F, FontStyle.Bold))
            {
                Rectangle valueArea = new Rectangle(8, 30, Width - 12, Height - 34);
                TextRenderer.DrawText(g, _value, valueFont, valueArea, _valueColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
