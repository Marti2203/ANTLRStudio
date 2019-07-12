using System;
using System.Collections.Generic;
using System.ComponentModel;
using Eto.Drawing;
using System.Data;
using System.Text;
using Eto.Forms;
using ANTLRStudio.Editor.Forms;

namespace FastColoredTextBoxNS
{
    public class Ruler : Drawable
    {
        public EventHandler TargetChanged;

        [DefaultValue(typeof(Color), "ControlLight")]
        public Color BackColor2 { get; set; }

        [DefaultValue(typeof(Color), "DarkGray")]
        public Color TickColor { get; set; }

        [DefaultValue(typeof(Color), "Black")]
        public Color CaretTickColor { get; set; }

        FastColoredTextBox target;

        [Description("Target FastColoredTextBox")]
        public FastColoredTextBox Target
        {
            get { return target; }
            set
            {
                if (target != null)
                    UnSubscribe(target);
                target = value;
                Subscribe(target);
                OnTargetChanged();
            }
        }


        //TODO MAX Size
        public Font Font { get; set; }
        public Color ForegroundColor { get; set; }
        public Ruler(Color foregroundColor, Font font = null)
        {
            InitializeComponent();

            //SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            MinimumSize = new Size(0, 24);
            //MaximumSize = new Size(int.MaxValue / 2, 24);
            ForegroundColor = foregroundColor;
            Font = font ?? SystemFonts.Default(12);
            BackColor2 = Colors.White;
            TickColor = Colors.DarkGray;
            CaretTickColor = Colors.Black;
        }



        protected virtual void OnTargetChanged()
        {
            TargetChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void UnSubscribe(FastColoredTextBox target)
        {
            target.Scroll -= target_Scroll;
            target.SelectionChanged -= Target_SelectionChanged;
            target.VisibleRangeChanged -= Target_VisibleRangeChanged;
        }

        protected virtual void Subscribe(FastColoredTextBox target)
        {
            target.Scroll += target_Scroll;
            target.SelectionChanged += Target_SelectionChanged;
            target.VisibleRangeChanged += Target_VisibleRangeChanged;
        }

        private void Target_VisibleRangeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        void Target_SelectionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected virtual void Target_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        //TODO CHECK THIS
        protected override void OnPaint(PaintEventArgs e)
        {
            if (target == null)
                return;

            PointF car = PointFromScreen(target.PointToScreen(target.PlaceToPoint(target.Selection.Start)));

            SizeF fontSize = Font.MeasureString("W");

            int column = 0;
            e.Graphics.FillRectangle(new LinearGradientBrush(new RectangleF(Size), BackgroundColor, BackColor2, 270), new Rectangle(0, 0, Width, Height));

            float columnWidth = target.CharWidth;
            //var sf = new StringFormat
            //{
            //    Alignment = StringAlignment.Center,
            //    LineAlignment = StringAlignment.Near
            //};

            var zeroPoint = new PointF(target.PositionToPoint(0));
            zeroPoint = PointFromScreen(target.PointToScreen(zeroPoint));

            using (var pen = new Pen(TickColor))
            using (var textBrush = new SolidBrush(ForegroundColor))
                for (float x = zeroPoint.X; x < Size.Width; x += columnWidth, ++column)
                {
                    if (column % 10 == 0)
                        e.Graphics.DrawText(Font, textBrush, x, 0f, column.ToString());

                    e.Graphics.DrawLine(pen, (int)x, fontSize.Height + (column % 5 == 0 ? 1 : 3), (int)x, Height - 4);
                }

            using (var pen = new Pen(TickColor))
                e.Graphics.DrawLine(pen, new PointF(car.X - 3, Height - 3), new PointF(car.X + 3, Height - 3));

            using (var pen = new Pen(CaretTickColor))
            {
                e.Graphics.DrawLine(pen, new PointF(car.X - 2, fontSize.Height + 3), new PointF(car.X - 2, Height - 4));
                e.Graphics.DrawLine(pen, new PointF(car.X, fontSize.Height + 1), new PointF(car.X, Height - 4));
                e.Graphics.DrawLine(pen, new PointF(car.X + 2, fontSize.Height + 3), new PointF(car.X + 2, Height - 4));
            }
        }


        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

    }
}
