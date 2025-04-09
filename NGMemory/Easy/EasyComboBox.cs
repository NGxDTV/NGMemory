using System;

namespace NGMemory.Easy
{
    /// <summary>
    /// Schnelle Hilfsfunktionen für ComboBox-Steuerelemente.
    /// </summary>
    public static class EasyComboBox
    {
        /// <summary>
        /// Gibt den aktuell ausgewählten Eintrag als Text zurück. Wenn nichts ausgewählt, null.
        /// </summary>
        public static string GetSelectedItem(IntPtr comboBoxHandle)
        {
            return WinInteropTools.ComboBox.GetSelectedItem(comboBoxHandle);
        }

        /// <summary>
        /// Liest alle Einträge aus der ComboBox und liefert diese als String-Array.
        /// </summary>
        public static string[] GetItems(IntPtr comboBoxHandle)
        {
            return WinInteropTools.ComboBox.GetItems(comboBoxHandle);
        }

        /// <summary>
        /// Wählt den angegebenen Text-Eintrag in der ComboBox aus.
        /// </summary>
        public static void SelectItemByString(IntPtr comboBoxHandle, string itemText)
        {
            WinInteropTools.ComboBox.SetSelectedItem(comboBoxHandle, itemText);
        }

        /// <summary>
        /// Wählt den Eintrag an der angegebenen Position aus.
        /// </summary>
        public static void SelectItemByIndex(IntPtr comboBoxHandle, int index)
        {
            WinInteropTools.ComboBox.SetSelectedIndex(comboBoxHandle, index);
        }
    }
}
