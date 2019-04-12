namespace ANTLRStudio.TreeLayout.Interfaces
{
    /**
     * Provides the extent (width and height) of a tree node.
     * <p>
     * Also see <a href="package-summary.html">this overview</a>.
     * 
     * @author Udo Borkowski (ub@abego.org)
     * 
     * @param <TreeNode> Type of elements used as nodes in the tree
     */
    public interface INodeExtentProvider<TreeNode>
    {
        /**
         * Returns the width of the given treeNode.
         * 
         * @param treeNode  &nbsp;
         * @return [result &gt;= 0]
         */
        float Width(TreeNode treeNode);

        /**
         * Returns the height of the given treeNode.
         * 
         * @param treeNode &nbsp;
         * @return [result &gt;= 0]
         */
        float Height(TreeNode treeNode);
    }
}
