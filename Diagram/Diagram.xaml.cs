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
        private int itemsIndex = 0;
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
            if (items is List<string>)
            {
                this.CreateItemContainerGenerator();
                var generator = this.ItemContainerGenerator;
                if(e.NewItems != null)
                {
                    if (e.NewStartingIndex + e.NewItems.Count < items.Count)
                    {
                        generator.InsertSpace(e.NewStartingIndex, e.NewItems.Count);
                    }
                }

                foreach (var item in items)
                {
                    var panel = this.Panel;

                    var i = generator.Materialize(itemsIndex++, item, null);

                    if (i.ContainerControl != null)
                    {
                        if (i.Index < panel.Children.Count)
                        {
                            // TODO: This will insert at the wrong place when there are null items.
                            panel.Children.Insert(i.Index, i.ContainerControl);
                        }
                        else
                        {
                            panel.Children.Add(i.ContainerControl);
                        }
                    }
                }
            }
        }
    }
}
