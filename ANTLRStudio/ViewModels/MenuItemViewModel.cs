using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;

namespace ANTLRStudio.ViewModels
{
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public IList<MenuItemViewModel> Items { get; set; }
        public object Icon { get; set; }
    }
}
