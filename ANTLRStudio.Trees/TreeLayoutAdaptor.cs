/*
 * Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System.Collections.Generic;
using System.Linq;
using ANTLRStudio.TreeLayout.Interfaces;
using Tree = Antlr4.Runtime.Tree.ITree;

namespace ANTLRStudio.Trees
{
    public class TreeLayoutAdaptor : ITreeForTreeLayout<Tree>
    {
        public TreeLayoutAdaptor(Tree root)
        {
            Root = root;
        }
        public Tree Root { get; }

        public IEnumerable<Tree> Children(Tree parentNode) => Enumerable.Range(0, parentNode.ChildCount).Select(parentNode.GetChild);


        public IEnumerable<Tree> ChildrenReversed(Tree parentNode)
        => Enumerable.Range(0, parentNode.ChildCount).Reverse().Select(parentNode.GetChild);

        public Tree FirstChild(Tree parentNode)
        => parentNode.GetChild(0);

        public bool IsChildOfParent(Tree node, Tree parentNode)
        => Children(parentNode).Contains(node);
        public bool IsLeaf(Tree node)
        => node.ChildCount == 0;

        public Tree LastChild(Tree parentNode)
        => parentNode.GetChild(parentNode.ChildCount - 1);
    }
}