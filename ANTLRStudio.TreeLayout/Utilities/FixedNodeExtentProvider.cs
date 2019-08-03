
using ANTLRStudio.TreeLayout.Interfaces;

namespace ANTLRStudio.TreeLayout.Utilities
{
    /**
     * A {@link NodeExtentProvider} returning the same width and height for each
     * node.
     * 
     * @author Udo Borkowski (ub@abego.org)
     * 
     * @param <T> Type of elements used as nodes in the tree
     */
    public class FixedNodeExtentProvider<T> : INodeExtentProvider<T>
    {


        private readonly float width;
        private readonly float height;

        /**
         * Specifies the constants to be used as the width and height of the nodes.
         * 
         * @param width
         *            [default=0]
         * 
         * @param height
         *            [default=0]
         */
        public FixedNodeExtentProvider(float width, float height)
        {

            this.width = width;
            this.height = height;
        }

        /**
         * see {@link #FixedNodeExtentProvider(float, float)}
         */
        public FixedNodeExtentProvider() : this(0, 0)
        {
        }

        public float Width(T treeNode) => width;


        public float Height(T treeNode) => height;

    }
}
