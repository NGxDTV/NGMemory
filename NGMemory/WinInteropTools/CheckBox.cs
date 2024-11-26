using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NGMemory.Kernel32;
using static NGMemory.WinInteropTools.GuiInteropHandler;

namespace NGMemory.WinInteropTools
{
    public class CheckBox
    {
        public static bool IsCheckBoxChecked(IntPtr Pointer, int Steuerelement)
        {
            const int BM_GETCHECK = 0x00F0;
            IntPtr result = SendMessage(getRef(Pointer, Steuerelement), BM_GETCHECK, IntPtr.Zero, new StringBuilder());
            return result.ToInt32() == 1;
        }
    }
}
