using System;
using ANTLRStudio.TreeLayout.Interfaces;
using ANTLRStudio.TreeLayout.Utilities;

namespace ANTLRStudio.TreeLayout.Example
{
    public class SampleTreeFactory
    {
        /**
         * @return a "Sample" tree with {@link TextInBox} items as nodes.
         */
        public static ITreeForTreeLayout<TextInBox> SimpleTree()
        {
            TextInBox root = new TextInBox("root", 40, 20);
            TextInBox n1 = new TextInBox("n1", 30, 20);
            TextInBox n1_1 = new TextInBox("n1.1", 80, 36);
            TextInBox n1_2 = new TextInBox("n1.2", 40, 20);
            TextInBox n1_3 = new TextInBox("n1.3", 80, 36);
            TextInBox n2 = new TextInBox("n2", 30, 20);
            TextInBox n2_1 = new TextInBox("n2.1", 30, 20);

            DefaultTreeForTreeLayout<TextInBox> tree = new DefaultTreeForTreeLayout<TextInBox>(
                    root);
            tree.Add(root, n1);
            tree.Add(n1, n1_1);
            tree.Add(n1, n1_2);
            tree.Add(n1, n1_3);
            tree.Add(root, n2);
            tree.Add(n2, n2_1);
            return tree;
        }

        /**
         * @return a "Sample" tree with {@link TextInBox} items as nodes.
         */
        public static ITreeForTreeLayout<TextInBox> ASTTree()
        {
            TextInBox root = new TextInBox("prog", 40, 20);
            TextInBox n1 = new TextInBox("classDef", 65, 20);
            TextInBox n1_1 = new TextInBox("class", 50, 20);
            TextInBox n1_2 = new TextInBox("T", 20, 20);
            TextInBox n1_3 = new TextInBox("{", 20, 20);
            TextInBox n1_4 = new TextInBox("member", 60, 20);
            TextInBox n1_5 = new TextInBox("member", 60, 20);
            TextInBox n1_5_1 = new TextInBox("<ERROR:int>", 90, 20);
            TextInBox n1_6 = new TextInBox("member", 60, 20);
            TextInBox n1_6_1 = new TextInBox("int", 30, 20);
            TextInBox n1_6_2 = new TextInBox("i", 20, 20);
            TextInBox n1_6_3 = new TextInBox(";", 20, 20);
            TextInBox n1_7 = new TextInBox("}", 20, 20);


            DefaultTreeForTreeLayout<TextInBox> tree = new DefaultTreeForTreeLayout<TextInBox>(
                    root);
            tree.Add(root, n1);
            tree.Add(n1, n1_1);
            tree.Add(n1, n1_2);
            tree.Add(n1, n1_3);
            tree.Add(n1, n1_4);
            tree.Add(n1, n1_5);
            tree.Add(n1_5, n1_5_1);
            tree.Add(n1, n1_6);
            tree.Add(n1_6, n1_6_1);
            tree.Add(n1_6, n1_6_2);
            tree.Add(n1_6, n1_6_3);
            tree.Add(n1, n1_7);
            return tree;
        }
    }
}
