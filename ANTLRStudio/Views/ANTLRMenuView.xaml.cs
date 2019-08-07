using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ANTLRStudio.ViewModels;

namespace ANTLRStudio.Views
{
    public class ANTLRMenuView : UserControl
    {
        public ANTLRMenuView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}