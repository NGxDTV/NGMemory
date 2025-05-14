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

        public static void PressKeysWithDelay(int delayBetweenKeysMs, params KeyCode[] scanCodes)
        {
            if (scanCodes == null || scanCodes.Length == 0) return;
            ushort[] codes = Array.ConvertAll(scanCodes, sc => (ushort)sc);
            
            ThreadPool.QueueUserWorkItem(_ => {
                foreach (var code in codes)
                {
                    Scan(code, false);  // Taste runter
                    Thread.Sleep(10);   // Kurze Pause
                    Scan(code, true);   // Taste hoch
                    if (delayBetweenKeysMs > 0 && code != codes[codes.Length - 1])
                        Thread.Sleep(delayBetweenKeysMs);
                }
            });
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

        // Neue Methoden für die InputHelper-Klasse

        public static void MouseMoveTo(int x, int y)
        {
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = x * (65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width),
                        dy = y * (65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height),
                        dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE
                    }
                }
            };
            
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MouseClick(Easy.MouseButton button)
        {
            uint downFlag = button == Easy.MouseButton.Left ? MOUSEEVENTF_LEFTDOWN : 
                           (button == Easy.MouseButton.Right ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_MIDDLEDOWN);
            
            uint upFlag = button == Easy.MouseButton.Left ? MOUSEEVENTF_LEFTUP : 
                         (button == Easy.MouseButton.Right ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_MIDDLEUP);
            
            INPUT inputDown = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION { mi = new MOUSEINPUT { dwFlags = downFlag } }
            };
            
            INPUT inputUp = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION { mi = new MOUSEINPUT { dwFlags = upFlag } }
            };
            
            SendInput(1, new[] { inputDown }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(10);
            SendInput(1, new[] { inputUp }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MouseDown(Easy.MouseButton button)
        {
            uint flag = button == Easy.MouseButton.Left ? MOUSEEVENTF_LEFTDOWN : 
                       (button == Easy.MouseButton.Right ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_MIDDLEDOWN);
            
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION { mi = new MOUSEINPUT { dwFlags = flag } }
            };
            
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MouseUp(Easy.MouseButton button)
        {
            uint flag = button == Easy.MouseButton.Left ? MOUSEEVENTF_LEFTUP : 
                       (button == Easy.MouseButton.Right ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_MIDDLEUP);
            
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION { mi = new MOUSEINPUT { dwFlags = flag } }
            };
            
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
