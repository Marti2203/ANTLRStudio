using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ANTLRStudio.TreeLayout.Interfaces;
using ANTLRStudio.TreeLayout.Utilities.Internal;
using System.Linq;
using Avalonia;

namespace ANTLRStudio.TreeLayout
{
    /**
     * Implements the actual tree layout algorithm.
     * <p>
     * The nodes with their readonly layout can be retrieved through
     * {@link #getNodeBounds()}.
     * </p>
     * See <a href="package-summary.html">this summary</a> to get an overview how to
     * use TreeLayout.
     * 
     * 
     * @author Udo Borkowski (ub@abego.org)
     *  
     * @param <TreeNode> Type of elements used as nodes in the tree
     */
    public class TreeLayout<TreeNode> where TreeNode : class
    {
        /**
         * Returns the Tree the layout is created for.
         * 
         * @return the Tree the layout is created for
         */
        public ITreeForTreeLayout<TreeNode> Tree { get; }

        /**
         * Returns the {@link NodeExtentProvider} used by this {@link TreeLayout}.
         * 
         * @return the {@link NodeExtentProvider} used by this {@link TreeLayout}
         */
        public INodeExtentProvider<TreeNode> NodeExtentProvider { get; }

        private float NodeHeight(TreeNode node) => NodeExtentProvider.Height(node);

        private float NodeWidth(TreeNode node) => NodeExtentProvider.Width(node);

        private float WidthOrHeightOfNode(TreeNode treeNode, bool returnWidth)
        => returnWidth ? NodeWidth(treeNode) : NodeHeight(treeNode);

        /**
         * When the level changes in Y-axis (i.e. root location Top or Bottom) the
         * height of a node is its thickness, otherwise the node's width is its
         * thickness.
         * <p>
         * The thickness of a node is used when calculating the locations of the
         * levels.
         * 
         * @param treeNode
         * @return
         */
        private float NodeThickness(TreeNode treeNode)
        => WidthOrHeightOfNode(treeNode, !IsLevelChangeInYAxis);


        /**
         * When the level changes in Y-axis (i.e. root location Top or Bottom) the
         * width of a node is its size, otherwise the node's height is its size.
         * <p>
         * The size of a node is used when calculating the distance between two
         * nodes.
         * 
         * @param treeNode
         * @return
         */
        private float NodeSize(TreeNode treeNode)
        => WidthOrHeightOfNode(treeNode, IsLevelChangeInYAxis);


        /**
         * Returns the Configuration used by this {@link TreeLayout}.
         * 
         * @return the Configuration used by this {@link TreeLayout}
         */
        public Configuration<TreeNode> Configuration { get; }

        private bool IsLevelChangeInYAxis
        => Configuration.RootLocation == Configuration<TreeNode>.Location.Top
            || Configuration.RootLocation == Configuration<TreeNode>.Location.Bottom;


        private int LevelChangeSign
        {
            get
            {
                Configuration<TreeNode>.Location rootLocation = Configuration.RootLocation;
                return rootLocation == Configuration<TreeNode>.Location.Bottom
                        || rootLocation == Configuration<TreeNode>.Location.Right ? -1 : 1;
            }
        }

        // ------------------------------------------------------------------------
        // bounds

        private float boundsLeft = float.MaxValue;
        private float boundsRight = float.MinValue;
        private float boundsTop = float.MaxValue;
        private float boundsBottom = float.MinValue;

        private void UpdateBounds(TreeNode node, float centerX, float centerY)
        {
            float width = NodeWidth(node);
            float height = NodeHeight(node);
            float left = centerX - width / 2;
            float right = centerX + width / 2;
            float top = centerY - height / 2;
            float bottom = centerY + height / 2;
            if (boundsLeft > left)
            {
                boundsLeft = left;
            }
            if (boundsRight < right)
            {
                boundsRight = right;
            }
            if (boundsTop > top)
            {
                boundsTop = top;
            }
            if (boundsBottom < bottom)
            {
                boundsBottom = bottom;
            }
        }


        /**
         * Returns the bounds of the tree layout.
         * <p>
         * The bounds of a TreeLayout is the smallest rectangle containing the
         * bounds of all nodes in the layout. It always starts at (0,0).
         * 
         * @return the bounds of the tree layout
         */
        public Rect Bounds => new Rect(0, 0, boundsRight - boundsLeft, boundsBottom - boundsTop);


