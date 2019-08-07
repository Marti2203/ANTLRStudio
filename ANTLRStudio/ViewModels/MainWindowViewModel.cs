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
namespace ANTLRStudio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ANTLRMenuViewModel MenuViewModel { get; private set; } = new ANTLRMenuViewModel();

    }
}
