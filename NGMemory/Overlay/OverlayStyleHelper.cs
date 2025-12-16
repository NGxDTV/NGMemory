using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NGMemory.Overlay
{
    /// <summary>
    /// Helper methods for manipulating extended window styles (EXSTYLE) and
    /// controlling whether a window appears in the Alt-Tab list.
    /// </summary>
    public class OverlayStyleHelper
    {
        /// <summary>
        /// Index for GetWindowLong/SetWindowLong to read the extended window style.
        /// </summary>
        private const int GWL_EXSTYLE = -20;

        /// <summary>
        /// Extended window style: Tool window (hidden from Alt-Tab).
        /// </summary>
        private const long WS_EX_TOOLWINDOW = 0x00000080L;

        /// <summary>
        /// Extended window style: App window (shown in Alt-Tab).
        /// </summary>
        private const long WS_EX_APPWINDOW = 0x00040000L;

        /// <summary>
        /// Extended window style: Window does not activate.
        /// </summary>
        private const long WS_EX_NOACTIVATE = 0x08000000L;

        /// <summary>
        /// SetWindowPos flag: do not change position.
        /// </summary>
        private const uint SWP_NOMOVE = 0x0002;

        /// <summary>
        /// SetWindowPos flag: do not change size.
        /// </summary>
        private const uint SWP_NOSIZE = 0x0001;

        /// <summary>
        /// SetWindowPos flag: do not change Z order.
        /// </summary>
        private const uint SWP_NOZORDER = 0x0004;

        /// <summary>
        /// SetWindowPos flag: force frame change.
        /// </summary>
        private const uint SWP_FRAMECHANGED = 0x0020;

        /// <summary>
        /// Stores previous EXSTYLE values for windows so they can be restored.
        /// </summary>
        private static readonly Dictionary<IntPtr, long> _oldExStyle = new Dictionary<IntPtr, long>();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// Reads the current extended window style (EXSTYLE) of the specified window.
        /// Supports both 32-bit and 64-bit processes.
        /// </summary>
        /// <param name="hWnd">Handle of the window.</param>
        /// <returns>The EXSTYLE value as a 64-bit integer.</returns>
        private static long GetExStyle(IntPtr hWnd)
        {
            if (IntPtr.Size == 8) return GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64();
            return GetWindowLong(hWnd, GWL_EXSTYLE);
        }

        /// <summary>
        /// Sets the extended window style (EXSTYLE) for the specified window and
        /// forces a frame update using SetWindowPos.
        /// </summary>
        /// <param name="hWnd">Handle of the window.</param>
        /// <param name="exStyle">New EXSTYLE value.</param>
        private static void SetExStyle(IntPtr hWnd, long exStyle)
        {
            if (IntPtr.Size == 8) SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle));
            else SetWindowLong(hWnd, GWL_EXSTYLE, unchecked((int)exStyle));
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        /// <summary>
        /// Hides the window from the Alt-Tab list by setting the ToolWindow style
        /// and removing the AppWindow style. Stores the old style for later restoration.
        /// </summary>
        /// <param name="hWnd">Handle of the window to modify.</param>
        /// <param name="owner">Optional: owner of the window (currently unused).</param>
        public static void HideFromAltTab(IntPtr hWnd, IntPtr owner)
        {
            if (hWnd == IntPtr.Zero) return;

            if (!_oldExStyle.ContainsKey(hWnd))
                _oldExStyle[hWnd] = GetExStyle(hWnd);

            long ex = GetExStyle(hWnd);
            ex |= WS_EX_TOOLWINDOW;
            ex &= ~WS_EX_APPWINDOW;
            ex |= WS_EX_NOACTIVATE;

            SetExStyle(hWnd, ex);
        }

        /// <summary>
        /// Restores the previously stored EXSTYLE value for the window, if present.
        /// </summary>
        /// <param name="hWnd">Handle of the window.</param>
        public static void RestoreAltTab(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;

            if (_oldExStyle.TryGetValue(hWnd, out var ex))
            {
                SetExStyle(hWnd, ex);
                _oldExStyle.Remove(hWnd);
            }
        }

        /// <summary>
        /// Forces the window to appear in the Alt-Tab list by removing the ToolWindow style
        /// and setting the AppWindow style.
        /// </summary>
        /// <param name="hWnd">Handle of the window.</param>
        public static void ForceShowInAltTab(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;

            long ex = GetExStyle(hWnd);
            ex &= ~WS_EX_TOOLWINDOW;
            ex |= WS_EX_APPWINDOW;
            ex &= ~WS_EX_NOACTIVATE;

            SetExStyle(hWnd, ex);
        }
    }
}
