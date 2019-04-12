using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using ANTLRStudio.TreeLayout.Interfaces;

namespace ANTLRStudio.TreeLayout.Example
{
    /**
     * A Component displaying a tree of TextInBoxes, given by a {@link TreeLayout}.
     * 
     * @author Udo Borkowski (ub@abego.org)
     */
    public class TextInBoxTreePane : Drawable
    {
        readonly TreeLayout<TextInBox> treeLayout;

        private ITreeForTreeLayout<TextInBox> GetTree() => treeLayout.Tree;

        private IEnumerable<TextInBox> ChildrenOf(TextInBox parent) => GetTree().Children(parent);

        private RectangleF BoundsOfNode(TextInBox node) => treeLayout.NodeBounds[node];


        /**
         * Specifies the tree to be displayed by passing in a {@link TreeLayout} for
         * that tree.
         * 
         * @param treeLayout the {@link TreeLayout} to be displayed
         */
        public TextInBoxTreePane(TreeLayout<TextInBox> treeLayout)
        {
            this.treeLayout = treeLayout;

            SizeF size = treeLayout.Bounds.Size;
            //setPreferredSize(size);
        }

        // -------------------------------------------------------------------
        // painting

        private readonly static int ARC_SIZE = 10;
        private readonly static Color BOX_COLOR = Colors.Orange;
        private readonly static Color BORDER_COLOR = Colors.Gray;
        private readonly static Color TEXT_COLOR = Colors.Black;

        private void PaintEdges(Graphics g, TextInBox parent)
        {
            if (!GetTree().IsLeaf(parent))
            {
                RectangleF b1 = BoundsOfNode(parent);
                double x1 = b1.Left + b1.Width / 2;
                double y1 = b1.Top + b1.Height / 2;
                foreach (TextInBox child in ChildrenOf(parent))
                {
                    RectangleF b2 = BoundsOfNode(child);

                    g.DrawLine(new Pen(BORDER_COLOR), (int)x1, (int)y1, (int)(b2.Left + b2.Width / 2),
                            (int)(b2.Top + b2.Height / 2));

                    PaintEdges(g, child);
                }
            }
        }

        private void PaintBox(Graphics g, TextInBox textInBox)
        {
            // draw the box in the background

            // g.setColor(BOX_COLOR);
            RectangleF box = BoundsOfNode(textInBox);
            g.FillRectangle(new SolidBrush(BOX_COLOR), (int)box.Left, (int)box.Top, (int)box.Width - 1,
                    (int)box.Height - 1);
            g.DrawRectangle(new Pen(BORDER_COLOR), (int)box.Left, (int)box.Top, (int)box.Width - 1,
                    (int)box.Height - 1);

            // draw the text on top of the box (possibly multiple lines)
            // g.setColor(TEXT_COLOR);
            string[] lines = textInBox.Text.Split('\n');
            // FontMetrics m = getFontMetrics(getFont());
            // int x = (int)box.x + ARC_SIZE / 2;
            //  int y = (int)box.y + m.getAscent() + m.getLeading() + 1;
            for (int i = 0; i < lines.Length; i++)
            {
                g.DrawText(new Font("Arial", ARC_SIZE), new SolidBrush(TEXT_COLOR), box.TopLeft, lines[i]);
                // y += m.getHeight();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            PaintEdges(g, GetTree().Root);

            // paint the boxes
            foreach (TextInBox textInBox in treeLayout.NodeBounds.Keys)
            {
                PaintBox(g, textInBox);
            }
        }
    }
}
