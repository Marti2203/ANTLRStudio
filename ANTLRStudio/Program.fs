module Main
open System;
open Eto.Forms;
open Eto.Drawing;
open AntlrTools;
open FormElements;
open MenuOperations;
[<STAThread>] // For Windows
[<EntryPoint>]
let main argv =
    let progName = "ANTLRStudio"
    Console.Title <- progName
    use app = new Application ()
    use form = new Form (Title = progName, Size = Size(Screen.PrimaryScreen.Bounds.Size))
    app.UnhandledException.Add(fun e -> let e = (e.ExceptionObject :?> Exception)
                                        displayException e)
    compilationErrors.Add(fun ers ->let text = (new Text.StringBuilder(),ers) 
                                               ||> Seq.fold (fun (curr:Text.StringBuilder) next -> curr.Append(next.ErrorText))
                                    MessageBox.Show(form,text.ToString(), "Could not parse generated grammar.",MessageBoxType.Error) |> ignore)
    setupInitialMenus app form
        |> mainForm app
        |> app.Run
    0
