using Eto.Drawing;
using Eto.Forms;

namespace ANTLRStudio.Editor.Controls
{
    [System.ComponentModel.ToolboxItem(false)]
    public class UnfocusablePanel : Drawable
    {
        public Color BackColor2 { get; set; }
        public Color BorderColor { get; set; }
        public Color ForegroundColor { get; set; }
        public string Text { get; set; }
        public Font Font { get; set; }
        public StringAlignment TextAlignment { get; set; }

        public UnfocusablePanel(Color foregroundColor, Font font = null)
        {
            CanFocus = true;
            this.ForegroundColor = foregroundColor;
            Font = font ?? SystemFonts.Default(12);
            //SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }
        //TODO May fail!
        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(e.ClipRectangle, BackColor2, BackgroundColor, 90))
                e.Graphics.FillRectangle(brush, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            using (var pen = new Pen(BorderColor))
                e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

            if (!string.IsNullOrEmpty(Text))
            {
                //StringFormat sf = new StringFormat
                //{
                //    Alignment = TextAlignment,
                //    LineAlignment = StringAlignment.Center
                //};
                using (var brush = new SolidBrush(ForegroundColor))
                    e.Graphics.DrawText(Font, brush, new PointF(1, 1), Text);
            }
        }
    }
}
