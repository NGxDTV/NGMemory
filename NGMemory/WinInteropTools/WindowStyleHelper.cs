using System;
using System.Runtime.InteropServices;
using static NGMemory.Constants;

namespace NGMemory.WinInteropTools
{
    /// <summary>
    /// Helper for modifying window styles and attributes.
    /// </summary>
    public static class WindowStyleHelper
    {
        /// <summary>
        /// Sets a window to be transparent and layered.
        /// </summary>
        public static bool MakeWindowTransparent(IntPtr hwnd)
        {
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle |= 0x00080000; // WS_EX_LAYERED
            return SetWindowLong(hwnd, GWL_EXSTYLE, exStyle) != 0;
        }

        /// <summary>
        /// Makes a window a child window of another window.
        /// </summary>
        public static bool MakeChildWindow(IntPtr childHwnd, IntPtr parentHwnd)
        {
            if (childHwnd == IntPtr.Zero || parentHwnd == IntPtr.Zero)
                return false;

            // Set parent
            IntPtr result = SetParent(childHwnd, parentHwnd);
            if (result == IntPtr.Zero)
                return false;

            // Modify style to be a child window
            int style = GetWindowLong(childHwnd, GWL_STYLE);
            style &= ~WS_POPUP;
            style |= WS_CHILD;
            
            SetWindowLong(childHwnd, GWL_STYLE, style);
            return true;
        }

        /// <summary>
        /// Sets a window's position within its parent.
        /// </summary>
        public static bool SetWindowPosition(IntPtr hwnd, int x, int y, int width, int height, bool noZOrder = true)
        {
            uint flags = SWP_NOACTIVATE;
            if (noZOrder)
                flags |= SWP_NOZORDER;

            return SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, flags);
        }

        /// <summary>
        /// Makes the window topmost.
        /// </summary>
        public static bool MakeWindowTopMost(IntPtr hwnd)
        {
            return SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, 
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        /// <summary>
        /// Get current window style.
        /// </summary>
        public static int GetWindowStyle(IntPtr hwnd)
        {
            return GetWindowLong(hwnd, GWL_STYLE);
        }

        /// <summary>
        /// Get current extended window style.
        /// </summary>
        public static int GetWindowExStyle(IntPtr hwnd)
        {
            return GetWindowLong(hwnd, GWL_EXSTYLE);
        }

        #region Native Methods

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Additional constants
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;

        #endregion
    }
}
