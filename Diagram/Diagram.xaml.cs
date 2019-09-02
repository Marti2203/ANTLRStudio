using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ANTLRStudio.Diagram
{
    public class Diagram : ItemsPresenterBase, IItemsPresenter
    {
        public Diagram()
        {
            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        protected override void ItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            List<string> items = (List<string>)this.Items;
            ItemSync.ItemsChanged(this, items, e);
        }
    }
}