        // ------------------------------------------------------------------------
        // size of level

        private readonly List<float> sizeOfLevel = new List<float>();

        private void CalcSizeOfLevels(TreeNode node, int level)
        {
            float oldSize;
            if (sizeOfLevel.Count <= level)
            {
                sizeOfLevel.Add(0);
                oldSize = 0;
            }
            else
            {
                oldSize = sizeOfLevel[level];
            }

            float size = NodeThickness(node);
            // size = nodeExtentProvider.getHeight(node);
            if (oldSize < size)
            {
                sizeOfLevel[level] = size;
            }

            if (!Tree.IsLeaf(node))
            {
                foreach (TreeNode child in Tree.Children(node))
                {
                    CalcSizeOfLevels(child, level + 1);
                }
            }
        }

        /**
         * Returns the number of levels of the tree.
         * 
         * @return [level &gt; 0]
         */
        public int LevelCount => sizeOfLevel.Count;

        /**
         * Returns the size of a level.
         * <p>
         * When the root is located at the top or bottom the size of a level is the
         * maximal height of the nodes of that level. When the root is located at
         * the left or right the size of a level is the maximal width of the nodes
         * of that level.
         * 
         * @param level &nbsp;
         * @return the size of the level [level &gt;= 0 &amp;&amp; level &lt; levelCount]
         */
        public float SizeOfLevel(int level) => sizeOfLevel[level];

        // ------------------------------------------------------------------------
        // NormalizedPosition

        /**
         * The algorithm calculates the position starting with the root at 0. I.e.
         * the left children will get negative positions. However we want the result
         * to be normalized to (0,0).
         * <p>
         * {@link NormalizedPosition} will normalize the position (given relative to
         * the root position), taking the current bounds into account. This way the
         * left most node bounds will start at x = 0, the top most node bounds at y
         * = 0.
         */
        private class NormalizedPosition
        {

            private TreeLayout<TreeNode> treeLayout;
            private float x_relativeToRoot;
            private float y_relativeToRoot;

            public NormalizedPosition(TreeLayout<TreeNode> treeLayout, float x_relativeToRoot, float y_relativeToRoot)
            {
                SetLocation(x_relativeToRoot, y_relativeToRoot);
                this.treeLayout = treeLayout;
            }

            public float X => x_relativeToRoot - treeLayout.boundsLeft;


            public float Y => y_relativeToRoot - treeLayout.boundsTop;

            // never called from outside
            public void SetLocation(float x_relativeToRoot, float y_relativeToRoot)
            {
                this.x_relativeToRoot = x_relativeToRoot;
                this.y_relativeToRoot = y_relativeToRoot;
            }
        }

        // ------------------------------------------------------------------------
        // The Algorithm

        private readonly Dictionary<TreeNode, float> mod = new Dictionary<TreeNode, float>();
        private readonly Dictionary<TreeNode, TreeNode> thread = new Dictionary<TreeNode, TreeNode>();
        private readonly Dictionary<TreeNode, float> prelim = new Dictionary<TreeNode, float>();
        private readonly Dictionary<TreeNode, float> change = new Dictionary<TreeNode, float>();
        private readonly Dictionary<TreeNode, float> shift = new Dictionary<TreeNode, float>();
        private readonly Dictionary<TreeNode, TreeNode> ancestor1 = new Dictionary<TreeNode, TreeNode>();
        private readonly Dictionary<TreeNode, int> number = new Dictionary<TreeNode, int>();
        private readonly Dictionary<TreeNode, NormalizedPosition> positions = new Dictionary<TreeNode, NormalizedPosition>();

        private float GetMod(TreeNode node) => mod.ContainsKey(node) ? mod[node] : 0;


        private void SetMod(TreeNode node, float d)
        {
            if (mod.ContainsKey(node))
            {
                mod[node] = d;
            }
            else
            {
                mod.Add(node, d);
            }

        }

        private TreeNode GetThread(TreeNode node)
        => thread.ContainsKey(node) ? thread[node] : default(TreeNode);

        private void SetThread(TreeNode node, TreeNode thread)
        {
            if (this.thread.ContainsKey(node))
            {
                this.thread[node] = thread;
            }
            else
            {
                this.thread.Add(node, thread);
            }

        }

