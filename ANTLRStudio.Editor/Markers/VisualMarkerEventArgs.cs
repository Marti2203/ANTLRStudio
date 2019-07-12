using Eto.Forms;
namespace ANTLRStudio.Editor.Markers
{
    public class VisualMarkerEventArgs : MouseEventArgs
    {
        public Style Style { get; private set; }
        public StyleVisualMarker Marker { get; private set; }

        public VisualMarkerEventArgs(Style style, StyleVisualMarker marker, MouseEventArgs args)
            : base(args.Buttons, args.Modifiers, args.Location, args.Delta, args.Pressure)
        {
            Style = style;
            Marker = marker;
        }
    }
}
