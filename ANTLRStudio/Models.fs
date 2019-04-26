[<AutoOpen>]
module Models
open System.IO;
open System;
type CompilerOption = { Name:string; mutable Value:bool; ActiveFlag:string; InactiveFlag: string }
let languages = [ 
                ("Java", null)
                ("C#", "CSharp")
                ("JavaScript", "JavaScript")
                ("Go", "Go")
                ("C++", "Cpp")
                ("Swift", "Swift")
                ]
let options = [
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
let cwd = Directory.GetCurrentDirectory()
let currentAntlr = Directory.GetFiles(cwd) |> Array.find(fun file -> file.EndsWith("-complete.jar") && file.Contains("antlr"))
let antlrLocation = Path.Combine(cwd, currentAntlr)
let java = if Environment.OSVersion.Platform <> PlatformID.Win32NT 
           then 
                "java" 
           else @"C:\Program Files (x86)\Common Files\Oracle\Java\javapath\java.exe"
