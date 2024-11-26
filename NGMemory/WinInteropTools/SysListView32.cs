using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static NGMemory.Kernel32;
using System.Windows.Forms;

namespace NGMemory.WinInteropTools
{
    public class SysListView32
    {
        public static List<ListViewItem> GetListViewItems(IntPtr hWnd, int columnCount)
        {
            List<ListViewItem> items = new List<ListViewItem>();
            int itemCount = SendMessage(hWnd, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

            for (int i = 0; i < itemCount; i++)
            {
                ListViewItem listItem = new ListViewItem();

                for (int subitem = 0; subitem < columnCount; subitem++)
                {
                    string text = ReadListViewItem(hWnd, i, subitem);
                    if (subitem == 0)
                    {
                        listItem.Text = text;
                    }
                    else
                    {
                        listItem.SubItems.Add(text);
                    }
                }

                items.Add(listItem);
            }

            return items;
        }

        public static string ReadListViewItem(IntPtr hWnd, int item, int subitem)
        {
            const int dwBufferSize = 1024;

            int dwProcessID;
            LV_ITEM lvItem;
            string retval;
            IntPtr hProcess = IntPtr.Zero;
            IntPtr lpRemoteBuffer = IntPtr.Zero;
            IntPtr lpLocalBuffer = IntPtr.Zero;

            try
            {
                lvItem = new LV_ITEM();
                lpLocalBuffer = Marshal.AllocHGlobal(dwBufferSize);
                GetWindowThreadProcessId(hWnd, out dwProcessID);

                hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, dwProcessID);

                lpRemoteBuffer = VirtualAllocEx(hProcess, IntPtr.Zero, dwBufferSize, MEM_COMMIT, PAGE_READWRITE);

                lvItem.mask = LVIF_TEXT;
                lvItem.iItem = item;
                lvItem.iSubItem = subitem;
                lvItem.pszText = (IntPtr)(lpRemoteBuffer.ToInt64() + Marshal.SizeOf(typeof(LV_ITEM)));
                lvItem.cchTextMax = dwBufferSize;

                WriteProcessMemory(hProcess, lpRemoteBuffer, ref lvItem, Marshal.SizeOf(typeof(LV_ITEM)), IntPtr.Zero);
                SendMessage(hWnd, LVM_GETITEM, 0, lpRemoteBuffer);
                ReadProcessMemory(hProcess, lpRemoteBuffer, lpLocalBuffer, dwBufferSize, IntPtr.Zero);

                retval = Marshal.PtrToStringAnsi((IntPtr)(lpLocalBuffer.ToInt64() + Marshal.SizeOf(typeof(LV_ITEM))));
            }
            finally
            {
                if (lpLocalBuffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpLocalBuffer);
                if (lpRemoteBuffer != IntPtr.Zero)
                    VirtualFreeEx(hProcess, lpRemoteBuffer, 0, MEM_RELEASE);
                if (hProcess != IntPtr.Zero)
                    CloseHandle(hProcess);
            }

            return retval;
        }
    }
}
