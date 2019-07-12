using System;

namespace ANTLRStudio.Editor.Utils
{
    /// <summary>
    /// Limited stack
    /// </summary>
    public class LimitedStack<T>
    {
        T[] items;
        int start;

        /// <summary>
        /// Max stack length
        /// </summary>
        public int MaxItemCount => items.Length;

        /// <summary>
        /// Current length of stack
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxItemCount">Maximum length of stack</param>
        public LimitedStack(int maxItemCount)
        {
            items = new T[maxItemCount];
            Count = 0;
            start = 0;
        }

        /// <summary>
        /// Pop item
        /// </summary>
        public T Pop()
        {
            if (Count == 0)
                throw new Exception("Stack is empty");

            int i = LastIndex;
            T item = items[i];
            items[i] = default(T);

            Count--;

            return item;
        }

        int LastIndex => (start + Count - 1) % items.Length;

        /// <summary>
        /// Peek item
        /// </summary>
        public T Peek() => Count == 0 ? default(T) : items[LastIndex];

        /// <summary>
        /// Push item
        /// </summary>
        public void Push(T item)
        {
            if (Count == items.Length)
                start = (start + 1) % items.Length;
            else
                Count++;

            items[LastIndex] = item;
        }

        /// <summary>
        /// Clear stack
        /// </summary>
        public void Clear()
        {
            items = new T[items.Length];
            Count = 0;
            start = 0;
        }

        public T[] ToArray()
        {
            T[] result = new T[Count];
            for (int i = 0; i < Count; i++)
                result[i] = items[(start + i) % items.Length];
            return result;
        }
    }
}