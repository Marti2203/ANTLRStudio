using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;

namespace ANTLRStudio.Drawing
{
    public class TextContainer : TemplatedControl
    {
        public static readonly DirectProperty<TextContainer, double> VerticalPaddingProperty =
          AvaloniaProperty.RegisterDirect<TextContainer, double>(
              nameof(VerticalPadding),
              o => o.VerticalPadding,
              (o, v) => o.VerticalPadding = v, 10.0);
       
        public static readonly DirectProperty<TextContainer, double> HorizontalPaddingProperty =
         AvaloniaProperty.RegisterDirect<TextContainer, double>(
             nameof(HorizontalPadding),
             o => o.HorizontalPadding,
             (o, v) => o.HorizontalPadding = v, 10.0);

        public static readonly DirectProperty<TextContainer, string> TextProperty =
         AvaloniaProperty.RegisterDirect<TextContainer, string>(
             nameof(Text),
             o => o.Text,
             (o, v) => o.Text = v, "");
        public TextContainer()
        {
            
        }

        public double VerticalPadding { get; set; } = 5.0;
        public double HorizontalPadding { get; set; } = 5.0;
        public string Text { get; set; }

        public override void Render(DrawingContext context)
        {
            FormattedText text = new FormattedText();
            text.Text = Text;
            text.Typeface = new Typeface(new FontFamily("Ariel"), 18);
            double penThickness = 2;
            var width = text.Bounds.Width + penThickness;
            var height = text.Bounds.Height + penThickness;

            var Pen = new Pen(Brushes.Black, penThickness);
            var topLeftContainer = new Point(penThickness, penThickness);
            var bottomRightContainer = new Point(penThickness + width + 2 * HorizontalPadding, penThickness + height + 2 * VerticalPadding);
            Rect rect = new Rect(topLeftContainer, bottomRightContainer);

            context.DrawRectangle(Pen, rect, 10);
            context.DrawText(Brushes.CadetBlue, new Point(HorizontalPadding + penThickness, VerticalPadding + penThickness), text);

            this.Width = rect.Width + 2 * penThickness;
            this.Height = rect.Height + 2 * penThickness;
            base.Render(context);
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
