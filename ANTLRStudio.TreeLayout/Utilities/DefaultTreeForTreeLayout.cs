using System;
using System.Collections.Generic;

namespace ANTLRStudio.TreeLayout.Utilities
{
    /**
     * Provides a generic implementation for the {@link org.abego.treelayout.TreeForTreeLayout}
     * interface, applicable to any type of tree node.
     * <p>
     * It allows you to create a tree "from scratch", without creating any new
     * class.
     * <p>
     * To create a tree you must provide the root of the tree (see
     * {@link #DefaultTreeForTreeLayout(Object)}. Then you can incrementally
     * construct the tree by adding children to the root or other nodes of the tree
     * (see {@link #addChild(Object, Object)} and
     * {@link #addChildren(Object, Object...)}).
     * 
     * @author Udo Borkowski (ub@abego.org)
     * 
     * @param <TreeNode> Type of elements used as nodes in the tree
     */
    public class DefaultTreeForTreeLayout<TreeNode> :
            AbstractTreeForTreeLayout<TreeNode>
    {

        Dictionary<TreeNode, List<TreeNode>> childrenMap = new Dictionary<TreeNode, List<TreeNode>>();
        Dictionary<TreeNode, TreeNode> parents = new Dictionary<TreeNode, TreeNode>();

        /**
         * Creates a new instance with a given node as the root
         * 
         * @param root
         *            the node to be used as the root.
         */
        public DefaultTreeForTreeLayout(TreeNode root) : base(root)
        {
        }

        public override TreeNode Parent(TreeNode node)
        {
            try
            {
                parents.TryGetValue(node, out TreeNode parent);
                return parent;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e);
                return default(TreeNode);
            }

        }

        public override List<TreeNode> ChildrenList(TreeNode node)
        => childrenMap.TryGetValue(node, out List<TreeNode> result) ? result : new List<TreeNode>();


        /**
         * 
         * @param node &nbsp;
         * @return true iff the node is in the tree
         */
        public bool HasNode(TreeNode node) => ReferenceEquals(node, Root) || parents.ContainsKey(node);


        /**
         * @param parentNode
         *            [hasNode(parentNode)]
         * @param node
         *            [!hasNode(node)]
         */
        public void Add(TreeNode parentNode, TreeNode node)
        {
            if (!childrenMap.ContainsKey(parentNode))
            {
                List<TreeNode> list = new List<TreeNode>();
                childrenMap.Add(parentNode, list);
            }
            childrenMap[parentNode].Add(node);
            parents.Add(node, parentNode);
        }

        public void Add(TreeNode parentNode, params TreeNode[] nodes)
        {
            foreach (var node in nodes)
            {
                Add(parentNode, node);
            }
        }

    }
}
