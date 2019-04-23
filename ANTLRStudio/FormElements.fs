module FormElements
open System;
open System.IO;
open Eto.Forms;
open System.Text;
open ANTLRStudio.Parser;
open ANTLRStudio.TreeLayout;
open ANTLRStudio.TreeLayout.Utilities;
//open ANTLRStudio.TreeLayout.Example;
open AntlrTools;
open ANTLRStudio.Trees;
let readGrammarToHtml name =
    let buildSvgHtml(svg : string)=
        //an SVG document will otherwise not show on Windows
        (new StringBuilder()).Append("<html>")
                             .Append("<head>")
                             .Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=9\"/>")
                             .Append("</head>")
                             .Append("<body>")
                             .Append(svg)
                             .Append("</body>")
                             .Append("</html>")
                             .ToString()
    let append (writer:StringWriter) (name,diagram:RailwayPortPy.Diagram) = 
        writer.Write(sprintf "<h1>%s</h1>\n" <| RailwayPortPy.escape(name))
        diagram.writeSvg(writer)
        writer.Write("\n")
        writer

    name
    |> AntlrParser.ParseFile
    |> (Seq.fold <| append <| new StringWriter())
    |> (fun x -> x.ToString())
    |> buildSvgHtml

let openGrammar (form:Form) =
    let dir = Directory.GetCurrentDirectory() // Eto.GTK changes the directory...
    use dialog = new OpenFileDialog(MultiSelect = false,
                                    Title = "Select Grammar",
                                    CheckFileExists = true,
                                    Directory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)))
    dialog.Filters.Add(new FileFilter("ANTLR Files (.g4)",".g4"))
    let dialogResult = dialog.ShowDialog(form.ParentWindow)
    Directory.SetCurrentDirectory(dir) // Eto.GTK changes the directory...

    match dialogResult with
    | DialogResult.Ok -> if dialog.FileName <> null then
                            let fileName = dialog.FileName
                            file <- fileName
                            printfn "Trigger!!"
                            loadedFileInput.Trigger(fileName)
    | v -> printf "User pressed %O" v


let studioForm (app:Application) (form:Form) =
    let webView = new WebView()
    let mutable treeViewer: TreeViewer = null
    let masterLayout = makeLayout <| Tbl([Row([El(webView);El(treeViewer)])])
    form.Content <- masterLayout
    form

let mainForm (app:Application) (form:Form) = 
    let webView = new WebView()
    let inputField = new RichTextArea(AcceptsTab = true, AcceptsReturn = true,Wrap = true)
    let ruleNames = new DropDown(Size = Eto.Drawing.Size(200,50))
    let generateCheckBox = new CheckBox(Text = "Ready",Size = Eto.Drawing.Size(50,50))
    let treeViewer = new TreeViewer(null,null)
    let scrollableTree = new Scrollable()
    let slider = new Slider(MaxValue = 10000,MinValue = 1,Value = 1000)
    scrollableTree.Content <- treeViewer
    let parse _ =
        if(currentParser <> null && ruleNames.SelectedValue <> null && generateCheckBox.Checked.GetValueOrDefault(false)) 
        then
            parse inputField.Text (ruleNames.SelectedValue :?> string) (currentParser,currentLexer) 
            |> (fun (tree,parser)-> treeViewer.SetRuleNames(new ResizeArray<string>(parser.RuleNames))
                                    treeViewer.SetTree(tree))
    inputField.TextChanged.Add(parse)
    ruleNames.SelectedValueChanged.Add(parse)
    generateCheckBox.CheckedChanged.Add(parse)
    slider.ValueChanged.Add(fun _ -> treeViewer.Scale <- (float32 slider.Value / 1000.f)
                                     slider.ID <-"Text" )
    loadedFile.Add(readGrammarToHtml >> webView.LoadHtml)
    loadedFile.Add(fun name -> let (parser,lexer,_) = generateParserLexerInMemory name
                               currentLexer <- lexer
                               currentParser <- parser
                               lexer.RemoveErrorListeners()
                               parser.RemoveErrorListeners()
                               form.ToolTip <- sprintf "Current Grammar is %s" <| parser.GrammarFileName.Split('.').[0]
                               ruleNames.DataStore <- parser.RuleNames |> Seq.sort |> Seq.cast<obj> )
    
    let layout = makeLayout <| Tbl [ Row [

                                         TableEl <| Tbl([
                                                         Row [
                                                              StretchedEl inputField
                                                              El ruleNames
                                                              El generateCheckBox
                                                              El slider
                                                              ]])]
                                     StretchedRow([StretchedEl scrollableTree])
                                        ]
    let size = form.Size.Width / 2
    let content = new Splitter(Panel1 = webView,Panel1MinimumSize =size,Panel2MinimumSize = size, Panel2 = layout)
    form.Content <- content
    form

//let exampleTreeForm (app:Application) (form:Form) =
    //let tree = SampleTreeFactory.ASTTree()
    //let gapBetweenLevels = 50.f
    //let gapBetweenNodes = 10.f
    //let configuration = new DefaultConfiguration<TextInBox>(gapBetweenLevels, gapBetweenNodes, Configuration<TextInBox>.Location.Left)

    //// create the NodeExtentProvider for TextInBox nodes
    //let nodeExtentProvider = new TextInBoxNodeExtentProvider()

    //// create the layout
    //let treeLayout = new TreeLayout<TextInBox>(tree, nodeExtentProvider, configuration)

    //// Create a panel that draws the nodes and edges and show the panel
    //let panel = new TextInBoxTreePane(treeLayout)
    //form.Content <- panel
    //form
    