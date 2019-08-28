using System;
namespace ANTLRStudio.Models
{
    [System.Serializable]
    public sealed class GrammarGenerationLanguageEventArgs : EventArgs
    {
        public string Language { get; private set; }
        public GrammarGenerationLanguageEventArgs(string language)
        {
            Language = language;
        }
    }
}
