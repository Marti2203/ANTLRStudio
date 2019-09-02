using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ANTLRStudio.Diagram
{
    public class Diagram : ItemsPresenterBase
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
            AvaloniaList<string> items = (AvaloniaList<string>)this.Items;
            ItemSync.ItemsChanged(this, items, e);
        }
    }
}
