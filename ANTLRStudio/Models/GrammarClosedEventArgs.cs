using System;
namespace ANTLRStudio.Models
{
    [System.Serializable]
    public sealed class GrammarClosedEventArgs : EventArgs
    {
        public string GrammarName { get; private set; }
        public string GrammarPath { get; private set; }
        public GrammarClosedEventArgs(string grammarName, string grammarPath)
        {
            GrammarName = grammarName;
            GrammarPath = grammarPath;
        }
    }
}
