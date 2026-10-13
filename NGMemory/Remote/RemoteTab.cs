using System;

namespace NGMemory.Remote
{
    public static class RemoteTab
    {
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint PSM_SETCURSEL = 0x0465;
        private const uint TCM_GETITEMCOUNT = 0x1304;
        private const uint TCM_GETCURSEL = 0x130B;
        private const uint TCM_GETITEMRECT = 0x130A;
        private const uint TCM_GETITEMW = 0x133C;
        private const int TCIF_TEXT = 0x0001;
        private const int TextOffset = 128;
        private const int Capacity = 1024;

        public static int Count(IntPtr tabControl)
        {
            return Math.Max(0, RemoteMessage.Number(tabControl, TCM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero, 0));
        }

        public static int Current(IntPtr tabControl)
        {
            return RemoteMessage.Number(tabControl, TCM_GETCURSEL, IntPtr.Zero, IntPtr.Zero, -1);
        }

        public static string[] Labels(IntPtr tabControl)
        {
            int count = Count(tabControl);

            if (count == 0) return new string[0];

            using (RemoteMemory memory = RemoteMemory.Open(tabControl, TextOffset + Capacity * 2))
            {
                if (memory == null) return new string[0];

                var labels = new string[count];

                for (int i = 0; i < count; i++)
                {
                    byte[] item = new RemoteStruct(memory.PointerSize)
                        .Int(TCIF_TEXT).Int(0).Int(0)
                        .Pointer(memory.At(TextOffset)).Int(Capacity).Int(0)
                        .Pointer(IntPtr.Zero)
                        .ToArray();
                    IntPtr found;

                    if (!memory.Write(item) || !memory.Send(tabControl, TCM_GETITEMW, new IntPtr(i), out found)) return new string[0];

                    IntPtr text = memory.ReadPointer(memory.PointerSize == 8 ? 16 : 12);

                    labels[i] = found == IntPtr.Zero ? string.Empty : memory.ReadString(text, Capacity).Replace("&", string.Empty);
                }

                return labels;
            }
        }

        public static int IndexOf(IntPtr tabControl, string label)
        {
            string wanted = Plain(label);

            if (wanted.Length == 0) return -1;

            string[] labels = Array.ConvertAll(Labels(tabControl), Plain);
            int index = Array.FindIndex(labels, item => string.Equals(item, wanted, StringComparison.OrdinalIgnoreCase));

            if (index < 0) index = Array.FindIndex(labels, item => item.StartsWith(wanted, StringComparison.OrdinalIgnoreCase));

            if (index < 0) index = Array.FindIndex(labels, item => item.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0);

            return index;
        }

        public static bool Select(IntPtr tabControl, int index, int verifyMs = 2500)
        {
            if (index < 0 || index >= Count(tabControl)) return false;

            if (Current(tabControl) == index) return true;

            int wait = Math.Max(0, verifyMs);
            IntPtr parent = RemoteNative.GetParent(tabControl);

            if (!RemoteMessage.Send(parent, PSM_SETCURSEL, new IntPtr(index), IntPtr.Zero)) return Current(tabControl) == index;

            if (RemotePoll.Until(() => Current(tabControl) == index, Math.Min(wait / 3, 400))) return true;

            if (!Click(tabControl, index)) return false;

            return verifyMs <= 0 || RemotePoll.Until(() => Current(tabControl) == index, wait);
        }

        private static bool Click(IntPtr tabControl, int index)
        {
            int[] item;

            using (RemoteMemory memory = RemoteMemory.Open(tabControl, 64))
            {
                IntPtr found;

                if (memory == null || !memory.Send(tabControl, TCM_GETITEMRECT, new IntPtr(index), out found) || found == IntPtr.Zero) return false;

                item = memory.ReadInts(4);
            }

            RemoteNative.Rect client;

            if (item == null || !RemoteNative.GetClientRect(tabControl, out client)) return false;

            int x = (item[0] + item[2]) / 2;
            int y = (item[1] + item[3]) / 2;

            if (x < 0 || y < 0 || x >= client.Right || y >= client.Bottom) return false;

            var point = new IntPtr((y << 16) | x);

            return RemoteMessage.Post(tabControl, WM_LBUTTONDOWN, new IntPtr(1), point)
                && RemoteMessage.Post(tabControl, WM_LBUTTONUP, IntPtr.Zero, point);
        }

        private static string Plain(string label)
        {
            return (label ?? string.Empty).Replace("&", string.Empty).Trim();
        }
    }
}
