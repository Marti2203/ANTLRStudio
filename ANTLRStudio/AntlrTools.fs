module AntlrTools
open System;
open System.IO
open Models;
let createArguments file language options = 
    let flags = options 
                |> Seq.map(fun option -> if option.Value then option.ActiveFlag else option.InactiveFlag)
                |> Seq.toList 
    let strings = [file; (if language <> null then sprintf "-Dlanguage=%s" language else String.Empty)]
    String.Join(" ", Seq.concat [strings;flags])

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

let generate file language options = 
    let antlrArguments = createArguments file language options
    use antlrJavaProcess = startJavaProgram (antlrLocation, antlrArguments)
    antlrJavaProcess.WaitForExit()
    let processOutput = antlrJavaProcess.StandardOutput.ReadToEnd()
    printfn "%s" <| processOutput