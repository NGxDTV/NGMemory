using System;

namespace NGMemory.Remote
{
    internal static class RemoteListItem
    {
        internal const int Text = 0x0001;
        internal const int State = 0x0008;

        internal static byte[] Build(int pointerSize, int mask, int row, int column, int state, int stateMask, IntPtr text, int capacity)
        {
            return new RemoteStruct(pointerSize)
                .Int(mask).Int(row).Int(column).Int(state).Int(stateMask)
                .Pointer(text).Int(capacity).Int(0)
                .Pointer(IntPtr.Zero).Int(0).Int(0).Int(0)
                .Pointer(IntPtr.Zero).Pointer(IntPtr.Zero).Int(0)
                .ToArray();
        }
    }
}
