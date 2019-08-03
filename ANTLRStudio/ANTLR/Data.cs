using System;
using System.IO;
using System.Linq;
namespace ANTLRStudio.ANTLR
{
    public static class Data
    {
        public static (string, string)[] Languages = {
                                            ("Java", null),
                                            ("C#", "CSharp"),
                                            ("JavaScript", "JavaScript"),
                                            ("Go", "Go"),
                                            ("C++", "Cpp"),
                                            ("Swift", "Swift"),
                    };
        public static (string Name, bool Value, string ActiveFlag, string InactiveFlag)[] CompilerOptions = {
        (Name : "ATN", Value:false ,ActiveFlag:"-atn",InactiveFlag:string.Empty ),
        (Name:"Long Messages", Value: false , ActiveFlag: "-long-messages",InactiveFlag: string.Empty),
        (Name:"Listener", Value: true , ActiveFlag: "-listener",InactiveFlag:"-no-listener"),
        (Name:"Visitor", Value: false , ActiveFlag: "-visitor",InactiveFlag: "-no-visitor"),
        (Name:"Generate Dependencies", Value: false, ActiveFlag: "-depend",InactiveFlag: string.Empty),
        (Name:"Treat Errors as warnings", Value:false, ActiveFlag:"-Werror",InactiveFlag: string.Empty),
        (Name:"Launch stringTemplate visualizer", Value:false , ActiveFlag: "-XdbgST",InactiveFlag: string.Empty),
        (Name:"Wait stringTemplate visualizer before contiunuing", Value:false, ActiveFlag:"-XdbgSTWait",InactiveFlag:string.Empty),
        (Name:"Force ATN Simulation", Value:false, ActiveFlag:"-Xforce-atn",InactiveFlag: string.Empty),
        (Name:"Dump loggin info", Value: false, ActiveFlag:"-Xlog",InactiveFlag:string.Empty),
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
    }
}
