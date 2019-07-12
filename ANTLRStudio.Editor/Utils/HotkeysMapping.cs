using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Eto.Forms;
using static Eto.Forms.Keys;
using KEYS = Eto.Forms.Keys;

namespace ANTLRStudio.Editor.Utils
{
    /// <summary>
    /// Dictionary of shortcuts for FCTB
    /// </summary>
    public class HotkeysMapping : SortedDictionary<KEYS, FCTBAction>
    {
        public virtual void InitDefault()
        {
#pragma warning disable RECS0030 // Suggests using the class declaring a static function when calling it
            this[KEYS.Control | G] = FCTBAction.GoToDialog;
            this[KEYS.Control | F] = FCTBAction.FindDialog;
            this[Alt | F] = FCTBAction.FindChar;
            this[F3] = FCTBAction.FindNext;
            this[KEYS.Control | H] = FCTBAction.ReplaceDialog;
            this[KEYS.Control | C] = FCTBAction.Copy;
            this[KEYS.Control | Shift | C] = FCTBAction.CommentSelected;
            this[KEYS.Control | X] = FCTBAction.Cut;
            this[KEYS.Control | V] = FCTBAction.Paste;
            this[KEYS.Control | A] = FCTBAction.SelectAll;
            this[KEYS.Control | Z] = FCTBAction.Undo;
            this[KEYS.Control | R] = FCTBAction.Redo;
            this[KEYS.Control | U] = FCTBAction.UpperCase;
            this[Shift | KEYS.Control | U] = FCTBAction.LowerCase;
            this[KEYS.Control | Minus] = FCTBAction.NavigateBackward;
            this[KEYS.Control | Shift | Minus] = FCTBAction.NavigateForward;
            this[KEYS.Control | B] = FCTBAction.BookmarkLine;
            this[KEYS.Control | Shift | B] = FCTBAction.UnbookmarkLine;
            this[KEYS.Control | N] = FCTBAction.GoNextBookmark;
            this[KEYS.Control | Shift | N] = FCTBAction.GoPrevBookmark;
            this[Alt | Backspace] = FCTBAction.Undo;
            this[KEYS.Control | Backspace] = FCTBAction.ClearWordLeft;
            this[Insert] = FCTBAction.ReplaceMode;
            this[KEYS.Control | Insert] = FCTBAction.Copy;
            this[Shift | Insert] = FCTBAction.Paste;
            this[Delete] = FCTBAction.DeleteCharRight;
            this[KEYS.Control | Delete] = FCTBAction.ClearWordRight;
            this[Shift | Delete] = FCTBAction.Cut;
            this[Left] = FCTBAction.GoLeft;
            this[Shift | Left] = FCTBAction.GoLeftWithSelection;
            this[KEYS.Control | Left] = FCTBAction.GoWordLeft;
            this[KEYS.Control | Shift | Left] = FCTBAction.GoWordLeftWithSelection;
            this[Alt | Shift | Left] = FCTBAction.GoLeft_ColumnSelectionMode;
            this[Right] = FCTBAction.GoRight;
            this[Shift | Right] = FCTBAction.GoRightWithSelection;
            this[KEYS.Control | Right] = FCTBAction.GoWordRight;
            this[KEYS.Control | Shift | Right] = FCTBAction.GoWordRightWithSelection;
            this[Alt | Shift | Right] = FCTBAction.GoRight_ColumnSelectionMode;
            this[Up] = FCTBAction.GoUp;
            this[Shift | Up] = FCTBAction.GoUpWithSelection;
            this[Alt | Shift | Up] = FCTBAction.GoUp_ColumnSelectionMode;
            this[Alt | Up] = FCTBAction.MoveSelectedLinesUp;
            this[KEYS.Control | Up] = FCTBAction.ScrollUp;
            this[Down] = FCTBAction.GoDown;
            this[Shift | Down] = FCTBAction.GoDownWithSelection;
            this[Alt | Shift | Down] = FCTBAction.GoDown_ColumnSelectionMode;
            this[Alt | Down] = FCTBAction.MoveSelectedLinesDown;
            this[KEYS.Control | Down] = FCTBAction.ScrollDown;
            this[PageUp] = FCTBAction.GoPageUp;
            this[Shift | PageUp] = FCTBAction.GoPageUpWithSelection;
            this[PageDown] = FCTBAction.GoPageDown;
            this[Shift | PageDown] = FCTBAction.GoPageDownWithSelection;
            this[Home] = FCTBAction.GoHome;
            this[Shift | Home] = FCTBAction.GoHomeWithSelection;
            this[KEYS.Control | Home] = FCTBAction.GoFirstLine;
            this[KEYS.Control | Shift | Home] = FCTBAction.GoFirstLineWithSelection;
            this[End] = FCTBAction.GoEnd;
            this[Shift | End] = FCTBAction.GoEndWithSelection;
            this[KEYS.Control | End] = FCTBAction.GoLastLine;
            this[KEYS.Control | Shift | End] = FCTBAction.GoLastLineWithSelection;
            this[Escape] = FCTBAction.ClearHints;
            this[KEYS.Control | M] = FCTBAction.MacroRecord;
            this[KEYS.Control | E] = FCTBAction.MacroExecute;
            this[KEYS.Control | Space] = FCTBAction.AutocompleteMenu;
            this[Tab] = FCTBAction.IndentIncrease;
            this[Shift | Tab] = FCTBAction.IndentDecrease;
            this[KEYS.Control | Subtract] = FCTBAction.ZoomOut;
            this[KEYS.Control | KEYS.Add] = FCTBAction.ZoomIn;
            this[KEYS.Control | D0] = FCTBAction.ZoomNormal;
            this[KEYS.Control | I] = FCTBAction.AutoIndentChars;
#pragma warning restore RECS0030 // Suggests using the class declaring a static function when calling it
        }

