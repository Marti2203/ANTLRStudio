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
namespace ANTLRStudio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ANTLRMenuViewModel MenuViewModel { get; private set; } = new ANTLRMenuViewModel();
        public List<string> TextContainers { get; set; } =
            new List<string> { "ListItemblahblahblahblahblah", "Listy2", "ThisItem" };
        public ISolidColorBrush EditorBackground { get; set; } = Brushes.Aquamarine;
        public ISolidColorBrush EditorForeground { get; set; } = Brushes.Black;
    }
}
