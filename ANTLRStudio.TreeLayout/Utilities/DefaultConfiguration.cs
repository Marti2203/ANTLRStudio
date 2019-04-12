

namespace ANTLRStudio.TreeLayout.Utilities
{
    /**
     * Specify a {@link Configuration} through configurable parameters, or falling
     * back to some frequently used defaults.
     * 
     * @author Udo Borkowski (ub@abego.org)
     * 
     * 
     * @param <TreeNode> Type of elements used as nodes in the tree
     */
    public class DefaultConfiguration<TreeNode> :
            Configuration<TreeNode>
    {

        /**
         * Specifies the constants to be used for this Configuration.
         * 
         * @param gapBetweenLevels &nbsp;
         * @param gapBetweenNodes &nbsp;
         * @param location
         *            [default: {@link org.abego.treelayout.Configuration.Location#Top Top}]
         * @param alignmentInLevel
         *            [default: {@link org.abego.treelayout.Configuration.AlignmentInLevel#Center Center}]
         */
        public DefaultConfiguration(float gapBetweenLevels,
                float gapBetweenNodes, Location location = Location.Top,
                AlignmentInLevel alignmentInLevel = AlignmentInLevel.Center)
        {

            this.gapBetweenLevels = gapBetweenLevels;
            this.gapBetweenNodes = gapBetweenNodes;
            RootLocation = location;
            Alignment = alignmentInLevel;
        }


        // -----------------------------------------------------------------------
        // gapBetweenLevels

        private readonly float gapBetweenLevels;


        public override float GapBetweenLevels(int nextLevel)
        {
            return gapBetweenLevels;
        }

        // -----------------------------------------------------------------------
        // gapBetweenNodes

        private readonly float gapBetweenNodes;


        public override float GapBetweenNodes(TreeNode nodeOne, TreeNode nodeTwo)
        {
            return gapBetweenNodes;
        }



    }
}
