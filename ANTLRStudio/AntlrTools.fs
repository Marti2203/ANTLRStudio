module AntlrTools
open System;
open System.IO
open System.CodeDom.Compiler;
open System.Reflection;
open Microsoft.CSharp;
open Antlr4.Runtime;
let createArguments file language options = 
    let flags = options 
                |> Seq.map(fun option -> if option.Value then option.ActiveFlag else option.InactiveFlag)
                |> Seq.toList 
    let strings = [sprintf "\"%s\"" file; (if language <> null then sprintf "-Dlanguage=%s" language else String.Empty)]
    String.Join(" ", Seq.concat [strings;flags])

let startJavaProgram(jarLocation: string, programArguments: string) =
    let javaProcessArguments =
        sprintf "-jar \"%s\" %s"
        <| antlrLocation
        <| programArguments

    let startInfo = new Diagnostics.ProcessStartInfo(WorkingDirectory = Directory.GetCurrentDirectory(),
                                                     FileName = java,
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

let generateTemporaryDirectory () =
   let tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Split('.').[0]);
   Directory.CreateDirectory(tempDirectory)
let generateParserLexerInMemory file =
    let directory = generateTemporaryDirectory()
    let directoryOption = [{
                            Name="Output Directory Flag"; Value= true; ActiveFlag="-o";InactiveFlag=String.Empty
                           }
                           {
                            Name="Output Directory Location"; Value= true; ActiveFlag=(sprintf "\"%s\"" directory.FullName);InactiveFlag=String.Empty
                           }
                           {
                            Name="Package Flag"; Value = true; ActiveFlag = "-package";InactiveFlag=String.Empty
                           }
                           {
                            Name="Package Name"; Value = true; ActiveFlag = "RandomAssembly";InactiveFlag=String.Empty
                           }]
    generate file "CSharp" directoryOption

    use provider = new CSharpCodeProvider()
    let referenceAssemblies = [|"System.dll";Path.Combine(cwd,"Antlr4.Runtime.Standard.dll")|];
    let grammarName = file.Split('.').[0]
    let resultName = sprintf "%s.dll" <| grammarName
    let parameters = new CompilerParameters(referenceAssemblies, resultName)
    parameters.GenerateInMemory <- true
    parameters.GenerateExecutable <- false
    let files = directory.EnumerateFiles("*.cs")
                |> Seq.map(fun x -> x.FullName)
                |> Seq.toArray
    let results = provider.CompileAssemblyFromFile(parameters,files)
    [0..(results.Errors.Count - 1)] |> Seq.map(fun x -> results.Errors.[x]) |> Seq.iter( fun x -> printfn "%A" x)
    directory.EnumerateFiles() |> Seq.iter (fun file -> file.Delete() )
    directory.Delete()
    Path.Combine(Path.GetDirectoryName(file),resultName) |> File.Delete
    let lexerClass = results.CompiledAssembly.GetTypes() |> Seq.find (fun t -> t.IsSubclassOf(typeof<Lexer>))
    let lexerInstance = lexerClass.GetConstructor([|typeof<ICharStream>|]).Invoke([|null|]) :?> Lexer
    let parserClass = results.CompiledAssembly.GetTypes() |> Seq.find (fun t -> t.IsSubclassOf(typeof<Parser>))
    let parserInstance = parserClass.GetConstructor([|typeof<ITokenStream>|]).Invoke([|null|]) :?> Parser
    (parserInstance,lexerInstance,results.CompiledAssembly)

let parse data ruleName (parser:Parser,lexer:Lexer) =
    if String.IsNullOrEmpty(data)
        then (null,parser)
    else
    let stream = CharStreams.fromstring(data)
    lexer.SetInputStream(stream)
    let tokenStream = new CommonTokenStream(lexer)
    tokenStream.Fill()
    parser.BuildParseTree <- true
    parser.TokenStream <- tokenStream
    try
        let output = (parser.GetType().GetMethod(ruleName).Invoke(parser,Array.empty))
        let tree = (output :?> Tree.ITree)
        (tree,parser)
    with 
    |  e -> (null,parser)
