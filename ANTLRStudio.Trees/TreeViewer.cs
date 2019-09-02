/*
 * Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ANTLRStudio.TreeLayout;
using ANTLRStudio.TreeLayout.Interfaces;
using ANTLRStudio.TreeLayout.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Tree = Antlr4.Runtime.Tree.ITree;

namespace ANTLRStudio.Trees
{
    public class TreeViewer : UserControl
    {
        #region Static Data
        public static readonly ISolidColorBrush LIGHT_RED = Brushes.IndianRed; // new Color(255, 244, 213, 211);
        public static readonly FontFamily fontFamily = FontFamily.Parse("Monospace");
        public static readonly Pen LinePen = new Pen(Brushes.Black, 1.0f, null, PenLineCap.Round, PenLineCap.Round, PenLineCap.Round, PenLineJoin.Round);
        #region Avalonia Properties
        public static readonly AvaloniaProperty<float> GapBetweenLevelsProperty = AvaloniaProperty.Register<TreeViewer, float>(nameof(GapBetweenLevels));
        public static readonly AvaloniaProperty<float> GapBetweenNodesProperty = AvaloniaProperty.Register<TreeViewer, float>(nameof(GapBetweenNodes));
        public static readonly AvaloniaProperty<int> NodeWidthPaddingProperty = AvaloniaProperty.Register<TreeViewer, int>(nameof(NodeWidthPadding));
        public static readonly AvaloniaProperty<int> NodeHeightPaddingProperty = AvaloniaProperty.Register<TreeViewer, int>(nameof(NodeHeightPadding));
        public static readonly AvaloniaProperty<int> ArcSizeProperty = AvaloniaProperty.Register<TreeViewer, int>(nameof(ArcSize));
        public static readonly AvaloniaProperty<int> ScaleProperty = AvaloniaProperty.Register<TreeViewer, int>(nameof(Scale));
        public static readonly AvaloniaProperty<IList<string>> RuleNamesProperty = AvaloniaProperty.Register<TreeViewer, IList<string>>(nameof(RuleNames));
        public static readonly AvaloniaProperty<bool> UseCurvedEdgesProperty = AvaloniaProperty.Register<TreeViewer, bool>(nameof(UseCurvedEdges));
        public static readonly AvaloniaProperty<ISolidColorBrush> BoxColorProperty = AvaloniaProperty.Register<TreeViewer, ISolidColorBrush>(nameof(BoxColor));
        public static readonly AvaloniaProperty<ISolidColorBrush> BorderColorProperty = AvaloniaProperty.Register<TreeViewer, ISolidColorBrush>(nameof(BorderColor));
        public static readonly AvaloniaProperty<ISolidColorBrush> HighlightedBoxColorProperty = AvaloniaProperty.Register<TreeViewer, ISolidColorBrush>(nameof(HighlightedBoxColor));
        public static readonly AvaloniaProperty<Color> TextColorProperty = AvaloniaProperty.Register<TreeViewer, Color>(nameof(TextColor));
        public static readonly AvaloniaProperty<ITreeTextProvider> TreeTextProviderProperty = AvaloniaProperty.Register<TreeViewer, ITreeTextProvider>(nameof(TreeTextProvider));
        public static readonly AvaloniaProperty<Tree> TreeProperty = AvaloniaProperty.Register<TreeViewer, Tree>(nameof(Tree));
        #endregion
        #endregion
        public Tree Tree
        {
            get => GetValue(TreeProperty);
            set
            {
                Console.WriteLine("TREE IS SETTING!!");
                if (value != null)
                {
                    TreeLayout =
                        new TreeLayout<Tree>(new TreeLayoutAdaptor(value),
                                             new VariableExtentProvide(this),
                                             new DefaultConfiguration<Tree>(GapBetweenLevels,
                                                                            GapBetweenNodes));
                    this.InvalidateVisual();
                }
                else
                {
                    TreeLayout = null;
                }
                SetValue(TreeProperty, value);
            }
        }

        public ITreeForTreeLayout<Tree> LayoutOfTree => TreeLayout.Tree;
        public TreeLayout<Tree> TreeLayout { get; private set; }
        protected List<Tree> highlightedNodes;

        [DefaultValue(17.0)]
        public float GapBetweenLevels
        {
            get => GetValue(GapBetweenLevelsProperty);
            set => SetValue(GapBetweenLevelsProperty, value);
        }

        //protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        //{
        //    base.OnPropertyChanged(e);
        //}

        [DefaultValue(7.0)]
        public float GapBetweenNodes
        {
            get => GetValue(GapBetweenNodesProperty);
            set => SetValue(GapBetweenNodesProperty, value);
        }
        [DefaultValue(2)]
        // added to left/right
        public int NodeWidthPadding
        {
            get => GetValue(NodeWidthPaddingProperty);
            set => SetValue(NodeWidthPaddingProperty, value);
        }
        [DefaultValue(0)]
        // added above/below
        public int NodeHeightPadding
        {
            get => GetValue(NodeHeightPaddingProperty);
            set => SetValue(NodeHeightPaddingProperty, value);
        }
        // make an arc in node outline?
        public int ArcSize
        {
            get => GetValue(ArcSizeProperty);
            set => SetValue(ArcSizeProperty, value);
        }

        [DefaultValue(1.0)]
        public float Scale
        {
            get => GetValue(ScaleProperty);
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                SetValue(ScaleProperty, value);
            }
        }
        public IList<string> RuleNames
        {
            get => GetValue(RuleNamesProperty);
            set
            {
                TreeTextProvider = new DefaultTreeTextProvider(value);
                SetValue(RuleNamesProperty, value);
            }
        }
        public bool UseCurvedEdges
        {
            get => GetValue(UseCurvedEdgesProperty);
            set => SetValue(UseCurvedEdgesProperty, value);
        }
        // set to a color to make it draw background
        public ISolidColorBrush BoxColor
        {
            get => GetValue(BoxColorProperty);
            set => SetValue(BoxColorProperty, value);
        }
        public ISolidColorBrush HighlightedBoxColor
        {
            get => GetValue(HighlightedBoxColorProperty);
            set => SetValue(HighlightedBoxColorProperty, value);
        }
        public ISolidColorBrush BorderColor
        {
            get => GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }
        public Color TextColor
        {
            get => GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        public ITreeTextProvider TreeTextProvider
        {
            get => GetValue(TreeTextProviderProperty);
            set => SetValue(TreeTextProviderProperty, value);
        }

        public TreeViewer()
        {
            TextColor = Colors.Black;
            BorderColor = Brushes.Black;
            BoxColor = Brushes.Transparent;
            HighlightedBoxColor = Brushes.LightGray;
            TreeProperty.Changed.Subscribe(e => { Tree = (e.NewValue as Tree); });
        }

        //public TreeViewer(List<string> ruleNames, Tree tree)
        //{
        //    RuleNames = ruleNames;
        //    if (tree != null)
        //    {
        //        Tree = tree;
        //    }
        //}

        // ---------------- PAINT -----------------------------------------------

        protected void PaintEdges(DrawingContext g, Tree parent)
        {
            if (!LayoutOfTree.IsLeaf(parent))
            {
                Pen stroke = LinePen;

                var parentBounds = BoundsOfNode(parent);
                double x1 = parentBounds.Center.X;
                double y1 = parentBounds.TopLeft.Y;
                foreach (Tree child in LayoutOfTree.Children(parent))
                {
                    var childBounds = BoundsOfNode(child);
                    double x2 = childBounds.Center.X;
                    double y2 = childBounds.TopLeft.Y;
                    //if (UseCurvedEdges)
                    //{
                    //    CubicCurve2D c = new CubicCurve2D.Double();
                    //    double ctrlx1 = x1;
                    //    double ctrly1 = (y1 + y2) / 2;
                    //    double ctrlx2 = x2;
                    //    double ctrly2 = y1;
                    //    c.setCurve(x1, y1, ctrlx1, ctrly1, ctrlx2, ctrly2, x2, y2);
                    //    g.DrawArc(stroke,);
                    //}
                    //else
                    //{
                    //System.Console.WriteLine($"{x1},{y1},{x2},{y2}");

                    g.DrawLine(stroke, new Point(x1, y1), new Point(x2, y2));
                    //}
                    PaintEdges(g, child);
                }
            }
        }

        protected void PaintBox(DrawingContext g, Tree tree)
        {
            var typeFace = new Typeface(FontFamily, FontSize, FontStyle, FontWeight);
            void Text(string data, Point location)
            {
                string formatted = Utils.EscapeWhitespace(data, true);
                var text = new FormattedText { Text = formatted };
                text.Typeface = typeFace;
                g.DrawText(Brushes.Black, location, text);
            }

            var box = BoundsOfNode(tree);
            // draw the box in the background
            bool ruleFailedAndMatchedNothing = false;
            if (tree is ParserRuleContext ctx)
            {
                ruleFailedAndMatchedNothing = ctx.exception != null && ctx.Stop != null && ctx.Stop.TokenIndex < ctx.Start.TokenIndex;
            }
            if (IsHighlighted(tree) || tree is IErrorNode || ruleFailedAndMatchedNothing)
            {
                var color = BoxColor;
                if (IsHighlighted(tree)) color = HighlightedBoxColor;
                if (tree is IErrorNode || ruleFailedAndMatchedNothing) color = LIGHT_RED;
                var rect = new Rect(new Point(box.Center.X, box.BottomLeft.Y), new Point(box.Width - 1, box.Height - 1));
                g.FillRectangle(color, rect);
            }
            g.DrawRectangle(new Pen(BorderColor), new Rect(box.X, box.Y, box.Width - 1, box.Height - 1));


            double x = box.X + ArcSize / 2 + NodeWidthPadding;
            //TODO FIX THIS WHEN AVALONIA GETS UPDATED
            //double y = box.BottomLeft.Y + font.Ascent + font.Leading + 1 + nodeHeightPadding;
            double y = box.BottomLeft.Y + this.FontSize + 1 + NodeWidthPadding;
            // draw the text on top of the box (possibly multiple lines)
            foreach (string line in TreeTextProvider.Text(tree).Split('\n'))
            {
                Text(line, new Point(x, y));
                y += this.FontSize; //fontFamily.LineHeight;
            }
        }


        public override void Render(DrawingContext context)
        {
            base.Render(context);

            Console.WriteLine(TreeLayout is null);
            Console.WriteLine("TEST");
            if (TreeLayout == null)
            {
                return;
            }
            Console.WriteLine("RENDER TIME!");
            context.FillRectangle(Brushes.White, new Rect(DesiredSize));

            using (var transform = context.PushPreTransform(Matrix.CreateScale(Scale, Scale)))
            {
                PaintEdges(context, LayoutOfTree.Root);

                // paint the boxes
                foreach (Tree tree in TreeLayout.NodeBounds.Keys)
                {
                    PaintBox(context, tree);
                }
            }
        }

        private Rect ScaledTreeSize() => TreeLayout.Bounds.Inflate(Scale);
        protected Rect BoundsOfNode(Tree node) => TreeLayout.NodeBounds[node];
        protected string GetText(Tree tree)
        {
            string s = TreeTextProvider.Text(tree);
            s = Utils.EscapeWhitespace(s, true);
            return s;
        }

        ///** Slow for big lists of highlighted nodes */
        public void AddHighlightedNodes(ICollection<Tree> nodes)
        {
            highlightedNodes = new List<Tree>(nodes);
        }

        public void RemoveHighlightedNodes(ICollection<Tree> nodes)
        {
            if (highlightedNodes != null)
            {
                // only remove exact objects defined by ==, not equals()
                foreach (Tree t in nodes)
                {
                    highlightedNodes.Remove(t);
                }
            }
        }
        public bool IsHighlighted(Tree node) => HighlightedNodeIndex(node) >= 0;
        protected int HighlightedNodeIndex(Tree node) => highlightedNodes == null ? -1 : highlightedNodes.IndexOf(node);

        ///** Get an adaptor for root that indicates how to walk ANTLR trees.
        // *  Override to change the adapter from the default of {@link TreeLayoutAdaptor}  */
        public virtual ITreeForTreeLayout<Tree> TreeLayoutAdaptorFor(Tree root) => new TreeLayoutAdaptor(root);

    }
}