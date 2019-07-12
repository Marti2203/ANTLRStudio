using Eto.Drawing;
namespace ANTLRStudio.Editor.Markers
{
    public class FoldedAreaMarker : VisualMarker
    {
        public readonly int iLine;

        public FoldedAreaMarker(int iLine, Rectangle rectangle)
            : base(rectangle)
        {
            this.iLine = iLine;
        }

        public override void Draw(Graphics gr, Pen pen)
        {
            gr.DrawRectangle(pen, rectangle);
        }
    }
}
