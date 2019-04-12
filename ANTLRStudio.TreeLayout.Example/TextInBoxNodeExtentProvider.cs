using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANTLRStudio.TreeLayout.Interfaces;

namespace ANTLRStudio.TreeLayout.Example
{
    /**
     * A {@link NodeExtentProvider} for nodes of type {@link TextInBox}.
     * <p>
     * As one would expect this NodeExtentProvider returns the width and height as
     * specified with each TextInBox.
     * 
     * @author Udo Borkowski (ub@abego.org)
     */
    public class TextInBoxNodeExtentProvider :
            INodeExtentProvider<TextInBox>
    {

        public float Width(TextInBox treeNode) => treeNode.Width;


        public float Height(TextInBox treeNode) => treeNode.Height;

    }
}

