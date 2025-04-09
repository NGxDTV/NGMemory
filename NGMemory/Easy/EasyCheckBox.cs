using System;

namespace NGMemory.Easy
{
    /// <summary>
    /// Schnelle Hilfsfunktionen für CheckBox-Steuerelemente.
    /// </summary>
    public static class EasyCheckBox
    {
        /// <summary>
        /// Prüft, ob die angegebene CheckBox aktiviert (true) oder deaktiviert (false) ist.
        /// </summary>
        public static bool IsChecked(IntPtr window, int controlId)
        {
            return WinInteropTools.CheckBox.IsCheckBoxChecked(window, controlId);
        }

        /// <summary>
        /// Setzt den Zustand der CheckBox: true = aktiviert, false = deaktiviert.
        /// </summary>
        public static void SetChecked(IntPtr window, int controlId, bool state)
        {
            WinInteropTools.CheckBox.SetCheckBoxState(window, controlId, state);
        }

        /// <summary>
        /// Wechselt den aktuellen Zustand der CheckBox (an/aus).
        /// </summary>
        public static void ToggleState(IntPtr window, int controlId)
        {
            bool current = IsChecked(window, controlId);
            SetChecked(window, controlId, !current);
        }

        /// <summary>
        /// Simuliert einen Klick auf die CheckBox.
        /// </summary>
        public static void ClickCheckBox(IntPtr window, int controlId)
        {
            var handle = WinInteropTools.GuiInteropHandler.getRef(window, controlId).Handle;
            NGMemory.User32.SendMessage(handle, 0x00F5, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
