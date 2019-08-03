using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ANTLRStudio.Views;
using Avalonia.Controls;
using Avalonia;

namespace ANTLRStudio.ViewModels
{
    public class ANTLRMenuViewModel : ViewModelBase
    {
        public string GrammarPath { get; private set; }
        public string GrammarName { get; private set; }
        public bool HasGrammarOpen => GrammarName != null;

        private static readonly FileDialogFilter g4Filter = new FileDialogFilter
        {
            Name = "Antlr4 files (.g4)",
            Extensions = new List<string> { "g4" }
        };

        private static readonly OpenFileDialog dialog = new OpenFileDialog
        {
            AllowMultiple = false,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Open Antlr4 Grammar",
            Filters = new List<FileDialogFilter> { g4Filter }
        };

        public async Task OpenGrammar(MainWindow window)
        {
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
            if (!HasGrammarOpen) return;
            GrammarName = null;
            GrammarPath = null;
            await window.TextEditor.SaveAsync(GrammarPath);
        }
        public void Exit() => Application.Current.Exit();
    }
}
