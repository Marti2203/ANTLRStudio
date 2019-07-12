using System;
using Eto.Forms;
using Eto.Drawing;
using System.ComponentModel;
using Range = ANTLRStudio.Editor.Text.Range;
using FastColoredTextBoxNS;

namespace ANTLRStudio.Editor.AutoComplete
{
    /// <summary>
    /// Popup menu for autocomplete
    /// </summary>
    [Browsable(false)]
    public class AutocompleteMenu : Control, IDisposable
    {
        //public ToolBar host;
        public Range Fragment { get; internal set; }

        /// <summary>
        /// Regex pattern for serach fragment around caret
        /// </summary>
        public string SearchPattern { get; set; }
        /// <summary>
        /// Minimum fragment length for popup
        /// </summary>
        public int MinFragmentLength { get; set; }
        /// <summary>
        /// User selects item
        /// </summary>
        public event EventHandler<SelectingEventArgs> Selecting;
        /// <summary>
        /// It fires after item inserting
        /// </summary>
        public event EventHandler<SelectedEventArgs> Selected;
        /// <summary>
        /// Occurs when popup menu is opening
        /// </summary>
        public event EventHandler<CancelEventArgs> Opening;
        /// <summary>
        /// Allow TAB for select menu item
        /// </summary>
        public bool AllowTabKey { get => Items.AllowTabKey; set => Items.AllowTabKey = value; }
        /// <summary>
        /// Interval of menu appear (ms)
        /// </summary>
        public int AppearInterval { get => Items.AppearInterval; set => Items.AppearInterval = value; }
        /// <summary>
        /// Sets the max tooltip window size
        /// </summary>
        public Size MaxTooltipSize { get => Items.MaxToolTipSize; set => Items.MaxToolTipSize = value; }
        /// <summary>
        /// Tooltip will perm show and duration will be ignored
        /// </summary>
        public bool AlwaysShowTooltip { get => Items.AlwaysShowTooltip; set => Items.AlwaysShowTooltip = value; }

        /// <summary>
        /// Back color of selected item
        /// </summary>
        [DefaultValue(typeof(Color), "Orange")]
        public Color SelectedColor
        {
            get { return Items.SelectedColor; }
            set { Items.SelectedColor = value; }
        }

        /// <summary>
        /// Border color of hovered item
        /// </summary>
        [DefaultValue(typeof(Color), "Red")]
        public Color HoveredColor
        {
            get { return Items.HoveredColor; }
            set { Items.HoveredColor = value; }
        }

        public AutocompleteMenu(FastColoredTextBox tb)
        {
            // create a new popup and add the list view to it 
            AutoClose = false;
            AutoSize = false;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            BackColor = Colors.White;
            Items = new AutocompleteListView(tb);
            host = new ToolStripControlHost(Items);
            host.Margin = new Padding(2, 2, 2, 2);
            host.Padding = Padding.Empty;
            host.AutoSize = false;
            host.AutoToolTip = false;
            CalcSize();
            base.Items.Add(host);
            Items.Parent = this;
            SearchPattern = @"[\w\.]";
            MinFragmentLength = 2;

        }

        public new Font Font
        {
            get { return Items.Font; }
            set { Items.Font = value; }
        }

        new internal void OnOpening(CancelEventArgs args)
        {
            Opening?.Invoke(this, args);
        }

        public new void Close()
        {
            Items.toolTip.Hide(Items);
            base.Close();
        }

        internal void CalcSize()
        {
            host.Size = Items.Size;
            Size = new System.Drawing.Size(Items.Size.Width + 4, Items.Size.Height + 4);
        }

        public virtual void OnSelecting() => Items.OnSelecting();

        public void SelectNext(int shift) => Items.SelectNext(shift);

        internal void OnSelecting(SelectingEventArgs args) => Selecting?.Invoke(this, args);

        public void OnSelected(SelectedEventArgs args) => Selected?.Invoke(this, args);

        public new AutocompleteListView Items { get; }

        /// <summary>
        /// Shows popup menu immediately
        /// </summary>
        /// <param name="forced">If True - MinFragmentLength will be ignored</param>
        public void Show(bool forced) => Items.DoAutocomplete(forced);

        /// <summary>
        /// Minimal size of menu
        /// </summary>
        public new Size MinimumSize
        {
            get { return Items.MinimumSize; }
            set { Items.MinimumSize = value; }
        }

        /// <summary>
        /// Image list of menu
        /// </summary>
        public new ImageList ImageList
        {
            get { return Items.ImageList; }
            set { Items.ImageList = value; }
        }

        /// <summary>
        /// Tooltip duration (ms)
        /// </summary>
        public int ToolTipDuration
        {
            get { return Items.ToolTipDuration; }
            set { Items.ToolTipDuration = value; }
        }

        /// <summary>
        /// Tooltip
        /// </summary>
        public ToolItem ToolTip
        {
            get { return Items.toolTip; }
            set { Items.toolTip = value; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Items != null && !Items.IsDisposed)
                Items.Dispose();
        }

        public void Dispose() => Dispose(true);
    }
}
