using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ANTLRStudio.Views;
using System.IO;
using System.Linq;
namespace ANTLRStudio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string GrammarPath { get; private set; }
        public string GrammarName { get; private set; }
        public async Task OpenGrammar(MainWindow window)
        {
            Console.WriteLine("HI!");
            Console.WriteLine(window is null);
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Open Antlr4 Grammar"
            };
            var g4Filter = new FileDialogFilter
            {
                Name = "Antlr4 files (.g4)",
                Extensions = new List<string> { "g4" }
            };
            dialog.Filters.Add(g4Filter);
            var res = await dialog.ShowAsync(window);
            if (res.Length == 1)
            {
                GrammarPath = res[0];
                GrammarName = GrammarPath.Split(Path.PathSeparator).Last();
                window.TextEditor.Load(res[0]);
            }
        }
        public async Task CloseGrammar(MainWindow window)
        {
            await window.TextEditor.SaveAsync(GrammarPath);
        }
    }
}
