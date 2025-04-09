using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NGMemory.Easy
{
    /// <summary>
    /// Macht das Batch-Auslesen und -Setzen verschiedener Steuerelemente einfacher.
    /// </summary>
    public static class EasyFormHelper
    {
        /// <summary>
        /// Setzt mehrere TextBox-Werte anhand eines Dictionary: {ControlID -> Text}.
        /// </summary>
        public static void SetTextFields(IntPtr window, Dictionary<int, string> textMap)
        {
            foreach (var pair in textMap)
            {
                EasyTextBox.SetText(window, pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Liest mehrere TextBox-Werte aus und gibt sie als Dictionary zurück.
        /// </summary>
        public static Dictionary<int, string> GetTextFields(IntPtr window, params int[] controlIds)
        {
            var result = new Dictionary<int, string>();
            foreach (var cid in controlIds)
            {
                result[cid] = EasyTextBox.GetText(window, cid);
            }
            return result;
        }

        /// <summary>
        /// Setzt mehrere CheckBoxen anhand eines Dictionary: {ControlID -> bool}.
        /// </summary>
        public static void SetCheckBoxes(IntPtr window, Dictionary<int, bool> checkMap)
        {
            foreach (var pair in checkMap)
            {
                EasyCheckBox.SetChecked(window, pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Liest mehrere CheckBox-Zustände und gibt sie als Dictionary zurück.
        /// </summary>
        public static Dictionary<int, bool> GetCheckBoxes(IntPtr window, params int[] controlIds)
        {
            var result = new Dictionary<int, bool>();
            foreach (var cid in controlIds)
            {
                result[cid] = EasyCheckBox.IsChecked(window, cid);
            }
            return result;
        }

        /// <summary>
        /// Setzt mehrere ComboBoxen anhand eines Dictionary: {ControlID -> ItemText}.
        /// </summary>
        public static void SetComboBoxes(IntPtr window, Dictionary<int, string> comboMap)
        {
            foreach (var pair in comboMap)
            {
                IntPtr comboHandle = EasyGuiInterop.GetControlHandle(window, pair.Key);
                EasyComboBox.SelectItemByString(comboHandle, pair.Value);
            }
        }

        /// <summary>
        /// Liest mehrere ComboBoxen und gibt die ausgewählten Einträge als Dictionary zurück.
        /// </summary>
        public static Dictionary<int, string> GetComboBoxes(IntPtr window, params int[] controlIds)
        {
            var result = new Dictionary<int, string>();
            foreach (var cid in controlIds)
            {
                IntPtr comboHandle = EasyGuiInterop.GetControlHandle(window, cid);
                result[cid] = EasyComboBox.GetSelectedItem(comboHandle) ?? "";
            }
            return result;
        }
    }
}
