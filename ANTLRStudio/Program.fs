module Main
open System;
open Eto.Forms;
open Eto.Drawing;
open MenuOperations;
open FormElements;

[<STAThread>] // For Windows
[<EntryPoint>]
let main argv =
    let progName = "ANTLRStudio"
    Console.Title <- progName
    use app = new Application ()
    app.UnhandledException.Add(fun _ -> ())
    use form = new Form (Title = progName, Size = Size(Screen.PrimaryScreen.Bounds.Size))

    //treeForm app form
    setupInitialMenus app form
        |> railwayForm app
        |> app.Run
    0
