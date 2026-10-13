using System;
using System.Runtime.InteropServices;

namespace NGMemory.Remote
{
    public static class RemoteText
    {
        private const uint WM_SETTEXT = 0x000C;
        private const int EN_KILLFOCUS = 0x0200;
        private const int EN_CHANGE = 0x0300;

        public static string Get(IntPtr control)
        {
            return RemoteMessage.Text(control);
        }

        public static bool Set(IntPtr control, string text, int verifyMs = 2000)
        {
            string value = text ?? string.Empty;
            IntPtr buffer = Marshal.StringToHGlobalUni(value);

            try
            {
                if (!RemoteMessage.Send(control, WM_SETTEXT, IntPtr.Zero, buffer)) return false;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            RemoteMessage.Command(control, EN_CHANGE);
            RemoteMessage.Command(control, EN_KILLFOCUS);

            return verifyMs <= 0 || RemotePoll.Until(() => Get(control) == value, verifyMs);
        }
    }
}
