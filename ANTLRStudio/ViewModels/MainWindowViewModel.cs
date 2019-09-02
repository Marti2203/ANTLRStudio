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
using Avalonia.Collections;

namespace ANTLRStudio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ANTLRMenuViewModel MenuViewModel { get; private set; } = new ANTLRMenuViewModel();

        public AvaloniaList<string> TextContainers { get; set; }
            = new AvaloniaList<string> { "ListItemblahblahblahblahblah", "Listy2", "ThisItem" };

        public void RemoveItem()
        {
            TextContainers.Add("New Item");
        }
    }
}