        private TreeNode GetAncestor(TreeNode node)
        => ancestor1.ContainsKey(node) ? ancestor1[node] : default(TreeNode);


        private void SetAncestor(TreeNode node, TreeNode ancestor)
        {
            if (ancestor1.ContainsKey(node))
            {
                ancestor1[node] = ancestor;
            }
            else
            {
                ancestor1.Add(node, ancestor);
            }
        }

        private float GetPrelim(TreeNode node)
        => prelim.ContainsKey(node) ? prelim[node] : 0;


        private void SetPrelim(TreeNode node, float d)
        {
            if (prelim.ContainsKey(node))
            {
                prelim[node] = d;
            }
            else
            {
                prelim.Add(node, d);
            }

        }

        private float GetChange(TreeNode node) => change.ContainsKey(node) ? change[node] : 0;


        private void SetChange(TreeNode node, float d)
        {
            if (change.ContainsKey(node))
            {
                change[node] = d;
            }
            else
            {
                change.Add(node, d);
            }
        }

        private float GetShift(TreeNode node) => shift.ContainsKey(node) ? shift[node] : 0;


        private void SetShift(TreeNode node, float d)
        {
            if (shift.ContainsKey(node))
            {
                shift[node] = d;
            }
            else
            {
                shift.Add(node, d);
            }

        }

        /**
         * The distance of two nodes is the distance of the centers of both noded.
         * <p>
         * I.e. the distance includes the gap between the nodes and half of the
         * sizes of the nodes.
         * 
         * @param v
         * @param w
         * @return the distance between node v and w
         */
        private float GetDistance(TreeNode v, TreeNode w)
        => (NodeSize(v) + NodeSize(w)) / 2 + Configuration.GapBetweenNodes(v, w);

        private TreeNode NextLeft(TreeNode v) => Tree.IsLeaf(v) ? GetThread(v) : Tree.FirstChild(v);

        private TreeNode NextRight(TreeNode v) => Tree.IsLeaf(v) ? GetThread(v) : Tree.LastChild(v);

        /**
         * 
         * @param node
         *            [tree.isChildOfParent(node, parentNode)]
         * @param parentNode
         *            parent of node
         * @return
         */
        private int Number(TreeNode node, TreeNode parentNode)
        {
            if (!shift.ContainsKey(node))
            {
                int i = 1;
                foreach (TreeNode child in Tree.Children(parentNode))
                {
                    if (number.ContainsKey(child))
                    {
                        number[child] = i++;
                    }
                    else
                    {
                        number.Add(child, i++);
                    }
                }


            }
            return number[node];
        }

        /**
         * 
         * @param vIMinus
         * @param v
         * @param parentOfV
         * @param defaultAncestor
         * @return the greatest distinct ancestor of vIMinus and its right neighbor
         *         v
         */
        private TreeNode Ancestor(TreeNode vIMinus, TreeNode v, TreeNode parentOfV,
                TreeNode defaultAncestor)
        {
            TreeNode ancestor = GetAncestor(vIMinus);

            // when the ancestor of vIMinus is a sibling of v (i.e. has the same
            // parent as v) it is also the greatest distinct ancestor vIMinus and
            // v. Otherwise it is the defaultAncestor

            return Tree.IsChildOfParent(ancestor, parentOfV) ? ancestor : defaultAncestor;
        }

        private void MoveSubtree(TreeNode wMinus, TreeNode wPlus, TreeNode parent, float shift)
        {
            int subtrees = Number(wPlus, parent) - Number(wMinus, parent);
            SetChange(wPlus, GetChange(wPlus) - shift / subtrees);
            SetShift(wPlus, GetShift(wPlus) + shift);
            SetChange(wMinus, GetChange(wMinus) + shift / subtrees);
            SetPrelim(wPlus, GetPrelim(wPlus) + shift);
            SetMod(wPlus, GetMod(wPlus) + shift);
        }

