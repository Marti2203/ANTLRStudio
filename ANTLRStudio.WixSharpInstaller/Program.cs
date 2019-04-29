using System;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;
using System.Linq;
using Path = System.IO.Path;
using System.Collections.Generic;

namespace ANTLRStudio.WixSharpInstaller
{
    public class Program
    {
        static readonly string AssemblyLocation = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        static readonly IEnumerable<string> Assemblies = new[] {
            "ANTLRStudio.exe",
            "FSharp.Core.dll",
            "Eto.dll",
            "Antlr4.Runtime.Standard.dll",
            "System.Runtime.Serialization.Primitives.dll",
            "ANTLRStudio.Trees.dll",
            "ANTLRStudio.TreeLayout.dll",
            "ANTLRStudio.Parser.dll"
        };
        public static string GraphicsBackend = "Eto.Wpf.dll";
        public static WixEntity[] shortcuts = new WixEntity[]
        {
             //new FileShortcut("ANTLRStudio.exe", "%Desktop%"),
             new ExeFileShortcut("Uninstall","[System64Folder]msiexec.exe","/x [ProductCode]"),
        };


        //static readonly RegValue InstalledValue = new RegValue("Software/ANTLRStudio", "Installed", 1) { ForceCreateOnInstall= true, ForceDeleteOnUninstall= true};
        static void Main()
        {
            // This project type has been superseded with the EmbeddedUI based "WixSharp Managed Setup - Custom Dialog"
            // project type. Which provides by far better final result and user experience.
            // However due to the Burn limitations (see this discussion: https://wixsharp.codeplex.com/discussions/645838)
            // currently "Custom CLR Dialog" is the only working option for having bootstrapper silent UI displaying
            // individual MSI packages UI implemented in managed code.
            string antlrJar = System.IO.Directory.GetFiles(AssemblyLocation, "antlr*-complete.jar")[0];

            var files = Assemblies.Concat(new string[] { GraphicsBackend, antlrJar })
                                           .Select(x => new File(Path.Combine(AssemblyLocation, x)) as WixEntity)
                                           .Concat(shortcuts)
                                           .ToArray();

            var project = new Project("ANTLRStudio", new Dir(@"%ProgramFiles%\ANTLRStudio", files))
            {
                GUID = Guid.NewGuid(),
                LaunchConditions = new List<LaunchCondition>() { new LaunchCondition("HASJAVA", "Java not installed  Please install JRE 1.6 or later.") },
            };

            var hasJava = new Property("HASJAVA",
                new RegistrySearch(RegistryHive.LocalMachine, @"SOFTWARE\JavaSoft\Java Runtime Environment", "EVersion", RegistrySearchType.raw), 
                new RegistrySearch(RegistryHive.LocalMachine, @"SOFTWARE\JavaSoft\Java Runtime Environment\Security Baseline", "1.8.0", RegistrySearchType.raw), 
                new RegistrySearch(RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\JavaSoft\Java Runtime Environment", "CurrentVersion", RegistrySearchType.raw));

            project.AddProperty(hasJava);
            project.AddProperty(new Property("GraphicsBackend", GraphicsBackend));

            project.InjectClrDialog("ShowCustomDialog", NativeDialogs.InstallDirDlg, NativeDialogs.VerifyReadyDlg);

            //remove LicenceDlg
            project.RemoveDialogsBetween(NativeDialogs.WelcomeDlg, NativeDialogs.InstallDirDlg);

            //project.AddRegValue(InstalledValue);

            //project.SourceBaseDir = "<input dir path>";

            project.OutDir = "bin";
            project.BuildMsi();
        }

        [CustomAction]
        public static ActionResult ShowCustomDialog(Session session)
        {
            return WixCLRDialog.ShowAsMsiDialog(new CustomDialog(session));
        }
    }
}