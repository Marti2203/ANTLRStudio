using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ANTLRStudio.Drawing
{
    class StructuredCanvas : Canvas
    {
        public StructuredCanvas()
        {

        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = Children;
            double previousChildSize = 0.0;
            var spacing = 10;

            //
            // Arrange and Position Children.
            //
            for (int i = 0, count = children.Count; i < count; ++i)
            {
                var child = children[i];

                if (child == null)
                { continue; }

                SetLeft(child as AvaloniaObject, previousChildSize);
                previousChildSize += child.DesiredSize.Width;
                previousChildSize += spacing;
            }

            base.ArrangeOverride(finalSize);

            return finalSize;
        }
    }
}
