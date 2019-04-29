module FormElements
open System;
open System.IO;
open Eto.Forms;
open System.Text;
open ANTLRStudio.Parser;
//open ANTLRStudio.TreeLayout;
//open ANTLRStudio.TreeLayout.Utilities;
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
                             .Append("<body style=\"background-color:hsl(30,20%,95%);\">")
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
                            loadedFileInput.Trigger(file)
    | v -> printf "User pressed %O" v
    

let mainForm (app:Application) (form:Form) =
    let webView = new WebView()
    let inputField = new RichTextArea(AcceptsTab = true, AcceptsReturn = true,Wrap = true)
    let ruleNames = new DropDown(Size = Eto.Drawing.Size(100,25))
    let generateCheckBox = new CheckBox(Text = "Ready",Size = Eto.Drawing.Size(50,25))
    let treeViewer = new TreeViewer(null,null)
    let scrollableTree = new Scrollable()
    let slider = new Slider(MaxValue = 20,MinValue = 1,Value = 10,Enabled = false,Size = Eto.Drawing.Size(50,25))
    let fontSizeStepper = new NumericStepper(MaxValue = 20.,Size= Eto.Drawing.Size(75,25),FormatString="Size:{0}", MinValue = 10. ,Value=(float treeViewer.FontSize),Increment = 1.)
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
    fontSizeStepper.ValueChanged.Add(fun _ -> treeViewer.FontSize <- int fontSizeStepper.Value)
    slider.ValueChanged.Add(fun _ -> treeViewer.Scale <- (float32 slider.Value) / 10.f)
    loadedFile.Add(readGrammarToHtml >> webView.LoadHtml)
    loadedFile.Add(fun name -> let (parser,lexer,_) = generateParserLexerInMemory name
                               currentLexer <- lexer
                               currentParser <- parser
                               lexer.RemoveErrorListeners()
                               parser.RemoveErrorListeners()
                               ruleNames.DataStore <- parser.RuleNames |> Seq.sort |> Seq.cast<obj> )
    loadedFile.Add(fun _ -> slider.Enabled <- true)
    let layout = makeLayout <| Tbl [ Row [

                                         TableEl <| Tbl([
                                                         Row [
                                                              StretchedEl inputField
                                                              El ruleNames
                                                              El generateCheckBox
                                                              El slider
                                                              El fontSizeStepper
                                                              ]])]
                                     StretchedRow([StretchedEl scrollableTree])
                                        ]
    let size = form.Size.Width / 2
    webView.Size <- new Eto.Drawing.Size(size, form.Size.Height)
    let content = new Splitter(Panel1 = webView,(*Panel1MinimumSize =size , Panel2MinimumSize = size,*) Panel2 = layout)
    form.Content <- content
    form

    