using System;

namespace NGMemory.Remote
{
    internal sealed class RemoteListReader : IDisposable
    {
        private const uint LVM_GETCOLUMNW = 0x105F;
        private const uint LVM_GETITEMTEXTW = 0x1073;
        private const int LVCF_TEXT = 0x0004;
        private const int TextOffset = 128;
        private const int MaxCapacity = 1 << 20;

        private readonly IntPtr listView;
        private readonly RemoteMemory memory;
        private int capacity = 4096;

        private RemoteListReader(IntPtr listView, RemoteMemory memory)
        {
            this.listView = listView;
            this.memory = memory;
        }

        internal static RemoteListReader Open(IntPtr listView)
        {
            RemoteMemory memory = RemoteMemory.Open(listView, TextOffset + 4096 * 2);

            return memory == null ? null : new RemoteListReader(listView, memory);
        }

        internal string Column(int index)
        {
            byte[] column = new RemoteStruct(memory.PointerSize)
                .Int(LVCF_TEXT).Int(0).Int(0)
                .Pointer(memory.At(TextOffset)).Int(capacity)
                .Int(0).Int(0).Int(0).Int(0).Int(0).Int(0)
                .ToArray();
            IntPtr found;

            if (!memory.Write(column) || !memory.Send(listView, LVM_GETCOLUMNW, new IntPtr(index), out found)) return null;

            if (found == IntPtr.Zero) return string.Empty;

            return memory.ReadString(memory.ReadPointer(memory.PointerSize == 8 ? 16 : 12), capacity);
        }

        internal string Cell(int row, int column)
        {
            while (true)
            {
                byte[] item = RemoteListItem.Build(memory.PointerSize, RemoteListItem.Text, row, column, 0, 0, memory.At(TextOffset), capacity);
                IntPtr copied;

                if (!memory.Write(item) || !memory.Send(listView, LVM_GETITEMTEXTW, new IntPtr(row), out copied)) return null;

                int length = Math.Max(0, Math.Min(unchecked((int)copied.ToInt64()), capacity - 1));

                if (length < capacity - 1 || capacity >= MaxCapacity) return memory.ReadString(memory.At(TextOffset), length);

                capacity *= 4;

                if (!memory.Reserve(TextOffset + capacity * 2)) return null;
            }
        }

        public void Dispose()
        {
            memory.Dispose();
        }
    }
}
