using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Antlr4.Runtime;
using CompilerOptions = System.Collections.Generic.IEnumerable<ANTLRStudio.Models.CompilerOption>;
using Antlr4.Runtime.Tree;
using System.Reflection;
using ANTLRStudio.Models;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace ANTLRStudio.ANTLR
{
    public static class Tooling
    {
        public static string CreateArguments(string filePath, string language, CompilerOptions options)
        {
            var flags = options.Select(option => option.Value ? option.ActiveFlag : option.InactiveFlag);
            string[] strings = { $"\"{filePath}\"", language != null ? $"-Dlanguage={language}" : string.Empty };
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

        public static Process StartCSharpCompiler(string resultName, string directory, params string[] assemblies)
        {
            string cSharpProcessArguments = $"-nologo -t:library -out:{resultName}.dll -debug -recurse:*.cs {string.Join(' ', assemblies.Select(x => $"-reference:{x}")) }";
            Console.WriteLine(directory);
            Console.WriteLine(cSharpProcessArguments);
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = directory,
                FileName = Data.CSharpCompilerPath,
                Arguments = cSharpProcessArguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var osProcess = Process.Start(startInfo);
            return osProcess;
        }

        public static void GenerateFiles(string filePath, string language, CompilerOptions options)
        {
            string antlrArguments = CreateArguments(filePath, language, options);
            using (var antlrJavaProcess = StartJavaProgram(Data.AntlrLocation, antlrArguments))
            {
                antlrJavaProcess.WaitForExit();
                string processOutput = antlrJavaProcess.StandardOutput.ReadToEnd();
                Console.WriteLine(processOutput);
            }
        }

        public static (Parser parser, Lexer lexer, Assembly resultingAssembly, CompilerErrorCollection errors) GenerateParserLexerInMemory(string filePath)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            var directory = GenerateTemporaryDirectory();
            string generationDependenciesFolder = Path.Combine(Directory.GetCurrentDirectory(), "GenerationDependencies");
            string grammarName = Path.GetFileNameWithoutExtension(filePath);
            string resultName = Path.Combine(directory.FullName, $"{grammarName}.dll");

            CompilerOptions directoryOption = new[] {
            new CompilerOption("Output Directory Flag", true, "-o"),
            new CompilerOption("Output Directory Location", true, $"\"{directory.FullName}\""),
            new CompilerOption("Package Flag",true, "-package"),
            new CompilerOption("Package Name", true, "RandomAssembly"),
            };

            GenerateFiles(filePath, "CSharp", directoryOption);
            
            string[] referenceAssemblies = {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll",
                Path.Combine(generationDependenciesFolder,"Antlr4.Runtime.Standard.dll"),
                Path.Combine(generationDependenciesFolder,"System.IO.dll"),
                Path.Combine(generationDependenciesFolder, "System.Runtime.dll"),
            };

            using (var compilerProcess = StartCSharpCompiler(grammarName, directory.FullName, referenceAssemblies))
            {
                compilerProcess.WaitForExit();
                string processOutput = compilerProcess.StandardOutput.ReadToEnd();
                Console.WriteLine(processOutput);
                var errors = processOutput.Split('\n');
                CompilerErrorCollection compilerErrors =
                    new CompilerErrorCollection(errors.Select(x => new CompilerError(null, 0, 0, null, x)).ToArray());
                if (errors.Any(x => x.Contains("error", StringComparison.InvariantCultureIgnoreCase)))
                    return (null, null, null, compilerErrors);
                //using (var provider = new CSharpCodeProvider())
                //{

                //var parameters = new CompilerParameters(referenceAssemblies, resultName)
                //{
                //    GenerateInMemory = true,
                //    GenerateExecutable = false
                //};
                //var files = directory.EnumerateFiles("*.cs")
                //                     .Select(x => x.FullName)
                //                     .ToArray();
                //var results = provider.CompileAssemblyFromFile(parameters, files);

                var compiledAssembly = Assembly.LoadFrom(resultName);

                //foreach (var error in Enumerable.Range(0, results.Errors.Count).Select(x => results.Errors[x]))
                //{
                //    Console.WriteLine(error);
                //}
                //if (results.Errors.HasErrors)
                //    return (null, null, null, results.Errors);
                //foreach (var generatedFile in directory.EnumerateFiles())
                //    generatedFile.Delete();
                //directory.Delete();

                /// WINDOWS DOES NOT LIKE A DLL TO BE DELETED
                //directory.Delete(true);
                var lexerClass = compiledAssembly
                                        .GetTypes()
                                        .First(t => t.IsSubclassOf(typeof(Lexer)));
                var lexerInstance = lexerClass.GetConstructor(new[] { typeof(ICharStream) })
                                              .Invoke(new object[] { null }) as Lexer;
                var parserClass = compiledAssembly
                                         .GetTypes()
                                         .First(t => t.IsSubclassOf(typeof(Parser)));
                var parserInstance = parserClass.GetConstructor(new[] { typeof(ITokenStream) })
                                                .Invoke(new object[] { null }) as Parser;
                return (parserInstance, lexerInstance, compiledAssembly, compilerErrors);
            }

        }

        public static (ITree tree, Parser parser) Parse(string data, string ruleName, Parser parser, Lexer lexer)
        {
            Console.WriteLine("Parsing!");
            ICharStream stream = CharStreams.fromstring(data);
            lexer.SetInputStream(stream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();
            parser.BuildParseTree = true;
            parser.TokenStream = tokenStream;
            ITree tree = parser.GetType().GetMethod(ruleName).Invoke(parser, Array.Empty<object>()) as ITree;
            Console.WriteLine(tree is null);
            return (tree, parser);
        }

        private static DirectoryInfo GenerateTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Split('.')[0]);
            return Directory.CreateDirectory(tempDirectory);
        }

    }
}
