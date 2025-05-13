using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static NGMemory.User32;
using static NGMemory.Constants;
using System.Threading;

namespace NGMemory.WinInteropTools
{
    public class MenuStripHelper
    {
        public static void ClickMenu(IntPtr hWnd, bool async, params int[] path)
        {
            if (async) ThreadPool.QueueUserWorkItem(_ => Core(hWnd, path));
            else Core(hWnd, path);
        }

        public static void Core(IntPtr hWnd, int[] path)
        {
            IntPtr hCur = GetMenu(hWnd);
            for (int i = 0; i < path.Length - 1; i++)
                hCur = GetSubMenu(hCur, path[i]);

            uint id = GetMenuItemID(hCur, path[path.Length - 1]);
            SendMessage(hWnd, WM_COMMAND, (IntPtr)id, IntPtr.Zero);
        }
    }
}
