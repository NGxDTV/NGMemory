using System;
using System.Runtime.InteropServices;
using System.Threading;

using static NGMemory.User32;
using static NGMemory.Constants;
using static NGMemory.Enums;

namespace NGMemory.WinInteropTools
{
    public class InputHelper
    {
        // Kombi beliebiger Tasten-ScanCodes
        public static void PressKeys(bool async, params KeyCode[] scanCodes)
        {
            if (scanCodes == null || scanCodes.Length == 0) return;
            ushort[] codes = Array.ConvertAll(scanCodes, sc => (ushort)sc);
            if (async) ThreadPool.QueueUserWorkItem(_ => SendCore(codes));
            else SendCore(codes);
        }

        static void SendCore(ushort[] codes)
        {
            foreach (var c in codes) Scan(c, false);             // down
            for (int i = codes.Length - 1; i >= 0; i--) Scan(codes[i], true); // up
        }

        static void Scan(ushort code, bool up)
        {
            var inp = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wScan = code,
                        dwFlags = KEYEVENTF_SCANCODE | (up ? KEYEVENTF_KEYUP : 0)
                    }
                }
            };
            SendInput(1, new[] { inp }, Marshal.SizeOf(typeof(INPUT)));
        }

        static void SendCtrlCInput()
        {
            Scan((ushort)KeyCode.LCtrl, false); // Ctrl↓
            Scan((ushort)KeyCode.C, false); // C↓
            Scan((ushort)KeyCode.C, true);  // C↑
            Scan((ushort)KeyCode.LCtrl, true);  // Ctrl↑
        }

        static void CopyCore(IntPtr hWnd)
        {
            if (!PostMessage(hWnd, WM_COPY, IntPtr.Zero, IntPtr.Zero))
                SendCtrlCInput();
        }

        public static void CopySelection(IntPtr hWnd, bool async)
        {
            if (async) ThreadPool.QueueUserWorkItem(_ => CopyCore(hWnd));
            else CopyCore(hWnd);
        }
    }
}
