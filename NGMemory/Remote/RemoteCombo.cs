using System;
using System.Runtime.InteropServices;

namespace NGMemory.Remote
{
    public static class RemoteCombo
    {
        private const uint CB_GETCOUNT = 0x0146;
        private const uint CB_GETCURSEL = 0x0147;
        private const uint CB_GETLBTEXT = 0x0148;
        private const uint CB_GETLBTEXTLEN = 0x0149;
        private const uint CB_SETCURSEL = 0x014E;
        private const int CBN_SELCHANGE = 1;
        private const int CBN_SELENDOK = 9;
        private const int Slack = 64;

        public static string[] Items(IntPtr combo)
        {
            int count = Math.Max(0, RemoteMessage.Number(combo, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero, 0));
            var items = new string[count];

            for (int i = 0; i < count; i++) items[i] = Item(combo, i);

            return items;
        }

        public static string Selected(IntPtr combo)
        {
            int index = Current(combo);

            return index < 0 ? string.Empty : Item(combo, index);
        }

        public static bool Select(IntPtr combo, string text, int verifyMs = 2000)
        {
            string wanted = (text ?? string.Empty).Trim();
            int index = Array.FindIndex(Items(combo), item => string.Equals(item.Trim(), wanted, StringComparison.OrdinalIgnoreCase));

            if (index < 0) return false;

            if (Current(combo) == index) return true;

            if (!RemoteMessage.Send(combo, CB_SETCURSEL, new IntPtr(index), IntPtr.Zero)) return false;

            RemoteMessage.Command(combo, CBN_SELENDOK);
            RemoteMessage.Command(combo, CBN_SELCHANGE);

            return verifyMs <= 0 || RemotePoll.Until(() => Current(combo) == index, verifyMs);
        }

        private static int Current(IntPtr combo)
        {
            return RemoteMessage.Number(combo, CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero, -1);
        }

        private static string Item(IntPtr combo, int index)
        {
            int length = RemoteMessage.Number(combo, CB_GETLBTEXTLEN, new IntPtr(index), IntPtr.Zero, -1);

            if (length < 0) return string.Empty;

            int capacity = length + Slack;
            IntPtr buffer = Marshal.AllocHGlobal(capacity * 2);

            try
            {
                Marshal.WriteInt16(buffer, 0, 0);

                IntPtr copied;

                if (!RemoteMessage.Send(combo, CB_GETLBTEXT, new IntPtr(index), buffer, out copied)) return string.Empty;

                Marshal.WriteInt16(buffer, (capacity - 1) * 2, 0);

                return Marshal.PtrToStringUni(buffer) ?? string.Empty;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
