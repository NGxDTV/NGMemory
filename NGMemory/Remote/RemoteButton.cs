using System;
using System.Threading;

namespace NGMemory.Remote
{
    public static class RemoteButton
    {
        private const uint BM_GETCHECK = 0x00F0;
        private const uint BM_SETCHECK = 0x00F1;
        private const uint BM_CLICK = 0x00F5;
        private const int BN_CLICKED = 0;
        private const int BST_CHECKED = 1;

        public static bool Click(IntPtr button)
        {
            if (!RemoteWindows.IsEnabled(button)) return false;

            return RemoteMessage.Post(button, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool IsChecked(IntPtr button)
        {
            return RemoteMessage.Number(button, BM_GETCHECK, IntPtr.Zero, IntPtr.Zero, 0) == BST_CHECKED;
        }

        public static bool SetChecked(IntPtr button, bool value, int verifyMs = 2000)
        {
            if (button == IntPtr.Zero || !RemoteNative.IsWindow(button)) return false;

            if (IsChecked(button) == value) return true;

            if (!Click(button)) return false;

            if (verifyMs <= 0) return true;

            if (RemotePoll.Until(() => IsChecked(button) == value, verifyMs / 2)) return true;

            RemoteMessage.Send(button, BM_SETCHECK, new IntPtr(value ? 1 : 0), IntPtr.Zero);
            RemoteMessage.Command(button, BN_CLICKED);

            if (!RemotePoll.Until(() => IsChecked(button) == value, verifyMs / 2)) return false;

            Thread.Sleep(100);

            return IsChecked(button) == value;
        }
    }
}
