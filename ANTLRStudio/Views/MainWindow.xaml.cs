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
            TextEditor = this.FindControl<TextEditor>("Editor");
            TextEditor.Background = Brushes.Aquamarine;
            TextEditor.ShowLineNumbers = true;
            TextEditor.TextArea.Foreground = Brushes.Black;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}