module FormElements
open System;
open System.IO;
open Eto.Forms;
open System.Text;
open ANTLRStudio.Parser;
open ANTLRStudio.TreeLayout;
open ANTLRStudio.TreeLayout.Utilities;
open ANTLRStudio.TreeLayout.Example;
open State;
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
    |> Seq.map (fun x -> x.ToTuple())
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
                            fileName |> readGrammarToHtml |> Some
                         else None
    | v -> printf "User pressed %O" v
           None


let railwayForm (app:Application) (form:Form) = 
    let webView = new WebView()
    match openGrammar form with
    | Some (data) -> webView.LoadHtml(data)
    | None -> ()
    form.Content <- webView
    form

let treeForm (app:Application) (form:Form) =
    let tree = SampleTreeFactory.ASTTree()
    let gapBetweenLevels = 50.f
    let gapBetweenNodes = 10.f
    let configuration = new DefaultConfiguration<TextInBox>(gapBetweenLevels, gapBetweenNodes, Configuration<TextInBox>.Location.Left)

    // create the NodeExtentProvider for TextInBox nodes
    let nodeExtentProvider = new TextInBoxNodeExtentProvider()

    // create the layout
    let treeLayout = new TreeLayout<TextInBox>(tree, nodeExtentProvider, configuration)

    // Create a panel that draws the nodes and edges and show the panel
    let panel = new TextInBoxTreePane(treeLayout)
    form.Content <- panel
    form