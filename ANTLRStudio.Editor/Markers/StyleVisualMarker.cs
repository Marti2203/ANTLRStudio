using Eto.Drawing;
namespace ANTLRStudio.Editor.Markers
{
    public class StyleVisualMarker : VisualMarker
    {
        public Style Style { get; private set; }

        public StyleVisualMarker(Rectangle rectangle, Style style)
            : base(rectangle)
        {
            Style = style;
        }
    }
}
