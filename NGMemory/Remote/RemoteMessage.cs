using System;
using System.Runtime.InteropServices;

namespace NGMemory.Remote
{
    internal static class RemoteMessage
    {
        internal const int DefaultTimeout = 4000;

        private const uint AbortIfHung = 0x0002;
        private const uint WM_GETTEXT = 0x000D;
        private const uint WM_GETTEXTLENGTH = 0x000E;
        private const uint WM_COMMAND = 0x0111;

        internal static bool Send(IntPtr window, uint message, IntPtr wParam, IntPtr lParam, out IntPtr result)
        {
            result = IntPtr.Zero;

            if (window == IntPtr.Zero || !RemoteNative.IsWindow(window)) return false;

            return RemoteNative.SendMessageTimeout(window, message, wParam, lParam, AbortIfHung, DefaultTimeout, out result) != IntPtr.Zero;
        }

        internal static bool Send(IntPtr window, uint message, IntPtr wParam, IntPtr lParam)
        {
            IntPtr result;

            return Send(window, message, wParam, lParam, out result);
        }

        internal static int Number(IntPtr window, uint message, IntPtr wParam, IntPtr lParam, int fallback)
        {
            IntPtr result;

            return Send(window, message, wParam, lParam, out result) ? unchecked((int)result.ToInt64()) : fallback;
        }

        internal static bool Post(IntPtr window, uint message, IntPtr wParam, IntPtr lParam)
        {
            return window != IntPtr.Zero && RemoteNative.IsWindow(window) && RemoteNative.PostMessage(window, message, wParam, lParam);
        }

        internal static bool Command(IntPtr control, int code)
        {
            IntPtr parent = RemoteNative.GetParent(control);
            int id = RemoteNative.GetDlgCtrlID(control) & 0xFFFF;

            return Send(parent, WM_COMMAND, new IntPtr(((code & 0xFFFF) << 16) | id), control);
        }

        internal static string Text(IntPtr window)
        {
            int capacity = Number(window, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero, -1);

            if (capacity < 0) return string.Empty;

            capacity += 2;

            for (int attempt = 0; ; attempt++)
            {
                IntPtr buffer = Marshal.AllocHGlobal(capacity * 2);

                try
                {
                    Marshal.WriteInt16(buffer, 0, 0);

                    IntPtr copied;

                    if (!Send(window, WM_GETTEXT, new IntPtr(capacity), buffer, out copied)) return string.Empty;

                    Marshal.WriteInt16(buffer, (capacity - 1) * 2, 0);

                    if (copied.ToInt64() < capacity - 1 || attempt >= 6) return Marshal.PtrToStringUni(buffer) ?? string.Empty;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }

                capacity *= 4;
            }
        }
    }
}
