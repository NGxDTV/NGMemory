using System;
using System.Collections.Generic;

namespace NGMemory.Remote
{
    internal sealed class RemoteStruct
    {
        private readonly List<byte> bytes = new List<byte>();
        private readonly int pointerSize;

        internal RemoteStruct(int pointerSize)
        {
            this.pointerSize = pointerSize;
        }

        internal RemoteStruct Int(int value)
        {
            Align(4);
            bytes.AddRange(BitConverter.GetBytes(value));

            return this;
        }

        internal RemoteStruct Pointer(IntPtr value)
        {
            Align(pointerSize);

            if (pointerSize == 8) bytes.AddRange(BitConverter.GetBytes(value.ToInt64()));
            else bytes.AddRange(BitConverter.GetBytes(unchecked((int)value.ToInt64())));

            return this;
        }

        internal byte[] ToArray()
        {
            Align(pointerSize);

            return bytes.ToArray();
        }

        private void Align(int size)
        {
            while (bytes.Count % size != 0) bytes.Add(0);
        }
    }
}
