// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
module Main
open System;
open Eto.Forms;
open Eto.Drawing;
open System.IO;
open System.Text;
open System.Reflection;
open System.Linq;
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

let cwd = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)
let currentAntlr = Directory.GetFiles(cwd) |> Array.find(fun file -> file.EndsWith("-complete.jar") 
                                                                     && file.StartsWith("antlr"))
let antlrLocation = Path.Combine(cwd, currentAntlr)

let buildSvgHtml(svg : string)=
    //an SVG document will otherwise not show on Windows
    let document = new StringBuilder()
    document.Append("<html>")
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
    (ANTLRStudio.Parser.AntlrParser.ParseFile file).writeSvg(writer)
    writer.ToString() |> buildSvgHtml |> webView.LoadHtml
    
let openGrammar (form:Form) =
    let dir = Directory.GetCurrentDirectory() // Eto changes the directory?!
    use dialog = new OpenFileDialog(MultiSelect = false,
                                    Title = "Select Grammar",
                                    CheckFileExists = true,
                                    Directory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)))
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

[<STAThread>] // For Windows
[<EntryPoint>]
let main argv =
    let progName = "ANTLRStudio"
    Console.Title <- progName
    use app = new Application ()
    webView <- new WebView()
    app.UnhandledException.Add(fun _ -> ())
    use form = new Form (Title = progName, Size =Size(Screen.PrimaryScreen.Bounds.Size))

    railwayForm app form
        |> insertMenus app 
        |> app.Run
    0
