/*
 * Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using ANTLRStudio.TreeLayout.Interfaces;
using Tree = Antlr4.Runtime.Tree.ITree;

namespace ANTLRStudio.Trees
{
    public class VariableExtentProvide : INodeExtentProvider<Tree>
    {
        TreeViewer viewer;
        public VariableExtentProvide(TreeViewer viewer)
        {
            this.viewer = viewer;
        }

        public float Width(Tree tree)
        {
            string s = viewer.TreeTextProvider.Text(tree);
            float w = viewer.Font.MeasureString(s).Width + viewer.nodeWidthPadding * 2;
            return w;
        }

        public float Height(Tree tree)
        {
            string s = viewer.TreeTextProvider.Text(tree);
            float h = viewer.Font.MeasureString(s).Height + viewer.nodeHeightPadding * 2;
            string[] lines = s.Split('\n');
            return h * lines.Length;
        }

    }
}