using NGMemory.Easy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NGMemory.Overlay
{
    public class OverlayStyleHelper
    {
        private const int GWL_EXSTYLE = -20;
        private const int GWL_HWNDPARENT = -8;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Hides the specified window from the Alt+Tab application switcher.
        /// </summary>
        public static void HideFromAltTab(IntPtr hWnd, IntPtr owner)
        {
            if (hWnd == IntPtr.Zero) return;
            int ex = GetWindowLong(hWnd, GWL_EXSTYLE);
            ex |= WS_EX_TOOLWINDOW;
            ex &= ~WS_EX_APPWINDOW;
            ex |= WS_EX_NOACTIVATE;
            SetWindowLong(hWnd, GWL_EXSTYLE, ex);
            if (owner != IntPtr.Zero)
            {
                SetWindowLong(hWnd, GWL_HWNDPARENT, owner.ToInt32());
            }
        }
    }
}
