using System;

namespace NGMemory.Remote
{
    public static class RemoteKeys
    {
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;

        public static bool Press(IntPtr window, int virtualKey)
        {
            uint scan = RemoteNative.MapVirtualKey((uint)virtualKey, 0) & 0xFF;
            uint down = 1 | (scan << 16);
            uint up = down | (1u << 30) | (1u << 31);

            return RemoteMessage.Post(window, WM_KEYDOWN, new IntPtr(virtualKey), Param(down))
                && RemoteMessage.Post(window, WM_KEYUP, new IntPtr(virtualKey), Param(up));
        }

        private static IntPtr Param(uint value)
        {
            return IntPtr.Size == 8 ? new IntPtr((long)value) : new IntPtr(unchecked((int)value));
        }
    }
}
