using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static NGMemory.Kernel32;

using static NGMemory.Structures;
using static NGMemory.User32;
using static NGMemory.Constants;
using static NGMemory.Enums;

namespace NGMemory.WinInteropTools
{
    public class ComboBox
    {
        public static string GetSelectedItem(IntPtr hWnd)
        {
            int index = SendMessage(hWnd, CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt32();
            if (index < 0) return null;

            StringBuilder buffer = new StringBuilder(256);
            SendMessage(hWnd, CB_GETLBTEXT, new IntPtr(index), buffer);
            return buffer.ToString();
        }

        public static string[] GetItems(IntPtr hWnd)
        {
            int count = SendMessage(hWnd, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
            if (count <= 0) return Array.Empty<string>();

            string[] items = new string[count];
            StringBuilder buffer = new StringBuilder(256);

            for (int i = 0; i < count; i++)
            {
                SendMessage(hWnd, CB_GETLBTEXT, new IntPtr(i), buffer);
                items[i] = buffer.ToString();
            }

            return items;
        }

        public static void SetSelectedItem(IntPtr hWndComboBox, string item)
        {
            const uint CB_SELECTSTRING = 0x014D;
            const int WM_COMMAND = 0x0111;
            const int CBN_SELCHANGE = 1;

            SendMessage(hWndComboBox, CB_SELECTSTRING, new IntPtr(-1), item);
            IntPtr hWndParent = GetParent(hWndComboBox);
            int controlId = GetDlgCtrlID(hWndComboBox);
            SendMessage(hWndParent, WM_COMMAND, MAKELPARAM(controlId, CBN_SELCHANGE), hWndComboBox);
        }

        public static void SetSelectedIndex(IntPtr hWndComboBox, int index)
        {
            const int CBN_SELCHANGE = 1;

            SendMessage(hWndComboBox, CB_SETCURSEL, new IntPtr(index), IntPtr.Zero);
            IntPtr hWndParent = GetParent(hWndComboBox);
            int controlId = GetDlgCtrlID(hWndComboBox);
            SendMessage(hWndParent, WM_COMMAND, MAKELPARAM(controlId, CBN_SELCHANGE), hWndComboBox);
        }

        public static IntPtr MAKELPARAM(int low, int high)
        {
            return (IntPtr)((high << 16) | (low & 0xFFFF));
        }

    }
}
