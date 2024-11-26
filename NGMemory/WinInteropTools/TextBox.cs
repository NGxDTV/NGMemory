using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NGMemory.Kernel32;
using static NGMemory.WinInteropTools.GuiInteropHandler;

namespace NGMemory.WinInteropTools
{
    public class TextBox
    {
        public static string getTextBoxValue(IntPtr Pointer, int Steuerelement)
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            Int32 TextLength = SendMessage(getRef(Pointer, Steuerelement), WM_GETTEXTLENGTH, IntPtr.Zero, (StringBuilder)null).ToInt32();
            SendMessage(getRef(Pointer, Steuerelement), WM_GETTEXT, new IntPtr(stringBuilder.Capacity), stringBuilder);
            return stringBuilder.ToString();
        }
    }
}
