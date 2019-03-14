// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
module Main
open System;
open Eto;
open Eto.Forms;
open Eto.Drawing;
open System.IO;
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
let antlrLocation = "./antlr-4.7.2-complete.jar"
let generate _ =
    printfn "Generate"
    use p = new Diagnostics.Process()
    let arguments = sprintf "%s \"%s\" %s" 
                    <| "java -jar"
                    <| antlrLocation 
                    <| transform ()
    p.StartInfo.WorkingDirectory <- Directory.GetCurrentDirectory()
    p.StartInfo.FileName <- "/bin/bash"
    p.StartInfo.Arguments <- sprintf "-c \"%s\"" arguments
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.CreateNoWindow <- true
    p.Start () |> ignore
    p.WaitForExit()
    printfn "%s" <| p.StandardOutput.ReadToEnd()
let readGrammar name =
    printfn "%s" name
    file <- name
let openGrammar (form:Form) =
    let dir = Directory.GetCurrentDirectory() // Eto changes the directory?!
    let dialog = new OpenFileDialog()
    dialog.MultiSelect <- false
    dialog.Title <- "Select Grammar"
    dialog.CheckFileExists <- true
    match dialog.ShowDialog(form.ParentWindow) with
    | DialogResult.Ok -> if dialog.FileName <> null then
                            readGrammar dialog.FileName
    | v -> printf "%O" v
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
    use writer = new System.IO.StringWriter()
    let web = new WebView()
    DiagramTest.test <| Some(writer :> System.IO.TextWriter)
    web.LoadHtml(writer.ToString())
    form.Content <- web
    form
[<EntryPoint>]
let main argv =
    let progName = "ANTLRStudio"
    Console.Title <- progName
    use app = new Application ()
    app.UnhandledException.Add(fun _ -> ())
    use form = new Form (Title = progName)
    form.Size <- Size(Screen.PrimaryScreen.Bounds.Size)

    railwayForm app form
        |> insertMenus app 
        |> app.Run
    0
