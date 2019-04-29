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
            "Eto.Wpf.dll",
            "Antlr4.Runtime.Standard.dll",
            "System.Runtime.Serialization.Primitives.dll",
            "ANTLRStudio.Trees.dll",
            "ANTLRStudio.TreeLayout.dll",
            "ANTLRStudio.Parser.dll"
        };
        public static string GraphicsBackend = "Eto.Wpf.dll";
        public static WixEntity[] Shortcuts = new WixEntity[]
        {
             new ExeFileShortcut("Uninstall","[System64Folder]msiexec.exe","/x [ProductCode]"),
        };

        static void Main()
        {
            // This project type has been superseded with the EmbeddedUI based "WixSharp Managed Setup - Custom Dialog"
            // project type. Which provides by far better final result and user experience.
            // However due to the Burn limitations (see this discussion: https://wixsharp.codeplex.com/discussions/645838)
            // currently "Custom CLR Dialog" is the only working option for having bootstrapper silent UI displaying
            // individual MSI packages UI implemented in managed code.
            string antlrJar = System.IO.Directory.GetFiles(AssemblyLocation, "antlr*-complete.jar")[0];

            var files = Assemblies.Concat(new string[] { antlrJar })
                                  .Select(x => new File(new Id(Path.GetFileNameWithoutExtension(x).Replace('-', '_')),
                                                               Path.Combine(AssemblyLocation, x)) as WixEntity)
                                  .Concat(Shortcuts)
                                  .ToArray();
            (files.First(x => x.Id == "ANTLRStudio") as File)
                .AddShortcuts(new FileShortcut("ANTLRStudio.exe", "%Desktop%"), 
                              new FileShortcut("ANTLRStudio.exe", @"%ProgramMenu%\ANTLRStudio"));


            var hasJava = new Property("HASJAVA",
                new RegistrySearch(RegistryHive.LocalMachine, @"SOFTWARE\JavaSoft\Java Runtime Environment", "EVersion", RegistrySearchType.raw),
                new RegistrySearch(RegistryHive.LocalMachine, @"SOFTWARE\JavaSoft\Java Runtime Environment\Security Baseline", "1.8.0", RegistrySearchType.raw),
                new RegistrySearch(RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\JavaSoft\Java Runtime Environment", "CurrentVersion", RegistrySearchType.raw));


            var project = new Project("ANTLRStudio", 
                                      hasJava, 
                                      new Dir(@"%ProgramFiles%\ANTLRStudio", files),
                                      new Dir(@"%ProgramMenu%\ANTLRStudio", new ExeFileShortcut("Uninstall", "[System64Folder]msiexec.exe", "/x [ProductCode]")))
            {
                GUID = Guid.NewGuid(),
                LaunchConditions = new List<LaunchCondition>() { new LaunchCondition("HASJAVA", "Java not installed  Please install JRE 1.6 or later.") },
            };

            project.ControlPanelInfo.Manufacturer = "Koicho Georgiev and Martin Mirchev";
            project.ControlPanelInfo.Contact = "marti_2203@abv.bg";
            project.InjectClrDialog("ShowCustomDialog", NativeDialogs.InstallDirDlg, NativeDialogs.VerifyReadyDlg);

            //remove LicenceDlg
            project.RemoveDialogsBetween(NativeDialogs.WelcomeDlg, NativeDialogs.InstallDirDlg);

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