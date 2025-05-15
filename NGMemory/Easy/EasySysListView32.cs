using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NGMemory.Easy
{
    /// <summary>
    /// Erweiterte Hilfsfunktionen für SysListView32.
    /// </summary>
    public static class EasySysListView32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesRead);

        // Native Konstante
        private const int LVM_DELETEALLITEMS = 0x1009;
        private const int LVM_DELETEITEM = 0x1008;
        private const int LVM_INSERTITEM = 0x1007;
        private const int LVM_SETITEM = 0x1006;
        private const int LVM_GETHEADER = 0x101F;
        private const int HDM_GETITEMCOUNT = 0x1200;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct HDITEM
        {
            public uint mask;
            public int cxy;
            public IntPtr pszText;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            public int iImage;
            public int iOrder;
        }

        private const int LVM_GETCOLUMN = 0x105F;
        private const int LVCF_TEXT = 0x0004;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LVCOLUMN
        {
            public uint mask; public int fmt; public int cx;
            public IntPtr pszText; public int cchTextMax; public int iSubItem;
            public int iImage; public int iOrder; public int cxMin;
            public int cxDefault; public int cxIdeal;
        }

        private static string[] GetColumnHeadersSafe(IntPtr hWnd, int count)
        {
            const int BUF = 256;
            User32.GetWindowThreadProcessId(hWnd, out int pid);
            IntPtr hProc = Kernel32.OpenProcess(Constants.PROCESS_ALL_ACCESS, false, pid);

            int size = Marshal.SizeOf<LVCOLUMN>();
            IntPtr remote = Kernel32.VirtualAllocEx(hProc, IntPtr.Zero, (int)(size + BUF * 2), Constants.MEM_COMMIT, Constants.PAGE_READWRITE);
            IntPtr local = Marshal.AllocHGlobal(size + BUF * 2);
            var res = new string[count];

            for (int i = 0; i < count; i++)
            {
                var col = new LVCOLUMN { mask = LVCF_TEXT, pszText = remote + size, cchTextMax = BUF };
                Marshal.StructureToPtr(col, local, false);

                IntPtr bytes;
                WriteProcessMemory(hProc, remote, local, size, out bytes);

                NGMemory.User32.SendMessage(hWnd, LVM_GETCOLUMN, (IntPtr)i, remote);
                ReadProcessMemory(hProc, remote + size, local + size, BUF * 2, out bytes);
                res[i] = Marshal.PtrToStringUni(local + size);
            }

            Marshal.FreeHGlobal(local);
            Kernel32.VirtualFreeEx(hProc, remote, 0, Constants.MEM_RELEASE);
            Kernel32.CloseHandle(hProc);
            return res;
        }

        /// <summary>
        /// Liefert die Anzahl der Spalten über das Header-Control.
        /// </summary>
        public static int GetColumnCount(IntPtr listViewHandle)
        {
            var header = NGMemory.User32.SendMessage(listViewHandle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            if (header == IntPtr.Zero) return 0;
            return NGMemory.User32.SendMessage(header, HDM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        /// <summary>
        /// Liest alle Zeilen anhand der Spaltenanzahl.
        /// </summary>
        public static List<ListViewItem> GetItems(IntPtr listViewHandle, int columnCount)
        {
            return WinInteropTools.SysListView32.GetListViewItems(listViewHandle, columnCount);
        }

        /// <summary>
        /// Liest alle Zeilen, ermittelt Spaltenanzahl automatisch (autoColumnCount=true).
        /// </summary>
        public static List<ListViewItem> GetItems(IntPtr listViewHandle, bool autoColumnCount)
        {
            int count = autoColumnCount ? GetColumnCount(listViewHandle) : 1;
            return GetItems(listViewHandle, count);
        }

        /// <summary>
        /// Liest den Text einer Zeile (itemIndex) und Spalte (subIndex).
        /// </summary>
        public static string ReadItemText(IntPtr listViewHandle, int itemIndex, int subIndex)
        {
            return WinInteropTools.SysListView32.ReadListViewItem(listViewHandle, itemIndex, subIndex);
        }

        /// <summary>
        /// Liefert die Anzahl der Zeilen (Items) in der ListView.
        /// </summary>
        public static int GetItemCount(IntPtr listViewHandle)
        {
            // Auslesen mit Spaltenanzahl 1 und einfach zählen.
            return GetItems(listViewHandle, 1).Count;
        }

        /// <summary>
        /// Liest alle Zeilen/Spalten als reine String-Listen.
        /// </summary>
        public static List<string[]> GetAllRowsAsStrings(IntPtr listViewHandle, int columnCount)
        {
            var items = GetItems(listViewHandle, columnCount);
            var result = new List<string[]>();

            foreach (var itm in items)
            {
                var temp = new List<string> { itm.Text };
                for (int i = 1; i < itm.SubItems.Count; i++)
                    temp.Add(itm.SubItems[i].Text);
                result.Add(temp.ToArray());
            }
            return result;
        }

        /// <summary>
        /// Liest alle Zeilen als String-Listen, Spaltenanzahl wird automatisch erkannt.
        /// </summary>
        public static List<string[]> GetAllRowsAsStrings(IntPtr listViewHandle, bool autoColumnCount)
        {
            int c = autoColumnCount ? GetColumnCount(listViewHandle) : 1;
            return GetAllRowsAsStrings(listViewHandle, c);
        }

        /// <summary>
        /// Fügt ein Item in die ListView ein. Nur Text in der ersten Spalte.
        /// </summary>
        public static void InsertItem(IntPtr listViewHandle, int index, string text)
        {
            // Minimaler LV_ITEM für das Einfügen
            var lvItem = new LV_ITEM();
            lvItem.iItem = index;
            lvItem.mask = 0x0001; // LVIF_TEXT
            lvItem.pszText = Marshal.StringToHGlobalUni(text);

            IntPtr pLvItem = Marshal.AllocHGlobal(Marshal.SizeOf(lvItem));
            Marshal.StructureToPtr(lvItem, pLvItem, false);
            NGMemory.User32.SendMessage(listViewHandle, LVM_INSERTITEM, IntPtr.Zero, pLvItem);
            Marshal.FreeHGlobal(lvItem.pszText);
            Marshal.FreeHGlobal(pLvItem);
        }

        /// <summary>
        /// Entfernt ein Item aus der ListView über den Index.
        /// </summary>
        public static void RemoveItem(IntPtr listViewHandle, int index)
        {
            NGMemory.User32.SendMessage(listViewHandle, LVM_DELETEITEM, (IntPtr)index, IntPtr.Zero);
        }

        /// <summary>
        /// Löscht alle Items aus der ListView.
        /// </summary>
        public static void ClearAllItems(IntPtr listViewHandle)
        {
            NGMemory.User32.SendMessage(listViewHandle, LVM_DELETEALLITEMS, IntPtr.Zero, IntPtr.Zero);
        }

        public static IntPtr SearchSysListView32InWindow(IntPtr listViewWindowHandle)
        {
            IntPtr child = IntPtr.Zero;

            for (int i = 0; i < 50; i++)
            {
                child = NGMemory.User32.FindWindowEx(listViewWindowHandle, child, null, null);
                if (child == IntPtr.Zero) break;

                StringBuilder sb = new StringBuilder(256);
                NGMemory.User32.GetClassName(child, sb, sb.Capacity);

                if (sb.ToString() == "SysListView32")
                {
                    return child;
                    break;
                }
            }

            return child;
        }

        public static int GetListViewItemByName(IntPtr listViewHandle, string ContainsName)
        {
            List<string[]> data = NGMemory.Easy.EasySysListView32.GetAllRowsAsStrings(listViewHandle, true);

            int index = 0;

            for (int i = 0; i < data.Count(); i++)
            {
                if (data[i][0].Contains(ContainsName))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Setzt den Text eines bestimmten Items in Spalte 0 (bzw. SubItem=0).
        /// </summary>
        public static void SetItemText(IntPtr listViewHandle, int itemIndex, string newText)
        {
            var lvItem = new LV_ITEM();
            lvItem.iSubItem = 0;
            lvItem.iItem = itemIndex;
            lvItem.mask = 0x0001; // LVIF_TEXT
            lvItem.pszText = Marshal.StringToHGlobalUni(newText);

            IntPtr pLvItem = Marshal.AllocHGlobal(Marshal.SizeOf(lvItem));
            Marshal.StructureToPtr(lvItem, pLvItem, false);
            NGMemory.User32.SendMessage(listViewHandle, LVM_SETITEM, IntPtr.Zero, pLvItem);
            Marshal.FreeHGlobal(lvItem.pszText);
            Marshal.FreeHGlobal(pLvItem);
        }

        public static void SelectItemByIndex(IntPtr listViewHandle, int index)
        {
            for (int i = 0; i < index; i++)
            {
                PostMessage(listViewHandle, 0x0100, 0x28, 0);
                Thread.Sleep(100);
            }
            PostMessage(listViewHandle, 0x0100, 0x0D, 0);
        }

        public static string CopyAllRowsTSV(IntPtr hWnd, bool withHeader = false)
        {
            int cols = GetColumnCount(hWnd);
            var sb = new StringBuilder();

            if (withHeader)
                sb.AppendLine(string.Join("\t", GetColumnHeadersSafe(hWnd, cols)));

            foreach (var row in GetAllRowsAsStrings(hWnd, cols))
                sb.AppendLine(string.Join("\t", row));

            Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
            return sb.ToString();
        }

        public static void SelectItemByName(IntPtr listViewHandle, string ContainsName)
        {
            int index = GetListViewItemByName(listViewHandle, ContainsName);
            SelectItemByIndex(listViewHandle, index);
        }

        // Einfacher LV_ITEM für unsere Zwecke (nur Unicode-Text).
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LV_ITEM
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
        }
    }
}
