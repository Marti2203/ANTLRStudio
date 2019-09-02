using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using ANTLRStudio.ViewModels;
using ANTLRStudio.Models;
using ANTLRStudio.ANTLR;
using Antlr4.Runtime;
using System.Reflection;
using System.CodeDom.Compiler;
using System;
namespace ANTLRStudio.Views
{
    public class MainWindow : Window
    {
        public readonly TextEditor TextEditor;
        public MainWindow()
        {
            InitializeComponent();
            TextEditor = this.FindControl<TextEditor>("Editor");
            ANTLRMenuViewModel.GrammarOpened += OnGrammarOpened;
            ANTLRMenuViewModel.GrammarClosed += OnGrammarClosed;
        }

        private void OnGrammarOpened(GrammarOpenedEventArgs e)
        {
            //TextEditor.Load(e.GrammarPath);
            (Parser p, Lexer l, Assembly a, CompilerErrorCollection errors) = Tooling.GenerateParserLexerInMemory(e.GrammarPath);
            if (!errors.HasErrors)
            {
                Console.WriteLine("GENERATED!!!");
                var vm = (this.DataContext as MainWindowViewModel);
                vm.RuleNames = p.RuleNames;
                this.FindControl<ComboBox>("Rules").SelectedItem = p.RuleNames[0];
                vm.Parser = p;
                vm.Lexer = l;
            }
        }
        private void OnGrammarClosed(GrammarClosedEventArgs e)
        {
            TextEditor.Save(e.GrammarPath);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}