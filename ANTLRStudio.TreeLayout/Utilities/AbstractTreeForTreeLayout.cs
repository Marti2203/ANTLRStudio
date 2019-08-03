using System.Collections.Generic;
using System.Linq;
using ANTLRStudio.TreeLayout.Interfaces;
namespace ANTLRStudio.TreeLayout.Utilities
{
    /**
     * Provides an easy way to implement the {@link org.abego.treelayout.TreeForTreeLayout} interface by
     * defining just two simple methods and a constructor.
     * <p>
     * To use this class the underlying tree must provide the children as a list
     * (see {@link #getChildrenList(Object)} and give direct access to the parent of
     * a node (see {@link #getParent(Object)}).
     * <p>
     * 
     * See also {@link DefaultTreeForTreeLayout}.
     * 
     * @author Udo Borkowski (ub@abego.org)
     * 
     * @param <TreeNode> Type of elements used as nodes in the tree
     */
    abstract public class AbstractTreeForTreeLayout<TreeNode> :
            ITreeForTreeLayout<TreeNode>
    {

        /**
         * Returns the parent of a node, if it has one.
         * <p>
         * Time Complexity: O(1)
         * 
         * @param node &nbsp;
         * @return [nullable] the parent of the node, or null when the node is a
         *         root.
         */
        abstract public TreeNode Parent(TreeNode node);

        /**
         * Return the children of a node as a {@link List}.
         * <p>
         * Time Complexity: O(1)
         * <p>
         * Also the access to an item of the list must have time complexity O(1).
         * <p>
         * A client must not modify the returned list.
         * 
         * @param node &nbsp;
         * @return the children of the given node. When node is a leaf the list is
         *         empty.
         */
        abstract public List<TreeNode> ChildrenList(TreeNode node);

        protected AbstractTreeForTreeLayout(TreeNode root)
        {
            Root = root;
        }

        public TreeNode Root { get; }

        public bool IsLeaf(TreeNode node) => ChildrenList(node).Count == 0;

        public bool IsChildOfParent(TreeNode node, TreeNode parentNode) => ReferenceEquals(Parent(node), parentNode);

        public IEnumerable<TreeNode> Children(TreeNode node) => ChildrenList(node);

        public IEnumerable<TreeNode> ChildrenReversed(TreeNode node)
        {
            var children = ChildrenList(node);
            children.Reverse();
            return children;
        }

        public TreeNode FirstChild(TreeNode parentNode) => ChildrenList(parentNode)[0];

        public TreeNode LastChild(TreeNode parentNode) => ChildrenList(parentNode).Last();
    }
}
