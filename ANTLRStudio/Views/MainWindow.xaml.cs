using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using System;

namespace ANTLRStudio.Views
{
    public class MainWindow : Window
    {
        public readonly TextEditor TextEditor;
        public MainWindow()
        {
            InitializeComponent();

            _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.Background = Brushes.Aquamarine;
            _textEditor.ShowLineNumbers = true;
            _textEditor.TextArea.Foreground = Brushes.Black;
            _textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}