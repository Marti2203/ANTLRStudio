using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using ANTLRStudio.ViewModels;
using ANTLRStudio.Models;
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
            TextEditor.Background = Brushes.Aquamarine;
            TextEditor.ShowLineNumbers = true;
            TextEditor.TextArea.Foreground = Brushes.Black;
            TextEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy();

            ANTLRMenuViewModel.GrammarOpened += OnGrammarOpened;
            ANTLRMenuViewModel.GrammarClosed += OnGrammarClosed;
        }

        private void OnGrammarOpened(GrammarOpenedEventArgs e)
        {
            TextEditor.Load(e.GrammarPath);
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