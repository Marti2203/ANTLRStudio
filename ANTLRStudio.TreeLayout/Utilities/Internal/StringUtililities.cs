using System;
using System.Text;

namespace ANTLRStudio.TreeLayout.Utilities.Internal
{
    internal static class StringUtililities
    {
        /**
         * Returns a quoted version of a given string, i.e. as a Java String
         * Literal.
         * 
         * @param s
         *            [nullable] the string to quote
         * @param nullResult
         *            [default="null"] the String to be returned for null values.
         * @return the nullResult when s is null, otherwise s as a quoted string
         *         (i.e. Java String Literal)
         * 
         */
        public static string Quote(string s, string nullResult = "null")
        {
            if (s == null)
            {
                return nullResult;
            }
            StringBuilder result = new StringBuilder();

            result.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '\b':
                        {
                            result.Append("\\b");
                            break;
                        }
                    case '\f':
                        {
                            result.Append("\\f");
                            break;
                        }
                    case '\n':
                        {
                            result.Append("\\n");
                            break;
                        }
                    case '\r':
                        {
                            result.Append("\\r");
                            break;
                        }
                    case '\t':
                        {
                            result.Append("\\t");
                            break;
                        }
                    case '\\':
                        {
                            result.Append("\\\\");
                            break;
                        }
                    case '"':
                        {
                            result.Append("\\\"");
                            break;
                        }
                    default:
                        {
                            if (c < ' ' || c >= '\u0080')
                            {
                                string n = ((int)c).ToString("X");
                                result.Append("\\u");
                                result.Append("0000".Substring(n.Length));
                                result.Append(n);
                            }
                            else
                            {
                                result.Append(c);
                            }
                            break;
                        }
                }
            }
            return result.Append('"').ToString();
        }
    }
}