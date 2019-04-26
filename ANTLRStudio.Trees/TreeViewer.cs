/*
 * Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ANTLRStudio.TreeLayout;
using ANTLRStudio.TreeLayout.Interfaces;
using ANTLRStudio.TreeLayout.Utilities;
using Eto.Drawing;
using Eto.Forms;
using Tree = Antlr4.Runtime.Tree.ITree;

namespace ANTLRStudio.Trees
{
    public class TreeViewer : Drawable
    {
        public static readonly Color LIGHT_RED = new Color(244, 213, 211);

        protected TreeLayout<Tree> treeLayout;
        protected List<Tree> highlightedNodes;

        protected FontStyle fontStyle = FontStyle.None;
        protected int fontSize = 11;
        public Font font = Fonts.Monospace(11);

        protected float gapBetweenLevels = 17.0F;
        protected float gapBetweenNodes = 7.0F;
        public int nodeWidthPadding = 2;  // added to left/right
        public int nodeHeightPadding; // added above/below
        protected int arcSize;           // make an arc in node outline?

        protected float scale = 1.0F;

        protected Color boxColor = Colors.Transparent;     // set to a color to make it draw background

        protected Color highlightedBoxColor = Colors.LightGrey;
        protected Color borderColor = Colors.Black;

        public TreeViewer(List<string> ruleNames, Tree tree)
        {
            SetRuleNames(ruleNames);
            if (tree != null)
            {
                SetTree(tree);
            }
        }

        private void UpdatePreferredSize()
        {
            Size = new Size(ScaledTreeSize().Size);
            Invalidate();
            if (Parent != null)
            {
                Parent.Invalidate();
            }
            Invalidate();
        }

        // ---------------- PAINT -----------------------------------------------

        public bool UseCurvedEdges { get; set; }


        protected void PaintEdges(Graphics g, Tree parent)
        {
            if (!Tree.IsLeaf(parent))
            {
                Pen stroke = new Pen(Colors.Black, 1.0f)
                {
                    LineCap = PenLineCap.Round,
                    LineJoin = PenLineJoin.Round
                };

                var parentBounds = BoundsOfNode(parent);
                float x1 = parentBounds.Center.X;
                float y1 = parentBounds.TopLeft.Y;
                foreach (Tree child in Tree.Children(parent))
                {
                    var childBounds = BoundsOfNode(child);
                    float x2 = childBounds.Center.X;
                    float y2 = childBounds.TopLeft.Y;
                    //if (UseCurvedEdges)
                    //{
                    //    CubicCurve2D c = new CubicCurve2D.Double();
                    //    float ctrlx1 = x1;
                    //    float ctrly1 = (y1 + y2) / 2;
                    //    float ctrlx2 = x2;
                    //    float ctrly2 = y1;
                    //    c.setCurve(x1, y1, ctrlx1, ctrly1, ctrlx2, ctrly2, x2, y2);
                    //    g.DrawArc(stroke,);
                    //}
                    //else
                    //{
                    System.Console.WriteLine($"{x1},{y1},{x2},{y2}");

                    g.DrawLine(stroke, x1, y1, x2, y2);
                    //}
                    PaintEdges(g, child);
                }
            }
        }

        protected void PaintBox(Graphics g, Tree tree)
        {
            var box = BoundsOfNode(tree);
            // draw the box in the background
            bool ruleFailedAndMatchedNothing = false;
            if (tree is ParserRuleContext ctx)
            {
                ruleFailedAndMatchedNothing = ctx.exception != null && ctx.Stop != null && ctx.Stop.TokenIndex < ctx.Start.TokenIndex;
            }
            if (IsHighlighted(tree) || tree is IErrorNode || ruleFailedAndMatchedNothing)
            {
                var color = boxColor;
                if (IsHighlighted(tree)) color = highlightedBoxColor;
                if (tree is IErrorNode || ruleFailedAndMatchedNothing) color = LIGHT_RED;
                g.FillRectangle(color, box.Center.X, box.BottomLeft.Y, box.Width - 1, box.Height - 1);
            }
            //g.DrawRectangle(borderColor, box.X, box.Y, box.Width - 1, box.Height - 1);


            // draw the text on top of the box (possibly multiple lines)
            string s = TreeTextProvider.Text(tree);
            string[] lines = s.Split('\n');
            float x = box.X + arcSize / 2 + nodeWidthPadding;
            float y = box.BottomLeft.Y + font.Ascent + font.Leading + 1 + nodeHeightPadding;
            for (int i = 0; i < lines.Length; i++)
            {
                Text(g, lines[i], new PointF(x, y));
                y += font.LineHeight;
            }
        }

        public void Text(Graphics g, string s, PointF location)
        {
            //      System.out.println("drawing '"+s+"' @ "+x+","+y);
            s = Utils.EscapeWhitespace(s, true);
            g.DrawText(font, Colors.Black, location, s);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (treeLayout == null)
            {
                return;
            }

            Graphics graphics = e.Graphics;
            graphics.AntiAlias = true;

            graphics.FillRectangle(Colors.White, new Rectangle(Size));


            graphics.ScaleTransform(scale);
            PaintEdges(graphics, Tree.Root);

            // paint the boxes
            foreach (Tree tree in treeLayout.NodeBounds.Keys)
            {
                PaintBox(graphics, tree);
            }
        }

        protected void GenerateEdges(TextWriter writer, Tree parent)
        {
            if (!Tree.IsLeaf(parent))
            {
                var b1 = BoundsOfNode(parent);
                float x1 = b1.Center.X;
                float y1 = b1.Center.Y;

                foreach (Tree child in Tree.Children(parent))
                {
                    var childbounds = BoundsOfNode(child);
                    float x2 = childbounds.Center.X;
                    float y2 = childbounds.BottomLeft.Y;
                    writer.WriteLine(Line("" + x1, "" + y1, "" + x2, "" + y2,
                                        "stroke:black; stroke-width:1px;"));
                    GenerateEdges(writer, child);
                }
            }
        }

        protected void GenerateBox(TextWriter writer, Tree parent)
        {

            // draw the box in the background
            var box = BoundsOfNode(parent);
            writer.Write(Rect("" + box.X, "" + box.Y, "" + box.Width, "" + box.Height,
                    "fill:orange; stroke:rgb(0,0,0);", "rx=\"1\""));

            // draw the text on top of the box (possibly multiple lines)
            string line = TreeTextProvider.Text(parent).Replace("<", "&lt;").Replace(">", "&gt;");
            int fontBoxSize = 10;
            int x = (int)box.X + 2;
            int y = (int)box.Y + fontBoxSize - 1;
            string style = string.Format("font-family:sans-serif;font-size:{0}px;",
                    fontBoxSize);
            writer.Write(Text("" + x, "" + y, style, line));
        }

        private static string Line(string x1, string y1, string x2, string y2,
            string style)
        {
            return string
                .Format("<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" style=\"{4}\" />\n",
                    x1, y1, x2, y2, style);
        }

        private static string Rect(string x, string y, string width, string height,
            string style, string extraAttributes)
        {
            return string
                .Format("<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" style=\"{4}\" {5}/>\n",
                    x, y, width, height, style, extraAttributes);
        }

        private static string Text(string x, string y, string style, string text)
        {
            return string.Format(
                "<text x=\"{0}\" y=\"{1}\" style=\"{2}\">\n{3}\n</text>\n", x, y,
                style, text);
        }

        private void PaintSVG(TextWriter writer)
        {
            GenerateEdges(writer, Tree.Root);

            foreach (Tree tree in treeLayout.NodeBounds.Keys)
            {
                GenerateBox(writer, tree);
            }
        }

        //@Override
        //    protected Graphics getComponentGraphics(Graphics g)
        //{
        //    Graphics2D g2d = (Graphics2D)g;
        //    g2d.scale(scale, scale);
        //    return super.getComponentGraphics(g2d);
        //}

        //// ----------------------------------------------------------------------


        private static readonly string DIALOG_WIDTH_PREFS_KEY = "dialog_width";
        private static readonly string DIALOG_HEIGHT_PREFS_KEY = "dialog_height";
        private static readonly string DIALOG_X_PREFS_KEY = "dialog_x";
        private static readonly string DIALOG_Y_PREFS_KEY = "dialog_y";
        private static readonly string DIALOG_DIVIDER_LOC_PREFS_KEY = "dialog_divider_location";
        private static readonly string DIALOG_VIEWER_SCALE_PREFS_KEY = "dialog_viewer_scale";

        //protected static JFrame showInDialog(readonly TreeViewer viewer)
        //{
        //    readonly JFrame dialog = new JFrame();
        //    dialog.setTitle("Parse Tree Inspector");

        //    readonly Preferences prefs = Preferences.userNodeForPackage(TreeViewer.class);

        //        // Make new content panes
        //        readonly Container mainPane = new JPanel(new BorderLayout(5, 5));
        //readonly Container contentPane = new JPanel(new BorderLayout(0, 0));
        //contentPane.setBackground(Color.white);

        //        // Wrap viewer in scroll pane
        //        JScrollPane scrollPane = new JScrollPane(viewer);
        //// Make the scrollpane (containing the viewer) the center component
        //contentPane.add(scrollPane, BorderLayout.CENTER);

        //        JPanel wrapper = new JPanel(new FlowLayout());

        //// Add button to bottom
        //JPanel bottomPanel = new JPanel(new BorderLayout(0, 0));
        //contentPane.add(bottomPanel, BorderLayout.SOUTH);

        //        JButton ok = new JButton("OK");
        //ok.addActionListener(
        //            new ActionListener()
        //{
        //    @Override
        //                public void actionPerformed(ActionEvent e)
        //    {
        //        dialog.dispatchEvent(new WindowEvent(dialog, WindowEvent.WINDOW_CLOSING));
        //    }
        //}
        //        );
        //        wrapper.add(ok);

        //        // Add an export-to-png button right of the "OK" button
        //        JButton png = new JButton("Export as PNG");
        //png.addActionListener(
        //            new ActionListener()
        //{
        //    @Override
        //                public void actionPerformed(ActionEvent e)
        //    {
        //        generatePNGFile(viewer, dialog);
        //    }
        //}
        //        );
        //        wrapper.add(png);

        //        // Add an export-to-png button right of the "OK" button
        //        JButton svg = new JButton("Export as SVG");
        //svg.addActionListener(
        //            new ActionListener()
        //{
        //    @Override
        //            public void actionPerformed(ActionEvent e)
        //    {
        //        generateSVGFile(viewer, dialog);
        //    }
        //}
        //        );
        //        wrapper.add(svg);

        //        bottomPanel.add(wrapper, BorderLayout.SOUTH);

        //        // Add scale slider
        //        double lastKnownViewerScale = prefs.getDouble(DIALOG_VIEWER_SCALE_PREFS_KEY, viewer.getScale());
        //viewer.setScale(lastKnownViewerScale);

        //        int sliderValue = (int)((lastKnownViewerScale - 1.0) * 1000);
        //readonly JSlider scaleSlider = new JSlider(JSlider.HORIZONTAL, -999, 1000, sliderValue);

        //scaleSlider.addChangeListener(
        //            new ChangeListener()
        //{
        //    @Override
        //                public void stateChanged(ChangeEvent e)
        //    {
        //        int v = scaleSlider.getValue();
        //        viewer.setScale(v / 1000.0 + 1.0);
        //    }
        //}
        //        );
        //        bottomPanel.add(scaleSlider, BorderLayout.CENTER);

        //        // Add a JTree representing the parser tree of the input.
        //        JPanel treePanel = new JPanel(new BorderLayout(5, 5));

        //// An "empty" icon that will be used for the JTree's nodes.
        //Icon empty = new EmptyIcon();

        //UIManager.put("Tree.closedIcon", empty);
        //        UIManager.put("Tree.openIcon", empty);
        //        UIManager.put("Tree.leafIcon", empty);

        //        Tree parseTreeRoot = viewer.getTree().getRoot();
        //TreeNodeWrapper nodeRoot = new TreeNodeWrapper(parseTreeRoot, viewer);
        //fillTree(nodeRoot, parseTreeRoot, viewer);
        //readonly JTree tree = new JTree(nodeRoot);
        //tree.getSelectionModel().setSelectionMode(TreeSelectionModel.SINGLE_TREE_SELECTION);

        //tree.addTreeSelectionListener(new TreeSelectionListener()
        //{
        //    @Override
        //                public void valueChanged(TreeSelectionEvent e)
        //    {

        //        JTree selectedTree = (JTree)e.getSource();
        //        TreePath path = selectedTree.getSelectionPath();
        //        if (path != null)
        //        {
        //            TreeNodeWrapper treeNode = (TreeNodeWrapper)path.getLastPathComponent();

        //            // Set the clicked AST.
        //            viewer.setTree((Tree)treeNode.getUserObject());
        //        }
        //    }
        //});

        //        treePanel.add(new JScrollPane(tree));

        //        // Create the pane for both the JTree and the AST
        //        readonly JSplitPane splitPane = new JSplitPane(JSplitPane.HORIZONTAL_SPLIT,
        //                treePanel, contentPane);

        //mainPane.add(splitPane, BorderLayout.CENTER);

        //        dialog.setContentPane(mainPane);

        //        // make viz
        //        WindowListener exitListener = new WindowAdapter() {
        //            @Override
        //            public void windowClosing(WindowEvent e)
        //{
        //    prefs.putInt(DIALOG_WIDTH_PREFS_KEY, (int)dialog.getSize().getWidth());
        //    prefs.putInt(DIALOG_HEIGHT_PREFS_KEY, (int)dialog.getSize().getHeight());
        //    prefs.putDouble(DIALOG_X_PREFS_KEY, dialog.getLocationOnScreen().getX());
        //    prefs.putDouble(DIALOG_Y_PREFS_KEY, dialog.getLocationOnScreen().getY());
        //    prefs.putInt(DIALOG_DIVIDER_LOC_PREFS_KEY, splitPane.getDividerLocation());
        //    prefs.putDouble(DIALOG_VIEWER_SCALE_PREFS_KEY, viewer.getScale());

        //    dialog.setVisible(false);
        //    dialog.dispose();
        //}
        //};
        //dialog.addWindowListener(exitListener);
        //        dialog.setDefaultCloseOperation(JFrame.DO_NOTHING_ON_CLOSE);

        //        int width = prefs.getInt(DIALOG_WIDTH_PREFS_KEY, 600);
        //int height = prefs.getInt(DIALOG_HEIGHT_PREFS_KEY, 500);
        //dialog.setPreferredSize(new Dimension(width, height));
        //        dialog.pack();

        //        // After pack(): set the divider at 1/3 (200/600) of the frame.
        //        int dividerLocation = prefs.getInt(DIALOG_DIVIDER_LOC_PREFS_KEY, 200);
        //splitPane.setDividerLocation(dividerLocation);

        //        if (prefs.getDouble(DIALOG_X_PREFS_KEY, -1) != -1) {
        //            dialog.setLocation(
        //                    (int) prefs.getDouble(DIALOG_X_PREFS_KEY, 100),
        //                    (int) prefs.getDouble(DIALOG_Y_PREFS_KEY, 100)
        //            );
        //        }
        //        else {
        //            dialog.setLocationRelativeTo(null);
        //        }

        //        dialog.setVisible(true);
        //        return dialog;
        //    }

        //    private static void generatePNGFile(TreeViewer viewer, JFrame dialog)
        //{
        //    BufferedImage bi = new BufferedImage(viewer.getSize().width,
        //                                         viewer.getSize().height,
        //                                         BufferedImage.TYPE_INT_ARGB);
        //    Graphics g = bi.createGraphics();
        //    viewer.paint(g);
        //    g.dispose();

        //    try
        //    {
        //        JFileChooser fileChooser = getFileChooser(".png", "PNG files");

        //        int returnValue = fileChooser.showSaveDialog(dialog);
        //        if (returnValue == JFileChooser.APPROVE_OPTION)
        //        {
        //            File pngFile = fileChooser.getSelectedFile();
        //            ImageIO.write(bi, "png", pngFile);

        //            try
        //            {
        //                // Try to open the parent folder using the OS' native file manager.
        //                Desktop.getDesktop().open(pngFile.getParentFile());
        //            }
        //            catch (Exception ex)
        //            {
        //                // We could not launch the file manager: just show a popup that we
        //                // succeeded in saving the PNG file.
        //                JOptionPane.showMessageDialog(dialog, "Saved PNG to: " +
        //                                              pngFile.getAbsolutePath());
        //                ex.printStackTrace();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        JOptionPane.showMessageDialog(dialog,
        //                                      "Could not export to PNG: " + ex.getMessage(),
        //                                      "Error",
        //                                      JOptionPane.ERROR_MESSAGE);
        //        ex.printStackTrace();
        //    }
        //}

        //private static JFileChooser getFileChooser(readonly string fileEnding,
        //                                            readonly string description)
        //{
        //    File suggestedFile = generateNonExistingFile(fileEnding);
        //    JFileChooser fileChooser = new JFileChooserConfirmOverwrite();
        //    fileChooser.setCurrentDirectory(suggestedFile.getParentFile());
        //    fileChooser.setSelectedFile(suggestedFile);
        //    FileFilter filter = new FileFilter() {

        //            @Override
        //            public boolean accept(File pathname)
        //    {
        //        if (pathname.isFile())
        //        {
        //            return pathname.getName().toLowerCase().endsWith(fileEnding);
        //        }

        //        return true;
        //    }

        //    @Override
        //            public string getDescription()
        //    {
        //        return description + " (*" + fileEnding + ")";
        //    }
        //};
        //fileChooser.addChoosableFileFilter(filter);
        //        fileChooser.setFileFilter(filter);
        //        return fileChooser;
        //    }

        //    private static void generateSVGFile(TreeViewer viewer, JFrame dialog)
        //{

        //    try
        //    {
        //        JFileChooser fileChooser = getFileChooser(".svg", "SVG files");

        //        int returnValue = fileChooser.showSaveDialog(dialog);
        //        if (returnValue == JFileChooser.APPROVE_OPTION)
        //        {
        //            File svgFile = fileChooser.getSelectedFile();
        //            // save the new svg file here!
        //            BufferedWriter writer = new BufferedWriter(new FileWriter(svgFile));
        //            // HACK: multiplying with 1.1 should be replaced wit an accurate number
        //            writer.write("<svg width=\"" + viewer.getSize().getWidth() * 1.1 + "\" height=\"" + viewer.getSize().getHeight() * 1.1 + "\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\">");
        //            viewer.paintSVG(writer);
        //            writer.write("</svg>");
        //            writer.flush();
        //            writer.close();
        //            try
        //            {
        //                // Try to open the parent folder using the OS' native file manager.
        //                Desktop.getDesktop().open(svgFile.getParentFile());
        //            }
        //            catch (Exception ex)
        //            {
        //                // We could not launch the file manager: just show a popup that we
        //                // succeeded in saving the PNG file.
        //                JOptionPane.showMessageDialog(dialog, "Saved SVG to: "
        //                    + svgFile.getAbsolutePath());
        //                ex.printStackTrace();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        JOptionPane.showMessageDialog(dialog,
        //            "Could not export to SVG: " + ex.getMessage(),
        //            "Error",
        //            JOptionPane.ERROR_MESSAGE);
        //        ex.printStackTrace();
        //    }
        //}

        private static string GenerateNonExistingFile(string extension)
        {
            const string parent = ".";
            const string name = "antlr4_parse_tree";

            string file = parent + name + extension;

            int counter = 1;

            // Keep looping until we create a File that does not yet exist.
            while (File.Exists(file))
            {
                file = parent + name + "_" + counter + extension;
                counter++;
            }

            return file;
        }

        //private static void FillTree(TreeNodeWrapper node, Tree tree, TreeViewer viewer)
        //{

        //    if (tree == null)
        //    {
        //        return;
        //    }

        //    for (int i = 0; i < tree.ChildCount; i++)
        //    {

        //        Tree childTree = tree.GetChild(i);
        //        TreeNodeWrapper childNode = new TreeNodeWrapper(childTree, viewer);

        //        //node.Add(childNode);

        //        FillTree(childNode, childTree, viewer);
        //    }
        //}

        private RectangleF ScaledTreeSize() => treeLayout.Bounds * scale;



        //// ---------------------------------------------------

        protected RectangleF BoundsOfNode(Tree node) => treeLayout.NodeBounds[node];


        protected string GetText(Tree tree)
        {
            string s = TreeTextProvider.Text(tree);
            s = Utils.EscapeWhitespace(s, true);
            return s;
        }

        public ITreeTextProvider TreeTextProvider { get; set; }

        public void SetFontSize(int sz)
        {
            fontSize = sz;
            font = Fonts.Serif(sz, fontStyle);
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

        protected bool IsHighlighted(Tree node) => HighlightedNodeIndex(node) >= 0;


        protected int HighlightedNodeIndex(Tree node) => highlightedNodes == null ? -1 : highlightedNodes.IndexOf(node);
        public int ArcSize { get; set; }
        public Color BoxColor { get; set; }
        public Color HighlightedBoxColor { get; set; }
        public Color BorderColor { get; set; }
        public Color TextColor { get; set; } = Colors.Black;

        protected ITreeForTreeLayout<Tree> Tree => treeLayout.Tree;

        public void SetTree(Tree root)
        {
            if (root != null)
            {
                treeLayout =
                    new TreeLayout<Tree>(new TreeLayoutAdaptor(root),
                                         new VariableExtentProvide(this),
                                         new DefaultConfiguration<Tree>(gapBetweenLevels,
                                                                        gapBetweenNodes));
                // Let the UI display this new AST.
                UpdatePreferredSize();
            }
            else
            {
                treeLayout = null;
                Invalidate();
            }
        }

        ///** Get an adaptor for root that indicates how to walk ANTLR trees.
        // *  Override to change the adapter from the default of {@link TreeLayoutAdaptor}  */
        public ITreeForTreeLayout<Tree> GetTreeLayoutAdaptor(Tree root) => new TreeLayoutAdaptor(root);


        public float Scale
        {
            get => scale;
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                scale = value;
                UpdatePreferredSize();
            }
        }

        public void SetRuleNames(IList<string> ruleNames)
        {
            TreeTextProvider = (new DefaultTreeTextProvider(ruleNames));
        }
    }
}