using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NGMemory.Kernel32;
using static NGMemory.MessageHelper;

using static NGMemory.Structures;
using static NGMemory.User32;
using static NGMemory.Constants;
using static NGMemory.Enums;

namespace NGMemory.WinInteropTools
{
    public static class GuiInteropHandler
    {
        public static void InteropSetText(IntPtr iptrHWndDialog, int iControlID, string strTextToSet)
        {
            IntPtr iptrHWndControl = GetDlgItem(iptrHWndDialog, iControlID);
            HandleRef hrefHWndTarget = new HandleRef(null, iptrHWndControl);
            SendMessage(hrefHWndTarget, WM_SETTEXT, IntPtr.Zero, new StringBuilder(strTextToSet));
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }

        public static IntPtr GetWindowByClassName(IEnumerable<IntPtr> windows, string className)
        {
            foreach (var window in windows)
            {
                var sb = new StringBuilder(256);
                GetClassName(window, sb, sb.Capacity);

                if (sb.ToString() == className)
                    return window;
            }
            return IntPtr.Zero;
        }

        public static IntPtr GetWindowByClassName(IntPtr window, string className)
        {
            for (int i = 0; i < 100; i++)
            {
                var sb = new StringBuilder(256);
                GetClassName(window, sb, sb.Capacity);

                if (sb.ToString() == className)
                    return window;
            }
            return IntPtr.Zero;
        }

        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(Process process)
        {
            var handles = new List<IntPtr>();
            foreach (ProcessThread thread in process.Threads)
            {
                EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }
            return handles;
        }

        public static HandleRef getRef(IntPtr hWnd, int controlid)
        {
            IntPtr iptrHWndControl = GetDlgItem(hWnd, controlid);
            HandleRef hrefHWndTarget = new HandleRef(null, iptrHWndControl);

            return hrefHWndTarget;
        }

        public static IntPtr getList(IEnumerable<IntPtr> es, string titel)
        {
            foreach (var s in es)
            {
                ListViewItem lvi = new ListViewItem();

                string Titel = GetWindowTitle(s);

                if (Titel.Contains(titel))
                    return s;
            }

            ShowMessage("Make sure the window is open.", "Window not found", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return IntPtr.Zero;
        }

        public class ListItemData
        {
            public IntPtr Id;
            public string Titel;
        }

        public static List<ListItemData> getAllList(IEnumerable<IntPtr> es)
        {
            List<ListItemData> liste = new List<ListItemData>();

            for (int i = 0; i < es.Count(); i++)
            {
                string Titel = GetWindowTitle(es.ToArray()[i]);

                liste.Add(new ListItemData()
                {
                    Id = es.ToArray()[i],
                    Titel = Titel
                });
            }

            return liste;
        }

        public static void showProcessWindow(Process[] prozess)
        {
            ShowWindow(prozess[0].MainWindowHandle, 3);
            SetForegroundWindow(prozess[0].MainWindowHandle);
        }

        public static IntPtr GetHandleByIndex(int index, List<IntPtr> handleList)
        {
            if (index >= 0 && index < handleList.Count)
            {
                return handleList[index];
            }
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        public static List<IntPtr> getChildList(IntPtr from)
        {
            IntPtr result = IntPtr.Zero;
            List<IntPtr> eS = new List<IntPtr>();
            IntPtr EX_PropertySheet = IntPtr.Zero;

            for (int i = 0; i < 100; i++)
            {
                result = FindWindowEx(from, result, null, null);
                string name = GetWindowTitle(result);

                if (name == "EX_PropertySheet")
                    EX_PropertySheet = result;
            }

            IntPtr first = IntPtr.Zero;

            for (int i = 0; i < 100; i++)
            {
                result = FindWindowEx(EX_PropertySheet, result, null, null);
                string name = GetWindowTitle(result);

                if (i == 0)
                    first = result;
                else { if (first == result) break; }

                eS.Add(result);
            }

            //eS.Sort((x, y) => x.ToInt64().CompareTo(y.ToInt64()));

            return eS;
        }

        public static IntPtr getWindowByContainsName(Process[] p, string windowName)
        {
            var WindowHandles = EnumerateProcessWindowHandles(p[0]);
            var WindowHandelsToList = WindowHandles.ToList();

            IntPtr Window = IntPtr.Zero;

            foreach (IntPtr s in WindowHandelsToList)
            {
                if (GetWindowTitle(s).Contains(windowName))
                {
                    Window = s;
                    break;
                }
            }

            return Window;
        }
    }
}