        /**
         * In difference to the original algorithm we also pass in the leftSibling
         * and the parent of v.
         * <p>
         * <b>Why adding the parameter 'parent of v' (parentOfV) ?</b>
         * <p>
         * In this method we need access to the parent of v. Not every tree
         * implementation may support efficient (i.e. constant time) access to it.
         * On the other hand the (only) caller of this method can provide this
         * information with only constant extra time.
         * <p>
         * Also we need access to the "left most sibling" of v. Not every tree
         * implementation may support efficient (i.e. constant time) access to it.
         * On the other hand the "left most sibling" of v is also the "first child"
         * of the parent of v. The first child of a parent node we can get in
         * constant time. As we got the parent of v we can so also get the
         * "left most sibling" of v in constant time.
         * <p>
         * <b>Why adding the parameter 'leftSibling' ?</b>
         * <p>
         * In this method we need access to the "left sibling" of v. Not every tree
         * implementation may support efficient (i.e. constant time) access to it.
         * However it is easy for the caller of this method to provide this
         * information with only constant extra time.
         * <p>
         * <p>
         * <p>
         * In addition these extra parameters avoid the need for
         * {@link TreeForTreeLayout} to include extra methods "getParent",
         * "getLeftSibling", or "getLeftMostSibling". This keeps the interface
         * {@link TreeForTreeLayout} small and avoids redundant implementations.
         * 
         * @param v
         * @param defaultAncestor
         * @param leftSibling
         *            [nullable] the left sibling v, if there is any
         * @param parentOfV
         *            the parent of v
         * @return the (possibly changes) defaultAncestor
         */
        private TreeNode Apportion(TreeNode v, TreeNode defaultAncestor,
                TreeNode leftSibling, TreeNode parentOfV)
        {
            TreeNode w = leftSibling;
            if (w == null)
            {
                // v has no left sibling
                return defaultAncestor;
            }
            // v has left sibling w

            // The following variables "v..." are used to traverse the contours to
            // the subtrees. "Minus" refers to the left, "Plus" to the right
            // subtree. "I" refers to the "inside" and "O" to the outside contour.
            TreeNode vOPlus = v;
            TreeNode vIPlus = v;
            TreeNode vIMinus = w;
            // get leftmost sibling of vIPlus, i.e. get the leftmost sibling of
            // v, i.e. the leftmost child of the parent of v (which is passed
            // in)
            TreeNode vOMinus = Tree.FirstChild(parentOfV);

            float sIPlus = GetMod(vIPlus);
            float sOPlus = GetMod(vOPlus);
            float sIMinus = GetMod(vIMinus);
            float sOMinus = GetMod(vOMinus);

            TreeNode nextRightVIMinus = NextRight(vIMinus);
            TreeNode nextLeftVIPlus = NextLeft(vIPlus);

            while (nextRightVIMinus != null && nextLeftVIPlus != null)
            {
                vIMinus = nextRightVIMinus;
                vIPlus = nextLeftVIPlus;
                vOMinus = NextLeft(vOMinus);
                vOPlus = NextRight(vOPlus);
                SetAncestor(vOPlus, v);
                float shift = GetPrelim(vIMinus) + sIMinus
                        - (GetPrelim(vIPlus) + sIPlus)
                        + GetDistance(vIMinus, vIPlus);

                if (shift > 0)
                {
                    MoveSubtree(Ancestor(vIMinus, v, parentOfV, defaultAncestor),
                            v, parentOfV, shift);
                    sIPlus = sIPlus + shift;
                    sOPlus = sOPlus + shift;
                }
                sIMinus = sIMinus + GetMod(vIMinus);
                sIPlus = sIPlus + GetMod(vIPlus);
                sOMinus = sOMinus + GetMod(vOMinus);
                sOPlus = sOPlus + GetMod(vOPlus);

                nextRightVIMinus = NextRight(vIMinus);
                nextLeftVIPlus = NextLeft(vIPlus);
            }

            if (nextRightVIMinus != null && NextRight(vOPlus) == null)
            {
                SetThread(vOPlus, nextRightVIMinus);
                SetMod(vOPlus, GetMod(vOPlus) + sIMinus - sOPlus);
            }

            if (nextLeftVIPlus != null && NextLeft(vOMinus) == null)
            {
                SetThread(vOMinus, nextLeftVIPlus);
                SetMod(vOMinus, GetMod(vOMinus) + sIPlus - sOMinus);
                defaultAncestor = v;
            }
            return defaultAncestor;
        }

        /**
         * 
         * @param v
         *            [!tree.isLeaf(v)]
         */
        private void ExecuteShifts(TreeNode v)
        {
            float shift = 0;
            float change = 0;
            foreach (TreeNode w in Tree.ChildrenReversed(v))
            {
                change = change + GetChange(w);
                SetPrelim(w, GetPrelim(w) + shift);
                SetMod(w, GetMod(w) + shift);
                shift = shift + GetShift(w) + change;
            }
        }

