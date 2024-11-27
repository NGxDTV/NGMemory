using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NGMemory.Kernel32;
using static NGMemory.WinInteropTools.GuiInteropHandler;

using static NGMemory.Structures;
using static NGMemory.User32;
using static NGMemory.Constants;
using static NGMemory.Enums;

namespace NGMemory.WinInteropTools
{
    public class CheckBox
    {
        public static bool IsCheckBoxChecked(IntPtr Pointer, int Steuerelement)
        {
            IntPtr result = SendMessage(getRef(Pointer, Steuerelement), BM_GETCHECK, IntPtr.Zero, new StringBuilder());
            return result.ToInt32() == 1;
        }
    }
}
