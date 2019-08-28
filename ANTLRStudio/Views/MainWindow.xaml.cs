using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using ANTLRStudio.ViewModels;
using ANTLRStudio.Models;
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