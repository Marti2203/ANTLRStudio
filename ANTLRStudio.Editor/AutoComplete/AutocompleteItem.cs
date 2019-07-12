using System;
using Eto.Drawing;

namespace ANTLRStudio.Editor.AutoComplete
{
    /// <summary>
    /// Item of autocomplete menu
    /// </summary>
    public class AutocompleteItem
    {
        public string Text;
        public int ImageIndex = -1;
        public object Tag;
        string toolTipTitle;
        string toolTipText;
        string menuText;
        public AutocompleteMenu Parent { get; internal set; }


        public AutocompleteItem()
        {
        }

        public AutocompleteItem(string text)
        {
            Text = text;
        }

        public AutocompleteItem(string text, int imageIndex)
            : this(text)
        {
            ImageIndex = imageIndex;
        }

        public AutocompleteItem(string text, int imageIndex, string menuText)
            : this(text, imageIndex)
        {
            this.menuText = menuText;
        }

        public AutocompleteItem(string text, int imageIndex, string menuText, string toolTipTitle, string toolTipText)
            : this(text, imageIndex, menuText)
        {
            this.toolTipTitle = toolTipTitle;
            this.toolTipText = toolTipText;
        }

        /// <summary>
        /// Returns text for inserting into Textbox
        /// </summary>
        public virtual string GetTextForReplace() => Text;

        /// <summary>
        /// Compares fragment text with this item
        /// </summary>
        public virtual CompareResult Compare(string fragmentText) => Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) &&
                   Text != fragmentText
                ? CompareResult.VisibleAndSelected
                : CompareResult.Hidden;

        /// <summary>
        /// Returns text for display into popup menu
        /// </summary>
        public override string ToString() => menuText ?? Text;

        /// <summary>
        /// This method is called after item inserted into text
        /// </summary>
        public virtual void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
        {

        }

        /// <summary>
        /// Title for tooltip.
        /// </summary>
        /// <remarks>Return null for disable tooltip for this item</remarks>
        public virtual string ToolTipTitle
        {
            get { return toolTipTitle; }
            set { toolTipTitle = value; }
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        /// <remarks>For display tooltip text, ToolTipTitle must be not null</remarks>
        public virtual string ToolTipText
        {
            get { return toolTipText; }
            set { toolTipText = value; }
        }

        /// <summary>
        /// Menu text. This text is displayed in the drop-down menu.
        /// </summary>
        public virtual string MenuText
        {
            get { return menuText; }
            set { menuText = value; }
        }

        /// <summary>
        /// Fore color of text of item
        /// </summary>
        public virtual Color ForeColor
        {
            get => Colors.Transparent;
            set => throw new NotImplementedException("Override this property to change color");
        }

        /// <summary>
        /// Back color of item
        /// </summary>
        public virtual Color BackColor
        {
            get => Colors.Transparent;
            set => throw new NotImplementedException("Override this property to change color");
        }
    }

    public enum CompareResult
    {
        /// <summary>
        /// Item do not appears
        /// </summary>
        Hidden,
        /// <summary>
        /// Item appears
        /// </summary>
        Visible,
        /// <summary>
        /// Item appears and will selected
        /// </summary>
        VisibleAndSelected
    }

    /// <summary>
    /// This Item does not check correspondence to current text fragment.
    /// SuggestItem is intended for dynamic menus.
    /// </summary>
    public class SuggestItem : AutocompleteItem
    {
        public SuggestItem(string text, int imageIndex) : base(text, imageIndex)
        {
        }

        public override CompareResult Compare(string fragmentText) => CompareResult.Visible;
    }
}
