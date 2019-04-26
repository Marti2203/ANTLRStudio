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
    app.UnhandledException.Add(fun e -> printfn "%s" (e.ExceptionObject :?> Exception).StackTrace )
    use form = new Form (Title = progName, Size = Size(Screen.PrimaryScreen.Bounds.Size))

    setupInitialMenus app form
        |> mainForm app
        |> app.Run
    0
