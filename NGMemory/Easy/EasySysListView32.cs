using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NGMemory.Easy
{
    /// <summary>
    /// Erweiterte Hilfsfunktionen für SysListView32.
    /// </summary>
    public static class EasySysListView32
    {
        // Native Konstante
        private const int LVM_DELETEALLITEMS = 0x1009;
        private const int LVM_DELETEITEM = 0x1008;
        private const int LVM_INSERTITEM = 0x1007;
        private const int LVM_SETITEM = 0x1006;
        private const int LVM_GETHEADER = 0x101F;
        private const int HDM_GETITEMCOUNT = 0x1200;

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
