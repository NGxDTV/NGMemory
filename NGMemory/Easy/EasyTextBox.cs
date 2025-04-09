using System;

namespace NGMemory.Easy
{
    /// <summary>
    /// Schnelle Hilfsfunktionen für TextBox-Steuerelemente.
    /// </summary>
    public static class EasyTextBox
    {
        /// <summary>
        /// Holt den Text aus dem angegebenen TextBox-Steuerelement.
        /// </summary>
        public static string GetText(IntPtr window, int controlId)
        {
            return WinInteropTools.TextBox.getTextBoxValue(window, controlId);
        }

        /// <summary>
        /// Setzt den Text in das angegebene TextBox-Steuerelement.
        /// </summary>
        public static void SetText(IntPtr window, int controlId, string text)
        {
            WinInteropTools.GuiInteropHandler.InteropSetText(window, controlId, text);
        }

        /// <summary>
        /// Löscht den Text (setzt ihn auf leer).
        /// </summary>
        public static void ClearText(IntPtr window, int controlId)
        {
            SetText(window, controlId, string.Empty);
        }
    }
}
