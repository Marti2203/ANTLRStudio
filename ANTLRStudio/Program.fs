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
[<EntryPoint>]
let main argv =
    let progName = "ANTLRStudio"
    Console.Title <- progName
    let app = new Application ()

    textEditorForm app 
        |> fun form -> form.Title <- progName; form
        |> fun form -> form.Size <- Size(Screen.PrimaryScreen.Bounds.Size); form
        |> insertMenus app 
        |> app.Run
    printfn "%A" argv
    0 // return an integer exit code
