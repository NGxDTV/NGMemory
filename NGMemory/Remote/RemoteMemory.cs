using System;
using System.Text;

namespace NGMemory.Remote
{
    internal sealed class RemoteMemory : IDisposable
    {
        private const uint Access = 0x0438;
        private const uint Commit = 0x3000;
        private const uint Release = 0x8000;
        private const uint ReadWrite = 0x04;
        private const int PageSize = 4096;

        private IntPtr process;
        private bool abandoned;

        private RemoteMemory(IntPtr process, int pointerSize)
        {
            this.process = process;
            PointerSize = pointerSize;
        }

        internal IntPtr Address { get; private set; }

        internal int PointerSize { get; }

        internal static RemoteMemory Open(IntPtr window, int size)
        {
            uint processId;

            if (window == IntPtr.Zero || RemoteNative.GetWindowThreadProcessId(window, out processId) == 0) return null;

            IntPtr process = RemoteNative.OpenProcess(Access, false, processId);

            if (process == IntPtr.Zero) return null;

            int pointerSize = RemoteBitness.PointerSize(process);
            var memory = new RemoteMemory(process, pointerSize);

            if (pointerSize != 0 && pointerSize <= IntPtr.Size && memory.Reserve(size)) return memory;

            memory.Dispose();

            return null;
        }

        internal bool Reserve(int size)
        {
            Free();
            Address = RemoteNative.VirtualAllocEx(process, IntPtr.Zero, new IntPtr(size), Commit, ReadWrite);

            return Address != IntPtr.Zero;
        }

        internal IntPtr At(int offset)
        {
            return Offset(Address, offset);
        }

        internal bool Write(byte[] data)
        {
            IntPtr written;

            return RemoteNative.WriteProcessMemory(process, Address, data, new IntPtr(data.Length), out written);
        }

        internal bool Send(IntPtr window, uint message, IntPtr wParam, out IntPtr result)
        {
            if (RemoteMessage.Send(window, message, wParam, Address, out result)) return true;

            // A timed-out message can still be handled later; freeing the block would let the target write to released memory.
            if (RemoteNative.IsWindow(window)) abandoned = true;

            return false;
        }

        internal int[] ReadInts(int count)
        {
            var buffer = new byte[count * 4];
            var values = new int[count];

            if (!Read(Address, buffer, buffer.Length)) return null;

            Buffer.BlockCopy(buffer, 0, values, 0, buffer.Length);

            return values;
        }

        internal IntPtr ReadPointer(int offset)
        {
            var buffer = new byte[PointerSize];

            if (!Read(At(offset), buffer, buffer.Length)) return IntPtr.Zero;

            long value = PointerSize == 8 ? BitConverter.ToInt64(buffer, 0) : BitConverter.ToUInt32(buffer, 0);

            return IntPtr.Size == 8 ? new IntPtr(value) : new IntPtr(unchecked((int)value));
        }

        internal string ReadString(IntPtr address, int maxChars)
        {
            var text = new StringBuilder();
            var buffer = new byte[PageSize];

            while (text.Length < maxChars)
            {
                int toPageEnd = PageSize - (int)(address.ToInt64() & (PageSize - 1));
                int chunk = Math.Min(toPageEnd, (maxChars - text.Length) * 2) & ~1;

                if (chunk == 0 || !Read(address, buffer, chunk)) break;

                for (int i = 0; i < chunk; i += 2)
                {
                    char value = (char)(buffer[i] | (buffer[i + 1] << 8));

                    if (value == '\0') return text.ToString();

                    text.Append(value);
                }

                address = Offset(address, chunk);
            }

            return text.ToString();
        }

        public void Dispose()
        {
            if (process == IntPtr.Zero) return;

            Free();
            RemoteNative.CloseHandle(process);
            process = IntPtr.Zero;
        }

        private bool Read(IntPtr address, byte[] buffer, int length)
        {
            IntPtr read;

            return RemoteNative.ReadProcessMemory(process, address, buffer, new IntPtr(length), out read) && read.ToInt64() == length;
        }

        private void Free()
        {
            if (Address != IntPtr.Zero && !abandoned) RemoteNative.VirtualFreeEx(process, Address, IntPtr.Zero, Release);

            Address = IntPtr.Zero;
            abandoned = false;
        }

        private static IntPtr Offset(IntPtr address, int offset)
        {
            long value = address.ToInt64() + offset;

            return IntPtr.Size == 8 ? new IntPtr(value) : new IntPtr(unchecked((int)value));
        }
    }
}
