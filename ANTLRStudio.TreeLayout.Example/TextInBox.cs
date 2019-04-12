using System;
namespace ANTLRStudio.TreeLayout.Example
{
    public class TextInBox
    {

        public string Text { get; }
        public int Height { get; }
        public int Width { get; }

        public TextInBox(string text, int width, int height)
        {
            Text = text;
            Width = width;
            Height = height;
        }
    }
}
