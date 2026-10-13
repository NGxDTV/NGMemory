using System;

namespace NGMemory.Remote
{
    internal static class RemoteBitness
    {
        private const uint QueryLimited = 0x1000;

        internal static int PointerSize(IntPtr process)
        {
            if (!Environment.Is64BitOperatingSystem) return 4;

            bool wow64;

            if (!RemoteNative.IsWow64Process(process, out wow64)) return 0;

            return wow64 ? 4 : 8;
        }

        internal static int PointerSizeOf(IntPtr window)
        {
            uint processId;

            if (window == IntPtr.Zero || RemoteNative.GetWindowThreadProcessId(window, out processId) == 0) return 0;

            IntPtr process = RemoteNative.OpenProcess(QueryLimited, false, processId);

            if (process == IntPtr.Zero) return 0;

            try
            {
                return PointerSize(process);
            }
            finally
            {
                RemoteNative.CloseHandle(process);
            }
        }
    }
}
