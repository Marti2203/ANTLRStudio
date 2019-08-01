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
        public readonly TextEditor _textEditor;
        public MainWindow()
        {
            InitializeComponent();
            _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.Background = Brushes.Aquamarine;
            _textEditor.ShowLineNumbers = true;
            _textEditor.TextArea.Foreground = Brushes.Black;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}