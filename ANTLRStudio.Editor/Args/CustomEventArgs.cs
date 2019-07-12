using System;
using ANTLRStudio.Editor.Utils;

namespace ANTLRStudio.Editor.Args
{
    /// <summary>
    /// CustomAction event args
    /// </summary>
    public class CustomActionEventArgs : EventArgs
    {
        public FCTBAction Action { get; private set; }

        public CustomActionEventArgs(FCTBAction action)
        {
            Action = action;
        }
    }
}
