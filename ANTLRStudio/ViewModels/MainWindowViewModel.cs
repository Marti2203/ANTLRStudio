using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ANTLRStudio.Views;
using System.IO;
using System.Linq;
using ReactiveUI;
using ANTLRStudio.Diagram;
using Avalonia.Media;
using AvaloniaEdit.Document;

using Tree = Antlr4.Runtime.Tree.ITree;
using Antlr4.Runtime;
using ANTLRStudio.ANTLR;

namespace ANTLRStudio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ANTLRMenuViewModel MenuViewModel { get; private set; } = new ANTLRMenuViewModel();
        public List<string> TextContainers { get; set; } =
            new List<string> { "ListItemblahblahblahblahblah", "Listy2", "ThisItem" };
        public ISolidColorBrush EditorBackground { get; set; } = Brushes.LightGray;
        public ISolidColorBrush EditorForeground { get; set; } = Brushes.Black;
        public TextDocument EditorDocument { get; set; } = new TextDocument();
        Tree tree;
        public Tree Tree
        {
            get => tree; set
            {
                tree = value;
                this.RaisePropertyChanged(nameof(Tree));
            }
        }
        IList<string> ruleNames = Array.Empty<string>();
        public IEnumerable<string> RuleNames
        {
            get => ruleNames;
            set
            {
                ruleNames = new List<string>(value);
                this.RaisePropertyChanged(nameof(RuleNames));
            }
        }
        public string SelectedRule { get; set; }
        public Parser Parser { get; internal set; }
        public Lexer Lexer { get; internal set; }
        public void TestTrees()
        {
            Console.WriteLine("Testing!");
            (Tree treeNew, Parser parser) = Tooling.Parse(EditorDocument.Text, SelectedRule, Parser, Lexer);
            Tree = treeNew;
        }
    }

}
