using System;
using System.Linq;
using System.Collections.Generic;

namespace ANTLRStudio.Editor.Text
{
    public class LinesAccessor : IList<string>
    {
        private IList<Line> ts;

        public LinesAccessor(IList<Line> ts)
        {
            this.ts = ts;
        }

        public int IndexOf(string item)
        {
            for (int i = 0; i < ts.Count; i++)
                if (ts[i].Text == item)
                    return i;

            return -1;
        }

        public void Insert(int index, string item) => throw new NotImplementedException();

        public void RemoveAt(int index) => throw new NotImplementedException();

        public string this[int index]
        {
            get => ts[index].Text;
            set => throw new NotImplementedException();
        }

        public void Add(string item) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public bool Contains(string item)
        {
            for (int i = 0; i < ts.Count; i++)
                if (ts[i].Text == item)
                    return true;

            return false;
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            for (int i = 0; i < ts.Count; i++)
                array[i + arrayIndex] = ts[i].Text;
        }

        public int Count => ts.Count;

        public bool IsReadOnly => true;

        public bool Remove(string item) => throw new NotImplementedException();

        public IEnumerator<string> GetEnumerator() => ts.Select(x => x.Text).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