        /**
         * In difference to the original algorithm we also pass in the leftSibling
         * (see {@link #apportion(Object, Object, Object, Object)} for a
         * motivation).
         * 
         * @param v
         * @param leftSibling
         *            [nullable] the left sibling v, if there is any
         */
        private void FirstWalk(TreeNode v, TreeNode leftSibling)
        {
            if (Tree.IsLeaf(v))
            {
                // No need to set prelim(v) to 0 as the getter takes care of this.

                TreeNode w = leftSibling;
                if (w != null)
                {
                    // v has left sibling

                    SetPrelim(v, GetPrelim(w) + GetDistance(v, w));
                }

            }
            else
            {
                // v is not a leaf

                TreeNode defaultAncestor = Tree.FirstChild(v);
                TreeNode previousChild = default(TreeNode);
                foreach (TreeNode w1 in Tree.Children(v))
                {
                    FirstWalk(w1, previousChild);
                    defaultAncestor = Apportion(w1, defaultAncestor, previousChild,
                            v);
                    previousChild = w1;
                }
                ExecuteShifts(v);
                float midpoint = (GetPrelim(Tree.FirstChild(v)) + GetPrelim(Tree.LastChild(v))) / 2;
                TreeNode w = leftSibling;
                if (w != null)
                {
                    // v has left sibling

                    SetPrelim(v, GetPrelim(w) + GetDistance(v, w));
                    SetMod(v, GetPrelim(v) - midpoint);

                }
                else
                {
                    // v has no left sibling

                    SetPrelim(v, midpoint);
                }
            }
        }

        /**
         * In difference to the original algorithm we also pass in extra level
         * information.
         * 
         * @param v
         * @param m
         * @param level
         * @param levelStart
         */
        private void SecondWalk(TreeNode v, float m, int level, float levelStart)
        {
            // construct the position from the prelim and the level information

            // The rootLocation affects the way how x and y are changed and in what
            // direction.
            float levelChangeSign = LevelChangeSign;
            bool levelChangeOnYAxis = IsLevelChangeInYAxis;
            float levelSize = SizeOfLevel(level);

            float x = GetPrelim(v) + m;

            float y;
            Configuration<TreeNode>.AlignmentInLevel alignment = Configuration.Alignment;
            if (alignment == Configuration<TreeNode>.AlignmentInLevel.Center)
            {
                y = levelStart + levelChangeSign * (levelSize / 2);
            }
            else if (alignment == Configuration<TreeNode>.AlignmentInLevel.TowardsRoot)
            {
                y = levelStart + levelChangeSign * (NodeThickness(v) / 2);
            }
            else
            {
                y = levelStart + levelSize - levelChangeSign
                        * (NodeThickness(v) / 2);
            }

            if (!levelChangeOnYAxis)
            {
                float t = x;
                x = y;
                y = t;
            }

            positions.Add(v, new NormalizedPosition(this, x, y));

            // update the bounds
            UpdateBounds(v, x, y);

            // recurse
            if (!Tree.IsLeaf(v))
            {
                float nextLevelStart = levelStart
                        + (levelSize + Configuration.GapBetweenLevels(level + 1))
                        * levelChangeSign;
                foreach (TreeNode w in Tree.Children(v))
                {
                    SecondWalk(w, m + GetMod(v), level + 1, nextLevelStart);
                }
            }
        }

        // ------------------------------------------------------------------------
        // nodeBounds

        private Dictionary<TreeNode, Rect> nodeBounds;

