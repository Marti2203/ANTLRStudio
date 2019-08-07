using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Antlr4.Runtime;
using CompilerOptions = System.Collections.Generic.IEnumerable<ANTLRStudio.Models.CompilerOption>;
using Antlr4.Runtime.Tree;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using ANTLRStudio.Models;

namespace ANTLRStudio.ANTLR
{
    public static class Tooling
    {
        public static string CreateArguments(string file, string language, CompilerOptions options)
        {
            var flags = options.Select(option => option.Value ? option.ActiveFlag : option.InactiveFlag);
            string[] strings = { $"\"{file}\"", language != null ? $"-Dlanguage={language}" : string.Empty };
            return string.Join(" ", strings.Concat(flags));
        }

        public static Process StartJavaProgram(string jarLocation, string programArguments)
        {
            string javaProcessArguments = $"-jar \"{jarLocation}\" {programArguments}";
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                FileName = Data.JavaRuntimeEnvironmentPath,
                Arguments = javaProcessArguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var osProcess = Process.Start(startInfo);
            return osProcess;
        }

        public static void GenerateFiles(string file, string language, CompilerOptions options)
        {
            string antlrArguments = CreateArguments(file, language, options);
            using (var antlrJavaProcess = StartJavaProgram(Data.AntlrLocation, antlrArguments))
            {
                antlrJavaProcess.WaitForExit();
                string processOutput = antlrJavaProcess.StandardOutput.ReadToEnd();
                Console.WriteLine(processOutput);
            }
        }

        private static DirectoryInfo GenerateTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Split('.')[0]);
            return Directory.CreateDirectory(tempDirectory);
        }
        public static (Parser parser, Lexer lexer, Assembly) GenerateParserLexerInMemory(string file)
        {

            var directory = GenerateTemporaryDirectory();

            CompilerOptions directoryOption = new[] {
                new CompilerOption("Output Directory Flag", true, "-o"),
                new CompilerOption("Output Directory Location", true, $"\"{directory.FullName}\""),
                new CompilerOption("Package Flag",true, "-package"),
                new CompilerOption("Package Name", true, "RandomAssembly"),
            };
            GenerateFiles(file, "CSharp", directoryOption);
            using (var provider = new CSharpCodeProvider())
            {

                string[] referenceAssemblies = { "System.dll", Path.Combine(Data.CurrentWorkingDirectory, "Antlr4.Runtime.Standard.dll") };
                string grammarName = file.Split('.')[0];
                string resultName = $"{grammarName}.dll";
                var parameters = new CompilerParameters(referenceAssemblies, resultName)
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false
                };
                var files = directory.EnumerateFiles("*.cs").Select(x => x.FullName).ToArray();
                var results = provider.CompileAssemblyFromFile(parameters, files);

                foreach (var error in Enumerable.Range(0, results.Errors.Count).Select(x => results.Errors[x]))
                {
                    Console.WriteLine(error);
                }
                foreach (var generatedFile in directory.EnumerateFiles())
                    generatedFile.Delete();
                directory.Delete();

                File.Delete(Path.Combine(Path.GetDirectoryName(file), resultName));
                var lexerClass = results.CompiledAssembly.GetTypes().First(t => t.IsSubclassOf(typeof(Lexer)));
                var lexerInstance = lexerClass.GetConstructor(new[] { typeof(ICharStream) }).Invoke(new object[] { null }) as Lexer;
                var parserClass = results.CompiledAssembly.GetTypes().First(t => t.IsSubclassOf(typeof(Parser)));
                var parserInstance = parserClass.GetConstructor(new[] { typeof(ITokenStream) }).Invoke(new object[] { null }) as Parser;
                return (parserInstance, lexerInstance, results.CompiledAssembly);
            }
        }

        public static (ITree tree, Parser parser) Parse(string data, string ruleName, Parser parser, Lexer lexer)
        {

            ICharStream stream = CharStreams.fromstring(data);
            lexer.SetInputStream(stream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();
            parser.BuildParseTree = true;
            parser.TokenStream = tokenStream;
            ITree tree = parser.GetType().GetMethod(ruleName).Invoke(parser, Array.Empty<object>()) as ITree;
            return (tree, parser);
        }
    }
}
