using System;
using System.IO;
using System.Linq;
using ANTLRStudio.Models;
namespace ANTLRStudio.ANTLR
{
    public static class Data
    {
        public static (string Name, string Flag)[] Languages = {
                                            ("Java", null),
                                            ("C#", "CSharp"),
                                            ("JavaScript", "JavaScript"),
                                            ("Go", "Go"),
                                            ("C++", "Cpp"),
                                            ("Swift", "Swift"),
                    };
        public static CompilerOption[] CompilerOptions = {
        new CompilerOption("ATN", false ,"-atn",string.Empty ),
        new CompilerOption("Long Messages",  false ,  "-long-messages", string.Empty),
        new CompilerOption("Listener",  true ,  "-listener","-no-listener"),
        new CompilerOption("Visitor",  false ,  "-visitor","-no-visitor"),
        new CompilerOption("Generate Dependencies",  false,  "-depend",string.Empty),
        new CompilerOption("Treat Errors as warnings", false, "-Werror",string.Empty),
        new CompilerOption("Launch stringTemplate visualizer", false ,  "-XdbgST",string.Empty),
        new CompilerOption("Wait stringTemplate visualizer before contiunuing", false, "-XdbgSTWait",string.Empty),
        new CompilerOption("Force ATN Simulation", false, "-Xforce-atn",string.Empty),
        new CompilerOption("Dump loggin info",  false, "-Xlog",string.Empty),
        };
        public static string CurrentWorkingDirectory = Directory.GetCurrentDirectory();
        public static string CurrentAntlr = Directory.GetFiles(CurrentWorkingDirectory)
                                            .First(file => file.EndsWith("-complete.jar", StringComparison.Ordinal)
                                                        && file.Contains("antlr"));
        public static string AntlrLocation = Path.Combine(CurrentWorkingDirectory, CurrentAntlr);
        /// <summary>
        /// Gets the path of the JRE as Windows does not always have it in PATH.
        /// </summary>
        /// <value>The path for the JRE.</value>
        public static string JavaRuntimeEnvironmentPath => Environment.OSVersion.Platform != PlatformID.Win32NT ? "java"
                                   : @"C:\Program Files (x86)\Common Files\Oracle\Java\javapath\java.exe";
        public static string CSharpCompilerPath => "csc";
    }
}
