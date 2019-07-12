using System.Collections.Generic;
using System.Text;
using ANTLRStudio.Editor.Args;
using ANTLRStudio.Editor.Forms;

namespace ANTLRStudio.Editor.Bookmarks
{

    /// <summary>
    /// Collection of bookmarks
    /// </summary>
    public class Bookmarks : BaseBookmarks
    {
        protected FastColoredTextBox tb;
        protected List<Bookmark> items = new List<Bookmark>();
        protected int counter;

        public Bookmarks(FastColoredTextBox tb)
        {
            this.tb = tb;
            tb.LineInserted += Tb_LineInserted;
            tb.LineRemoved += Tb_LineRemoved;
        }

        protected virtual void Tb_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            for (int i = 0; i < Count; i++)
                if (items[i].LineIndex >= e.Index)
                {
                    if (items[i].LineIndex >= e.Index + e.Count)
                    {
                        items[i].LineIndex = items[i].LineIndex - e.Count;
                        continue;
                    }

                    var was = e.Index <= 0;
                    foreach (var b in items)
                        was |= b.LineIndex == e.Index - 1;

                    if (was)
                    {
                        items.RemoveAt(i);
                        i--;
                    }
                    else
                        items[i].LineIndex = e.Index - 1;

                    //if (items[i].LineIndex == e.Index + e.Count - 1)
                    //{
                    //    items[i].LineIndex = items[i].LineIndex - e.Count;
                    //    continue;
                    //}
                    //
                    //items.RemoveAt(i);
                    //i--;
                }
        }

        protected virtual void Tb_LineInserted(object sender, LineInsertedEventArgs e)
        {
            for (int i = 0; i < Count; i++)
                if (items[i].LineIndex >= e.Index)
                {
                    items[i].LineIndex = items[i].LineIndex + e.Count;
                }
                else
                if (items[i].LineIndex == e.Index - 1 && e.Count == 1)
                {
                    if (tb[e.Index - 1].StartSpacesCount == tb[e.Index - 1].Count)
                        items[i].LineIndex = items[i].LineIndex + e.Count;
                }
        }

        public override void Dispose()
        {
            tb.LineInserted -= Tb_LineInserted;
            tb.LineRemoved -= Tb_LineRemoved;
        }

        public override IEnumerator<Bookmark> GetEnumerator() => items.GetEnumerator();

        public override void Add(int lineIndex, string bookmarkName)
        {
            Add(new Bookmark(tb, bookmarkName ?? "Bookmark " + counter, lineIndex));
        }

        public override void Add(int lineIndex)
        {
            Add(new Bookmark(tb, "Bookmark " + counter, lineIndex));
        }

        public override void Clear()
        {
            items.Clear();
            counter = 0;
        }

        public override void Add(Bookmark item)
        {
            foreach (var bm in items)
                if (bm.LineIndex == item.LineIndex)
                    return;

            items.Add(item);
            counter++;
            tb.Invalidate();
        }

        public override bool Contains(Bookmark item) => items.Contains(item);

        public override bool Contains(int lineIndex)
        {
            foreach (var item in items)
                if (item.LineIndex == lineIndex)
                    return true;
            return false;
        }

        public override void CopyTo(Bookmark[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

        public override int Count => items.Count;

        public override bool IsReadOnly => false;

        public override bool Remove(Bookmark item)
        {
            tb.Invalidate();
            return items.Remove(item);
        }

        /// <summary>
        /// Removes bookmark by line index
        /// </summary>
        public override bool Remove(int lineIndex)
        {
            bool was = false;
            for (int i = 0; i < Count; i++)
                if (items[i].LineIndex == lineIndex)
                {
                    items.RemoveAt(i);
                    i--;
                    was = true;
                }
            tb.Invalidate();

            return was;
        }

        /// <summary>
        /// Returns Bookmark by index.
        /// </summary>
        public override Bookmark GetBookmark(int i)
        {
            return items[i];
        }
    }
}
