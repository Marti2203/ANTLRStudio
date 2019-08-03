using System.Collections.Generic;


namespace ANTLRStudio.TreeLayout.Interfaces
{
    public interface ITreeForTreeLayout<TreeNode>
    {

        /**
         * Returns the the root of the tree.
         * <p>
         * Time Complexity: O(1)
         * 
         * @return the root of the tree
         */
        TreeNode Root { get; }

        /**
         * Tells if a node is a leaf in the tree.
         * <p>
         * Time Complexity: O(1)
         * 
         * @param node &nbsp;
         * @return true iff node is a leaf in the tree, i.e. has no children.
         */
        bool IsLeaf(TreeNode node);

        /**
         * Tells if a node is a child of a given parentNode.
         * <p>
         * Time Complexity: O(1)
         * 
         * @param node &nbsp;
         * @param parentNode &nbsp;
         * @return true iff the node is a child of the given parentNode
         */
        bool IsChildOfParent(TreeNode node, TreeNode parentNode);

        /**
         * Returns the children of a parent node.
         * <p>
         * Time Complexity: O(1)
         * 
         * @param parentNode
         *            [!isLeaf(parentNode)]
         * @return the children of the given parentNode, from first to last
         */
        IEnumerable<TreeNode> Children(TreeNode parentNode);

        /**
         * Returns the children of a parent node, in reverse order.
         * <p>
         * Time Complexity: O(1)
         * 
         * @param parentNode
         *            [!isLeaf(parentNode)]
         * @return the children of given parentNode, from last to first
         */
        IEnumerable<TreeNode> ChildrenReversed(TreeNode parentNode);

        /**
         * Returns the first child of a parent node.
         * <p>
         * Time Complexity: O(1)
         * 
         * @param parentNode
         *            [!isLeaf(parentNode)]
         * @return the first child of the parentNode
         */
        TreeNode FirstChild(TreeNode parentNode);

        /**
         * Returns the last child of a parent node.
         * <p>
         * 
         * Time Complexity: O(1)
         * 
         * @param parentNode
         *            [!isLeaf(parentNode)]
         * @return the last child of the parentNode
         */
        TreeNode LastChild(TreeNode parentNode);
    }
}
