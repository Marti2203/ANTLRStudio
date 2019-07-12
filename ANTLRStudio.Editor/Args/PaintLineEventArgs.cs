using System;
using Eto.Drawing;
using Eto.Forms;

namespace ANTLRStudio.Editor.Args
{
    public class PaintLineEventArgs : PaintEventArgs
    {
        public PaintLineEventArgs(int iLine, Rectangle rect, Graphics gr, Rectangle clipRect) : base(gr, clipRect)
        {
            LineIndex = iLine;
            LineRect = rect;
        }

        public int LineIndex { get; private set; }
        public Rectangle LineRect { get; private set; }
    }

}
