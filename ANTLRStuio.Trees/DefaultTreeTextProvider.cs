/*
 * Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System.Collections.Generic;
using Tree = Antlr4.Runtime.Tree.ITree;
using Antlr4.Runtime.Misc;

namespace ANTLRStudio.Trees
{
    public class DefaultTreeTextProvider : ITreeTextProvider
    {
        private readonly List<string> ruleNames;

        public DefaultTreeTextProvider(List<string> ruleNames)
        {
            this.ruleNames = ruleNames;
        }

        public string Text(Tree node) => Antlr4.Runtime.Tree.Trees.GetNodeText(node, ruleNames);

    }
}