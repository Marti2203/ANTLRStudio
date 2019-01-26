// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
module Main
open System;
open Eto;
open Eto.Forms;
open Eto.Drawing;
let insertMenus (app:Application) (form:Form) =
    let menu = new MenuBar()
    let menus = [
                SubMenu("File",
                    [
                    ActionMenuItem("Open") |> action (fun _ -> Console.WriteLine("HI!"))
                    ActionMenuItem("Quit") |> action (fun _ -> app.Quit())
                    ])
                ]
    menus |> Seq.iter (menu.Items.Add << makeMenu)
    form.Menu <- menu
    form

let textEditorForm (app:Application) =
    let form = new Form()
    let textArea = new TextArea()
    form.Content <- textArea
    form

let railwayForm (app:Application) (form:Form) = 
    let docExample = //SvgDocument.Open(
        """<svg class="railroad-diagram" width="532.5" height="109" viewBox="0 0 532.5 109">"""
        +
        sprintf "<style>%s</style>" RailwayPortPy.DEFAULT_STYLE
        +
        """
    <g transform="translate(.5 .5)">
    <g>
    <path d="M20 30v20m10 -20v20m-10 -10h20"></path>
    </g>
    <g>
    <path d="M40 40h0"></path>
    <path d="M108.5 40h0"></path>
    <path d="M40 40h20"></path>
    <g>
    <path d="M60 40h28.5"></path>
    </g>
    <path d="M88.5 40h20"></path>
    <path d="M40 40a10 10 0 0 1 10 10v0a10 10 0 0 0 10 10"></path>
    <g class="terminal">
    <path d="M60 60h0"></path>
    <path d="M88.5 60h0"></path>
    <rect x="60" y="49" width="28.5" height="22" rx="10" ry="10"></rect>
    <text x="74.25" y="64">+</text>
    </g>
    <path d="M88.5 60a10 10 0 0 0 10 -10v0a10 10 0 0 1 10 -10"></path>
    </g>
    <g>
    <path d="M108.5 40h0"></path>
    <path d="M296 40h0"></path>
    <path d="M108.5 40h20"></path>
    <g class="non-terminal">
    <path d="M128.5 40h0"></path>
    <path d="M276 40h0"></path>
    <rect x="128.5" y="29" width="147.5" height="22"></rect>
    <text x="202.25" y="44">name-start char</text>
    </g>
    <path d="M276 40h20"></path>
    <path d="M108.5 40a10 10 0 0 1 10 10v10a10 10 0 0 0 10 10"></path>
    <g class="non-terminal">
    <path d="M128.5 70h38.25"></path>
    <path d="M237.75 70h38.25"></path>
    <rect x="166.75" y="59" width="71" height="22"></rect>
    <text x="202.25" y="74">escape</text>
    </g>
    <path d="M276 70a10 10 0 0 0 10 -10v-10a10 10 0 0 1 10 -10"></path>
    </g>
    <g>
    <path d="M296 40h0"></path>
    <path d="M492.5 40h0"></path>
    <path d="M296 40a10 10 0 0 0 10 -10v0a10 10 0 0 1 10 -10"></path>
    <g>
    <path d="M316 20h156.5"></path>
    </g>
    <path d="M472.5 20a10 10 0 0 1 10 10v0a10 10 0 0 0 10 10"></path>
    <path d="M296 40h20"></path>
    <g>
    <path d="M316 40h0"></path>
    <path d="M472.5 40h0"></path>
    <path d="M316 40h10"></path>
    <g>
    <path d="M326 40h0"></path>
    <path d="M462.5 40h0"></path>
    <path d="M326 40h20"></path>
    <g class="non-terminal">
    <path d="M346 40h0"></path>
    <path d="M442.5 40h0"></path>
    <rect x="346" y="29" width="96.5" height="22"></rect>
    <text x="394.25" y="44">name char</text>
    </g>
    <path d="M442.5 40h20"></path>
    <path d="M326 40a10 10 0 0 1 10 10v10a10 10 0 0 0 10 10"></path>
    <g class="non-terminal">
    <path d="M346 70h12.75"></path>
    <path d="M429.75 70h12.75"></path>
    <rect x="358.75" y="59" width="71" height="22"></rect>
    <text x="394.25" y="74">escape</text>
    </g>
    <path d="M442.5 70a10 10 0 0 0 10 -10v-10a10 10 0 0 1 10 -10"></path>
    </g>
    <path d="M462.5 40h10"></path>
    <path d="M326 40a10 10 0 0 0 -10 10v29a10 10 0 0 0 10 10"></path>
    <g>
    <path d="M326 89h136.5"></path>
    </g>
    <path d="M462.5 89a10 10 0 0 0 10 -10v-29a10 10 0 0 0 -10 -10"></path>
    </g>
    <path d="M472.5 40h20"></path>
    </g>
    <path d="M 492.5 40 h 20 m -10 -10 v 20 m 10 -20 v 20"></path>
    </g>
    </svg> """ //)
    use writer = new System.IO.StringWriter()
    let web = new WebView()
    DiagramTest.test <| Some(writer :> System.IO.TextWriter)
    web.LoadHtml(writer.ToString())
    form.Content <- web
    form
//open Svg
//let diagramForm (app:Application) (form:Form) =
    //if float32 doc.Height > float32 form.Size.Height then
    //    doc.Width <- SvgUnit((float32 doc.Width / float32 doc.Height) * float32 form.Size.Height)
    //    doc.Height <- SvgUnit(float32 form.Size.Height)
    //doc.Draw()
    ////let web = new WebView()
    //////printf "%s" <| web.ExecuteScript(System.IO.File.ReadAllText("railroad.js"))
    ////web.LoadHtml(System.IO.File.ReadAllText("example.html"))
    ////form.Content <- web
    ////web.DocumentLoaded.Add(fun _ -> web.Reload() ; printfn "RELOAD!")
    //form
[<EntryPoint>]
let main argv =
    let progName = "ANTLRStudio"
    Console.Title <- progName
    use app = new Application ()
    app.UnhandledException.Add(fun _ -> ())
    use form = new Form (Title = progName)
    form.Size <- Size(Screen.PrimaryScreen.Bounds.Size)
    //DiagramTest.test()
    railwayForm app form
        |> insertMenus app 
        |> app.Run
    
    //printfn "%A" argv
    0 // return an integer exit code
