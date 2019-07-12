using System;
using Eto.Forms;
namespace ANTLRStudio.Editor.Utils
{
    public class KeysConverter
    {
        public Keys ConvertFromString(string value)
        {
            return Keys.A;
        }
        public string ConvertToString(Keys value)
        {
            return "A";
        }
    }
}
