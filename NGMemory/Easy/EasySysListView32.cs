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

        // LVM & Header
        private const int LVM_FIRST = 0x1000;
        private const int LVM_DELETEALLITEMS = LVM_FIRST + 9;    // 0x1009
        private const int LVM_DELETEITEM = LVM_FIRST + 8;    // 0x1008
        private const int LVM_INSERTITEM = LVM_FIRST + 7;    // 0x1007
        private const int LVM_SETITEM = LVM_FIRST + 6;    // 0x1006
        private const int LVM_GETHEADER = LVM_FIRST + 31;   // 0x101F
        private const int LVM_GETITEMCOUNT = LVM_FIRST + 4;    // 0x1004
        private const int LVM_GETITEMTEXTW = LVM_FIRST + 115;  // 0x1073 (Unicode)
        private const int LVM_GETCOLUMN = LVM_FIRST + 95;   // 0x105F
        private const int LVM_ENSUREVISIBLE = LVM_FIRST + 19;   // 0x1013
        private const int LVM_GETTOPINDEX = LVM_FIRST + 39;   // 0x1027
        private const int LVM_GETCOUNTPERPAGE = LVM_FIRST + 40;   // 0x1028

        private const int HDM_FIRST = 0x1200;
        private const int HDM_GETITEMCOUNT = HDM_FIRST + 0;       // 0x1200

        // Scrolling
        private const int WM_VSCROLL = 0x0115;
        private const int SB_LINEUP = 0;
        private const int SB_LINEDOWN = 1;
        private const int SB_PAGEUP = 2;
        private const int SB_PAGEDOWN = 3;
        private const int SB_TOP = 6;
        private const int SB_BOTTOM = 7;

        private const uint LVIF_TEXT = 0x0001;
        private const int LVCF_TEXT = 0x0004;

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
            NGMemory.User32.GetWindowThreadProcessId(hWnd, out int pid);
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

        public static int GetColumnCount(IntPtr listViewHandle)
        {
            var header = NGMemory.User32.SendMessage(listViewHandle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            if (header == IntPtr.Zero) return 0;
            return NGMemory.User32.SendMessage(header, HDM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        public static List<ListViewItem> GetItems(IntPtr listViewHandle, int columnCount)
        {
            return WinInteropTools.SysListView32.GetListViewItems(listViewHandle, columnCount);
        }

        public static List<ListViewItem> GetItems(IntPtr listViewHandle, bool autoColumnCount)
        {
            int count = autoColumnCount ? GetColumnCount(listViewHandle) : 1;
            return GetItems(listViewHandle, count);
        }

        public static string ReadItemText(IntPtr listViewHandle, int itemIndex, int subIndex)
        {
            return WinInteropTools.SysListView32.ReadListViewItem(listViewHandle, itemIndex, subIndex);
        }

        public static int GetItemCount(IntPtr listViewHandle)
        {
            return NGMemory.User32.SendMessage(listViewHandle, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        public static List<string[]> GetAllRowsAsStrings(IntPtr listViewHandle, int columnCount)
        {
            return GetAllRowsViaLvm(listViewHandle, columnCount, 2048);
        }

        public static List<string[]> GetAllRowsAsStrings(IntPtr listViewHandle, bool autoColumnCount)
        {
            int c = autoColumnCount ? GetColumnCount(listViewHandle) : 1;
            return GetAllRowsViaLvm(listViewHandle, c, 2048);
        }

        public static void InsertItem(IntPtr listViewHandle, int index, string text)
        {
            var lvItem = new LV_ITEM
            {
                iItem = index,
                mask = LVIF_TEXT,
                pszText = Marshal.StringToHGlobalUni(text)
            };

            IntPtr pLvItem = Marshal.AllocHGlobal(Marshal.SizeOf(lvItem));
            Marshal.StructureToPtr(lvItem, pLvItem, false);
            NGMemory.User32.SendMessage(listViewHandle, LVM_INSERTITEM, IntPtr.Zero, pLvItem);
            Marshal.FreeHGlobal(lvItem.pszText);
            Marshal.FreeHGlobal(pLvItem);
        }

        public static void RemoveItem(IntPtr listViewHandle, int index)
        {
            NGMemory.User32.SendMessage(listViewHandle, LVM_DELETEITEM, (IntPtr)index, IntPtr.Zero);
        }

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
                if (data[i].Length > 0 && data[i][0].Contains(ContainsName))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public static void SetItemText(IntPtr listViewHandle, int itemIndex, string newText)
        {
            var lvItem = new LV_ITEM
            {
                iSubItem = 0,
                iItem = itemIndex,
                mask = LVIF_TEXT,
                pszText = Marshal.StringToHGlobalUni(newText)
            };

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
                PostMessage(listViewHandle, 0x0100, 0x28, 0); // VK_DOWN
                Thread.Sleep(100);
            }
            PostMessage(listViewHandle, 0x0100, 0x0D, 0); // VK_RETURN
        }

        public static string CopyAllRowsTSV(IntPtr hWnd, bool withHeader = false)
        {
            HydrateAllItems(hWnd);
            int cols = GetColumnCount(hWnd);
            var sb = new StringBuilder();

            if (withHeader)
                sb.AppendLine(string.Join("\t", GetColumnHeadersSafe(hWnd, cols)));

            foreach (var row in GetAllRowsViaLvm(hWnd, cols, 2048))
                sb.AppendLine(string.Join("\t", row));

            Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
            return sb.ToString();
        }

        public static void SelectItemByName(IntPtr listViewHandle, string ContainsName)
        {
            int index = GetListViewItemByName(listViewHandle, ContainsName);
            SelectItemByIndex(listViewHandle, index);
        }

        private static int GetTopIndex(IntPtr hWnd) =>
            NGMemory.User32.SendMessage(hWnd, LVM_GETTOPINDEX, IntPtr.Zero, IntPtr.Zero).ToInt32();

        private static int GetCountPerPage(IntPtr hWnd) =>
            NGMemory.User32.SendMessage(hWnd, LVM_GETCOUNTPERPAGE, IntPtr.Zero, IntPtr.Zero).ToInt32();

        /// <summary>
        /// Scrollt die ListView seitenweise nach unten, bis keine neuen Items/Positionen mehr geladen werden.
        /// Anschließend zurück zum Anfang.
        /// </summary>
        private static void HydrateAllItems(IntPtr hWnd, int stablePasses = 5, int maxPasses = 2000)
        {
            int stable = 0;
            for (int i = 0; i < maxPasses && stable < stablePasses; i++)
            {
                int beforeCount = GetItemCount(hWnd);
                int beforeTop = GetTopIndex(hWnd);

                NGMemory.User32.SendMessage(hWnd, WM_VSCROLL, (IntPtr)SB_PAGEDOWN, IntPtr.Zero);
                int cpp = GetCountPerPage(hWnd); if (cpp < 1) cpp = 50;
                NGMemory.User32.SendMessage(hWnd, LVM_ENSUREVISIBLE, (IntPtr)(beforeTop + cpp - 1), IntPtr.Zero);

                Thread.Sleep(15);

                int afterCount = GetItemCount(hWnd);
                int afterTop = GetTopIndex(hWnd);

                if (afterCount == beforeCount && afterTop == beforeTop) stable++;
                else stable = 0;
            }

            NGMemory.User32.SendMessage(hWnd, WM_VSCROLL, (IntPtr)SB_TOP, IntPtr.Zero);
            Thread.Sleep(10);
        }

        private static List<string[]> GetAllRowsViaLvm(IntPtr hWnd, int columnCount, int maxCharsPerCell)
        {
            HydrateAllItems(hWnd);

            var rows = new List<string[]>();

            NGMemory.User32.GetWindowThreadProcessId(hWnd, out int pid);
            IntPtr hProc = Kernel32.OpenProcess(Constants.PROCESS_ALL_ACCESS, false, pid);

            int lvSize = Marshal.SizeOf<LV_ITEM>();
            int bufBytes = maxCharsPerCell * 2; // UTF-16
            IntPtr remote = Kernel32.VirtualAllocEx(hProc, IntPtr.Zero, lvSize + bufBytes, Constants.MEM_COMMIT, Constants.PAGE_READWRITE);
            IntPtr localLv = Marshal.AllocHGlobal(lvSize);
            IntPtr localBuf = Marshal.AllocHGlobal(bufBytes);

            try
            {
                int itemCount = NGMemory.User32.SendMessage(hWnd, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

                for (int i = 0; i < itemCount; i++)
                {
                    var cells = new string[columnCount];

                    for (int c = 0; c < columnCount; c++)
                    {
                        var lvi = new LV_ITEM
                        {
                            mask = LVIF_TEXT,
                            iItem = i,
                            iSubItem = c,
                            cchTextMax = maxCharsPerCell,
                            pszText = remote + lvSize // Textpuffer direkt hinter LV_ITEM
                        };

                        Marshal.StructureToPtr(lvi, localLv, false);
                        WriteProcessMemory(hProc, remote, localLv, lvSize, out _);

                        NGMemory.User32.SendMessage(hWnd, LVM_GETITEMTEXTW, (IntPtr)i, remote);

                        ReadProcessMemory(hProc, remote + lvSize, localBuf, bufBytes, out _);
                        cells[c] = Marshal.PtrToStringUni(localBuf) ?? string.Empty;
                    }

                    rows.Add(cells);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(localLv);
                Marshal.FreeHGlobal(localBuf);
                Kernel32.VirtualFreeEx(hProc, remote, 0, Constants.MEM_RELEASE);
                Kernel32.CloseHandle(hProc);
            }

            return rows;
        }

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