        public override string ToString()
        {
            var cult = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            StringBuilder sb = new StringBuilder();
            var kc = new KeysConverter();
            foreach (var pair in this)
            {
                sb.AppendFormat("{0}={1}, ", kc.ConvertToString(pair.Key), pair.Value);
            }

            if (sb.Length > 1)
                sb.Remove(sb.Length - 2, 2);
            Thread.CurrentThread.CurrentUICulture = cult;

            return sb.ToString();
        }

        public static HotkeysMapping Parse(string s)
        {
            var result = new HotkeysMapping();
            result.Clear();
            var cult = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var kc = new KeysConverter();

            foreach (var p in s.Split(','))
            {
                var pp = p.Split('=');
                var k = (Keys)kc.ConvertFromString(pp[0].Trim());
                var a = (FCTBAction)Enum.Parse(typeof(FCTBAction), pp[1].Trim());
                result[k] = a;
            }

            Thread.CurrentThread.CurrentUICulture = cult;

            return result;
        }
    }

    /// <summary>
    /// Actions for shortcuts
    /// </summary>
    public enum FCTBAction
    {
        None,
        AutocompleteMenu,
        AutoIndentChars,
        BookmarkLine,
        ClearHints,
        ClearWordLeft,
        ClearWordRight,
        CommentSelected,
        Copy,
        Cut,
        DeleteCharRight,
        FindChar,
        FindDialog,
        FindNext,
        GoDown,
        GoDownWithSelection,
        GoDown_ColumnSelectionMode,
        GoEnd,
        GoEndWithSelection,
        GoFirstLine,
        GoFirstLineWithSelection,
        GoHome,
        GoHomeWithSelection,
        GoLastLine,
        GoLastLineWithSelection,
        GoLeft,
        GoLeftWithSelection,
        GoLeft_ColumnSelectionMode,
        GoPageDown,
        GoPageDownWithSelection,
        GoPageUp,
        GoPageUpWithSelection,
        GoRight,
        GoRightWithSelection,
        GoRight_ColumnSelectionMode,
        GoToDialog,
        GoNextBookmark,
        GoPrevBookmark,
        GoUp,
        GoUpWithSelection,
        GoUp_ColumnSelectionMode,
        GoWordLeft,
        GoWordLeftWithSelection,
        GoWordRight,
        GoWordRightWithSelection,
        IndentIncrease,
        IndentDecrease,
        LowerCase,
        MacroExecute,
        MacroRecord,
        MoveSelectedLinesDown,
        MoveSelectedLinesUp,
        NavigateBackward,
        NavigateForward,
        Paste,
        Redo,
        ReplaceDialog,
        ReplaceMode,
        ScrollDown,
        ScrollUp,
        SelectAll,
        UnbookmarkLine,
        Undo,
        UpperCase,
        ZoomIn,
        ZoomNormal,
        ZoomOut,
        CustomAction1,
        CustomAction2,
        CustomAction3,
        CustomAction4,
        CustomAction5,
        CustomAction6,
        CustomAction7,
        CustomAction8,
        CustomAction9,
        CustomAction10,
        CustomAction11,
        CustomAction12,
        CustomAction13,
        CustomAction14,
        CustomAction15,
        CustomAction16,
        CustomAction17,
        CustomAction18,
        CustomAction19,
        CustomAction20
    }

}
