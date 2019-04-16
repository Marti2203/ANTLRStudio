﻿module Main
open System;
open Eto.Forms;
open Eto.Drawing;
open System.IO;
open System.Text;
open ANTLRStudio.TreeLayout;
open ANTLRStudio.TreeLayout.Utilities;
open ANTLRStudio.TreeLayout.Example;
let RadioMenuItem = Menu.RadioMenuItem
let CheckMenuItem = Menu.CheckMenuItem

let languages = [ 
                ("Java", null)
                ("C#", "CSharp")
                ("JavaScript", "JavaScript")
                ("Go", "Go")
                ("C++", "Cpp")
                ("Swift", "Swift")
                ]
let mutable file : string = null
let mutable language : string = null
type CompilerOption = { Name:string; mutable Value:bool; ActiveFlag:string; InactiveFlag: string}
let mutable options = [
                         { Name="ATN"; Value=false ; ActiveFlag="-atn";InactiveFlag=String.Empty}
                         { Name="Long Messages"; Value= false ; ActiveFlag= "-long-messages";InactiveFlag= String.Empty}
                         { Name="Listener"; Value= true ; ActiveFlag= "-listener";InactiveFlag="-no-listener"}
                         { Name="Visitor"; Value= false ; ActiveFlag= "-visitor";InactiveFlag= "-no-visitor"}
                         { Name="Generate Dependencies"; Value= false; ActiveFlag= "-depend";InactiveFlag= String.Empty}
                         { Name="Treat Errors as warnings"; Value=false; ActiveFlag="-Werror";InactiveFlag= String.Empty}
                         { Name="Launch StringTemplate visualizer"; Value=false ; ActiveFlag= "-XdbgST";InactiveFlag= String.Empty}
                         { Name="Wait StringTemplate visualizer before contiunuing"; Value=false; ActiveFlag="-XdbgSTWait";InactiveFlag=String.Empty}
                         { Name="Force ATN Simulation"; Value=false; ActiveFlag="-Xforce-atn";InactiveFlag= String.Empty}
                         { Name="Dump loggin info"; Value= false; ActiveFlag="-Xlog";InactiveFlag=String.Empty}
                      ]                

let transform () = 
    let flags = options 
                |> Seq.map(fun option -> if option.Value then option.ActiveFlag else option.InactiveFlag)
                |> Seq.toList 
    let strings = [file; (if language <> null then sprintf "-Dlanguage=%s" language else String.Empty)]
    String.Join(" ", Seq.concat [strings;flags])

let cwd = Directory.GetCurrentDirectory()
let currentAntlr = Directory.GetFiles(cwd) |> Array.find(fun file -> file.EndsWith("-complete.jar") && file.Contains("antlr"))
let antlrLocation = Path.Combine(cwd, currentAntlr)

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
let startJavaProgram(jarLocation: string, programArguments: string) =
    let javaProcessArguments =
        sprintf "-jar \"%s\" %s"
        <| antlrLocation
        <| programArguments

    let startInfo = new Diagnostics.ProcessStartInfo(WorkingDirectory = Directory.GetCurrentDirectory(),
                                                     FileName = "java",
                                                     Arguments = javaProcessArguments,
                                                     UseShellExecute = false,
                                                     RedirectStandardOutput=true,
                                                     CreateNoWindow = true)

    let osProcess = Diagnostics.Process.Start(startInfo);
    osProcess

let generate _ =
    let antlrArguments = transform ()
    use antlrJavaProcess = startJavaProgram (antlrLocation, antlrArguments)
    antlrJavaProcess.WaitForExit();
    let processOutput = antlrJavaProcess.StandardOutput.ReadToEnd()
    printfn "%s" <| processOutput

let mutable webView : WebView = null
let readGrammar name =
    use writer = new System.IO.StringWriter()
    printfn "%s" name
    file <- name
    (ANTLRStudio.Parser.AntlrParser.ParseFile file)
    |> Seq.map (fun x -> x.ToTuple())
    |> Seq.iter (fun (name,diagram) -> writer.Write(sprintf "<h1>%s</h1>\n" <| RailwayPortPy.escape(name))
                                       diagram.writeSvg(writer)
                                       writer.Write("\n") )
    writer.ToString() |> buildSvgHtml |> webView.LoadHtml
    
let openGrammar (form:Form) =
    let dir = Directory.GetCurrentDirectory() // Eto changes the directory?!
    use dialog = new OpenFileDialog(MultiSelect = false,
                                    Title = "Select Grammar",
                                    CheckFileExists = true,
                                    Directory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)))
    dialog.Filters.Add(new FileFilter("ANTLR Files (.g4)",".g4"))
    let mutable fileName = null
    match dialog.ShowDialog(form.ParentWindow) with
    | DialogResult.Ok -> if dialog.FileName <> null then
                            fileName <- dialog.FileName
    | v -> printf "User pressed %O" v
    if fileName <> null then readGrammar fileName
    Directory.SetCurrentDirectory(dir) // Eto changes the directory?!
let insertMenus (app:Application) (form:Form) =
    let transformCheckItem option =
        let item = CheckMenuItem(option.Name)
                   |> action (fun _ -> option.Value <- not option.Value )
        if not option.Value 
            then item 
        else (item |> check)

    let menu = new MenuBar()
    let menus = [
                SubMenu("File",
                    [
                    ActionMenuItem("Open") |> action (fun _ -> openGrammar form)                                         
                    ActionMenuItem("Quit") |> action (fun _ -> app.Quit())
                    ]) 
                SubMenu("Language", languages 
                                    |> Seq.map(fun (name,value) -> RadioMenuItem("Languages",name) 
                                                                   |> action (fun _ -> language <- value)))
                SubMenu("Options",  options |> Seq.map transformCheckItem)
                ActionMenuItem("Generate")  |> action generate
                ]
    menus |> Seq.iter (menu.Items.Add << makeMenu)
    form.Menu <- menu
    form

let railwayForm (app:Application) (form:Form) = 
    openGrammar form
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

[<STAThread>] // For Windows
[<EntryPoint>]
let main argv =
    let progName = "ANTLRStudio"
    Console.Title <- progName
    use app = new Application ()
    webView <- new WebView()
    app.UnhandledException.Add(fun _ -> ())
    use form = new Form (Title = progName, Size =Size(Screen.PrimaryScreen.Bounds.Size))

    //treeForm app form
    railwayForm app form
        |> insertMenus app 
        |> app.Run
    0