        /**
         * Returns the layout of the tree nodes by mapping each node of the tree to
         * its bounds (position and size).
         * <p>
         * For each rectangle x and y will be &gt;= 0. At least one rectangle will have
         * an x == 0 and at least one rectangle will have an y == 0.
         * 
         * @return maps each node of the tree to its bounds (position and size).
         */
        public Dictionary<TreeNode, Rect> NodeBounds
        {
            get
            {
                if (nodeBounds == null)
                {
                    nodeBounds = positions.Keys.ToDictionary(treeNode => treeNode, treeNode =>
                    {
                        NormalizedPosition pos = positions[treeNode];
                        float w = NodeWidth(treeNode);
                        float h = NodeHeight(treeNode);
                        float x = pos.X - w / 2;
                        float y = pos.Y - h / 2;
                        return new Rect(x, y, w, h);
                    });

                    //foreach (TreeNode treeNode in positions.Keys)
                    //{
                    //    NormalizedPosition pos = positions[treeNode];
                    //    float w = NodeWidth(treeNode);
                    //    float h = NodeHeight(treeNode);
                    //    float x = pos.X - w / 2;
                    //    float y = pos.Y - h / 2;
                    //    nodeBounds.Add(treeNode, new RectangleF(x, y, w, h));
                    //}
                }
                return nodeBounds;
            }
        }

        // ------------------------------------------------------------------------
        // constructor

        /**
         * Creates a TreeLayout for a given tree.
         * <p>
         * In addition to the tree the {@link NodeExtentProvider} and the
         * {@link Configuration} must be given.
         * 
         * @param tree &nbsp;
         * @param nodeExtentProvider &nbsp;
         * @param configuration &nbsp;
         */
        public TreeLayout(ITreeForTreeLayout<TreeNode> tree,
                INodeExtentProvider<TreeNode> nodeExtentProvider,
                Configuration<TreeNode> configuration)
        {
            Tree = tree;
            NodeExtentProvider = nodeExtentProvider;
            Configuration = configuration;
            // No need to explicitly set mod, thread and ancestor as their getters
            // are taking care of the initial values. This avoids a full tree walk
            // through and saves some memory as no entries are added for
            // "initial values".

            TreeNode r = tree.Root;
            FirstWalk(r, default(TreeNode));
            CalcSizeOfLevels(r, 0);
            SecondWalk(r, -GetPrelim(r), 0, 0);
        }


        // ------------------------------------------------------------------------
        // checkTree

        private void AddUniqueNodes(Dictionary<TreeNode, TreeNode> nodes, TreeNode newNode)
        {
            if (nodes.ContainsKey(newNode))
            {
                throw new SystemException(string.Format("Node used more than once in tree: {0}", newNode));
            }
            nodes.Add(newNode, newNode);
            foreach (TreeNode n in Tree.Children(newNode))
            {
                AddUniqueNodes(nodes, n);
            }
        }

        /**
         * Check if the tree is a "valid" tree.
         * <p>
         * Typically you will use this method during development when you get an
         * unexpected layout from your trees.
         * <p>
         * The following checks are performed:
         * <ul>
         * <li>Each node must only occur once in the tree.</li>
         * </ul>
         */
        public void CheckTree()
        {
            Dictionary<TreeNode, TreeNode> nodes = new Dictionary<TreeNode, TreeNode>();

            // Traverse the tree and check if each node is only used once.
            AddUniqueNodes(nodes, Tree.Root);
        }

        // ------------------------------------------------------------------------
        // dumpTree

        private void DumpTree(StreamWriter output, TreeNode node, int indent,
                DumpConfiguration dumpConfiguration)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                sb.Append(dumpConfiguration.Indent);
            }

            if (dumpConfiguration.IncludeObjectToString)
            {
                sb.Append("[")
                  .Append(node.GetType().Name)
                  .Append("@")
                  .Append(node.GetHashCode().ToString("X"))
                  .Append("]");
            }

            sb.Append(StringUtililities.Quote(node?.ToString()));

            if (dumpConfiguration.IncludeNodeSize)
            {
                sb.Append(" (size: ")
                  .Append(NodeWidth(node))
                  .Append("x")
                  .Append(NodeHeight(node))
                  .Append(")");
            }

            output.WriteLine(sb);

            foreach (TreeNode n in Tree.Children(node))
            {
                DumpTree(output, n, indent + 1, dumpConfiguration);
            }
        }

        /**
         * Prints a dump of the tree to the given printStream, using the node's
         * "toString" method.
         * 
         * @param printStream &nbsp;
         * @param dumpConfiguration
         *            [default: new DumpConfiguration()]
         */
        public void DumpTree(StreamWriter printStream, DumpConfiguration dumpConfiguration)
        => DumpTree(printStream, Tree.Root, 0, dumpConfiguration);


        public void DumpTree(StreamWriter printStream) => DumpTree(printStream, new DumpConfiguration());

    }
}
