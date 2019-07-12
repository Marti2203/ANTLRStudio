using System.Text.RegularExpressions;
using ANTLRStudio.Editor.Styles;

namespace ANTLRStudio.Editor.Syntax
{
    public class RuleDesc
    {
        Regex regex;
        public string pattern;
        public RegexOptions options = RegexOptions.None;
        public Style style;

        public Regex Regex
        {
            get
            {
                if (regex == null)
                {
                    regex = new Regex(pattern, SyntaxHighlighter.RegexCompiledOption | options);
                }
                return regex;
            }
        }
    }
}
