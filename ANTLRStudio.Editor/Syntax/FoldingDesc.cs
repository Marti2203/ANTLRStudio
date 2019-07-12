using System.Text.RegularExpressions;

namespace ANTLRStudio.Editor.Syntax
{
    public class FoldingDesc
    {
        public string startMarkerRegex;
        public string finishMarkerRegex;
        public RegexOptions options = RegexOptions.None;
    }
}
