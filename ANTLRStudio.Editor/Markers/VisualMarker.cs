using Eto.Drawing;
using Eto.Forms;
namespace ANTLRStudio.Editor.Markers
{
    public class VisualMarker
    {
        public readonly Rectangle rectangle;

        public VisualMarker(Rectangle rectangle)
        {
            this.rectangle = rectangle;
        }

        public virtual void Draw(Graphics gr, Pen pen)
        {
        }

        public virtual Cursor Cursor => Cursors.Arrow;
    }
}
