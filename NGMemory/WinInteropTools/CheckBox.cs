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
            IntPtr handle = getRef(Pointer, Steuerelement).Handle;
            if (global::System.Windows.Forms.Control.FromHandle(handle) is global::System.Windows.Forms.CheckBox checkBox)
                return checkBox.Checked;

            IntPtr result = SendMessage(handle, BM_GETCHECK, IntPtr.Zero, IntPtr.Zero);
            return result.ToInt32() == BST_CHECKED;
        }

        public static void SetCheckBoxState(IntPtr Pointer, int Steuerelement, bool isChecked)
        {
            int state = isChecked ? BST_CHECKED : BST_UNCHECKED;
            IntPtr handle = getRef(Pointer, Steuerelement).Handle;
            if (global::System.Windows.Forms.Control.FromHandle(handle) is global::System.Windows.Forms.CheckBox checkBox)
            {
                checkBox.Checked = isChecked;
                return;
            }

            SendMessage(handle, BM_SETCHECK, (IntPtr)state, IntPtr.Zero);
        }

    }
}
