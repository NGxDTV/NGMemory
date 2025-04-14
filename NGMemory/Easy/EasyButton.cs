using System;
using System.Threading.Tasks;
using NGMemory.WinInteropTools;

namespace NGMemory.Easy
{
    public static class EasyButton
    {
        public static void Click(IntPtr window, int controlId)
        {
            var handle = WinInteropTools.GuiInteropHandler.getRef(window, controlId).Handle;
            NGMemory.User32.SendMessage(handle, 0x00F5, IntPtr.Zero, IntPtr.Zero);
        }

        public static void ClickAsync(IntPtr window, int controlId)
        {
            Task.Run(() => Click(window, controlId));
        }
    }
}
