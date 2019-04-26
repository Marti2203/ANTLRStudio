﻿module Main
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
 
    use form = new Form (Title = progName, Size = Size(Screen.PrimaryScreen.Bounds.Size))
    app.UnhandledException.Add(fun e -> let e = (e.ExceptionObject :?> Exception)
                                        printfn "%s %s %s" e.Message Environment.NewLine e.StackTrace )
    setupInitialMenus app form
        |> mainForm app
        |> app.Run
    0
