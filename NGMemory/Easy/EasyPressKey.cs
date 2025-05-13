using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static NGMemory.Enums;

namespace NGMemory.Easy
{
    public class EasyPressKey
    {
        public static void PressKeys(bool async, params KeyCode[] scanCodes)
        {
            WinInteropTools.InputHelper.PressKeys(async, scanCodes);
        }
    }
}
