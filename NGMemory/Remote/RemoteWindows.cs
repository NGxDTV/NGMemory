using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace NGMemory.Remote
{
    public static class RemoteWindows
    {
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const uint GW_OWNER = 4;
        private const uint WM_CLOSE = 0x0010;

        public static IReadOnlyList<IntPtr> TopLevel(string processName)
        {
            var windows = new List<IntPtr>();
            HashSet<uint> processIds = ProcessIds(processName);

            if (processIds.Count == 0) return windows;

            RemoteNative.EnumWindows((window, parameter) =>
            {
                uint processId;
                RemoteNative.GetWindowThreadProcessId(window, out processId);

                if (processIds.Contains(processId) && RemoteNative.IsWindowVisible(window)) windows.Add(window);

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public static IReadOnlyList<IntPtr> Captioned(string processName)
        {
            var windows = new List<IntPtr>();

            foreach (IntPtr top in TopLevel(processName))
            {
                windows.Add(top);

                foreach (IntPtr child in Descendants(top))
                {
                    bool captioned = (RemoteNative.GetWindowLong(child, GWL_STYLE) & WS_CAPTION) == WS_CAPTION;

                    if (captioned && RemoteNative.IsWindowVisible(child)) windows.Add(child);
                }
            }

            return windows;
        }

        public static IntPtr Find(string processName, string className, string titleContains)
        {
            foreach (IntPtr window in Captioned(processName))
            {
                if (Matches(window, className, titleContains)) return window;
            }

            return IntPtr.Zero;
        }

        public static IReadOnlyList<IntPtr> FindAll(string processName, string className, string titleContains)
        {
            var windows = new List<IntPtr>();

            foreach (IntPtr window in Captioned(processName))
            {
                if (Matches(window, className, titleContains)) windows.Add(window);
            }

            return windows;
        }

        public static bool IsRunning(string processName)
        {
            return ProcessIds(processName).Count > 0;
        }

        public static string Title(IntPtr window)
        {
            return RemoteMessage.Text(window);
        }

        public static string ClassName(IntPtr window)
        {
            var text = new StringBuilder(260);

            return RemoteNative.GetClassName(window, text, text.Capacity) > 0 ? text.ToString() : string.Empty;
        }

        public static bool IsAlive(IntPtr window)
        {
            return window != IntPtr.Zero && RemoteNative.IsWindow(window) && RemoteNative.IsWindowVisible(window);
        }

        public static bool IsEnabled(IntPtr window)
        {
            return window != IntPtr.Zero && RemoteNative.IsWindow(window) && RemoteNative.IsWindowEnabled(window);
        }

        public static IntPtr Owner(IntPtr window)
        {
            return window == IntPtr.Zero ? IntPtr.Zero : RemoteNative.GetWindow(window, GW_OWNER);
        }

        public static IReadOnlyList<IntPtr> Descendants(IntPtr parent)
        {
            var windows = new List<IntPtr>();

            if (parent == IntPtr.Zero || !RemoteNative.IsWindow(parent)) return windows;

            RemoteNative.EnumChildWindows(parent, (window, parameter) =>
            {
                windows.Add(window);

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public static IntPtr Control(IntPtr parent, int controlId)
        {
            if (parent == IntPtr.Zero || !RemoteNative.IsWindow(parent)) return IntPtr.Zero;

            IntPtr direct = RemoteNative.GetDlgItem(parent, controlId);

            if (direct != IntPtr.Zero) return direct;

            foreach (IntPtr window in Descendants(parent))
            {
                if (ControlId(window) == (controlId & 0xFFFF) && RemoteNative.IsWindowVisible(window)) return window;
            }

            return IntPtr.Zero;
        }

        public static int ControlId(IntPtr control)
        {
            return control == IntPtr.Zero ? 0 : RemoteNative.GetDlgCtrlID(control) & 0xFFFF;
        }

        public static bool Is32Bit(IntPtr window)
        {
            return RemoteBitness.PointerSizeOf(window) == 4;
        }

        public static bool Close(IntPtr window)
        {
            return RemoteMessage.Post(window, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static Rectangle Bounds(IntPtr window)
        {
            RemoteNative.Rect rect;

            if (window == IntPtr.Zero || !RemoteNative.GetWindowRect(window, out rect)) return Rectangle.Empty;

            return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static Rectangle BoundsIn(IntPtr control, IntPtr ancestor)
        {
            RemoteNative.Rect rect;

            if (control == IntPtr.Zero || ancestor == IntPtr.Zero || !RemoteNative.IsWindow(ancestor)) return Rectangle.Empty;

            if (!RemoteNative.GetWindowRect(control, out rect)) return Rectangle.Empty;

            RemoteNative.MapWindowPoints(IntPtr.Zero, ancestor, ref rect, 2);

            return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        private static bool Matches(IntPtr window, string className, string titleContains)
        {
            if (className != null && !string.Equals(ClassName(window), className, StringComparison.OrdinalIgnoreCase)) return false;

            return titleContains == null || Title(window).IndexOf(titleContains, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static HashSet<uint> ProcessIds(string processName)
        {
            var ids = new HashSet<uint>();
            string name = (processName ?? string.Empty).Trim();

            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name = name.Substring(0, name.Length - 4);

            if (name.Length == 0) return ids;

            try
            {
                foreach (Process process in Process.GetProcessesByName(name))
                {
                    using (process) ids.Add((uint)process.Id);
                }
            }
            catch (Exception)
            {
                ids.Clear();
            }

            return ids;
        }
    }
}
