module AntlrTools
open System;
open System.IO
open Models;
open System.CodeDom;
open System.CodeDom.Compiler;
open System.Reflection;
open Microsoft.CSharp;
open Antlr4.Runtime;
open Antlr4;
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

let generateParserLexerInMemory file data =
    let directory = Path.Combine(Directory.GetCurrentDirectory(),"Generations")
    if not <| Directory.Exists(directory) then Directory.CreateDirectory(directory) |> ignore
    let directoryOption = { Name="Output Directory"; Value= true; ActiveFlag="-o";InactiveFlag=String.Empty}
    generate file (languages |> Seq.find(fun (name, v) -> name = "C#") |> snd) [directoryOption]

    use provider = new CSharpCodeProvider()
    let referenceAssemblies = [|"System.dll" 
                                "Antlr4.Runtime.dll"
                                "Antlr4.Runtime.Standard.dll"|];
    let resultName = sprintf "%s.dll" <| file.Split('.').[0]
    let parameters = new CompilerParameters(referenceAssemblies, resultName)
    parameters.GenerateInMemory <- true
    parameters.GenerateExecutable <- false
    let results = provider.CompileAssemblyFromSource(parameters,Directory.GetFiles(directory) 
                                                                |> Seq.filter(fun x -> x.EndsWith(".cs")) 
                                                                |> Seq.toArray)
    let lexerClass = results.CompiledAssembly.GetTypes() |> Seq.find (fun t -> t.IsSubclassOf(typeof<Lexer>))
    let lexerInstance = lexerClass.GetConstructor([|typeof<ICharStream>|]).Invoke(null) :?> Lexer
    let parserClass = results.CompiledAssembly.GetTypes() |> Seq.find (fun t -> t.IsSubclassOf(typeof<Parser>))
    let parserInstance = lexerClass.GetConstructor([|typeof<ITokenStream>|]).Invoke(null) :?> Parser
    let stream = CharStreams.fromstring(data)
    lexerInstance.SetInputStream(stream)
    let tokenStream = new CommonTokenStream(lexerInstance)
    tokenStream.Fill()
    parserInstance.BuildParseTree <- true
    parserInstance.TokenStream <- tokenStream
    

    
