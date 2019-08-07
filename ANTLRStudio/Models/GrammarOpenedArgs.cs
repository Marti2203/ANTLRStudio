using System;
namespace ANTLRStudio.Models
{
    [Serializable]
    public sealed class GrammarOpenedEventArgs : EventArgs
    {
        public string GrammarName { get; private set; }
        public string GrammarPath { get; private set; }
        public GrammarOpenedEventArgs(string grammarName, string grammarPath)
        {
            GrammarName = grammarName;
            GrammarPath = grammarPath;
        }
    }
}
