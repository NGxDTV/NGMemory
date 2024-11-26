using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static NGMemory.Kernel32;

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

        public static void SetSelectedItem(IntPtr hWnd, string item)
        {
            SendMessage(hWnd, CB_SELECTSTRING, IntPtr.Zero, new StringBuilder(item));
        }

        public static void SetSelectedIndex(IntPtr hWnd, int index)
        {
            SendMessage(hWnd, CB_SETCURSEL, new IntPtr(index), IntPtr.Zero);
        }
    }
}
