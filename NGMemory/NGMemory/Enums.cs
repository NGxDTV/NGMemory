using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGMemory
{
    public class Enums
    {
        public enum Register
        {
            Rax,
            Rcx,
            Rdx,
            Rbx,
            Rsp,
            Rbp,
            Rsi,
            Rdi,
            R8,
            R9,
            R10,
            R11,
            R12,
            R13,
            R14,
            R15,
            Rip,
            NULL
        }
        public enum MessageBoxType : uint
        {
            MB_OK = 0x00000000,
            MB_OKCANCEL = 0x00000001,
            MB_ABORTRETRYIGNORE = 0x00000002,
            MB_YESNOCANCEL = 0x00000003,
            MB_YESNO = 0x00000004,
            MB_RETRYCANCEL = 0x00000005,
            MB_CANCELTRYCONTINUE = 0x00000006,

            MB_ICONHAND = 0x00000010,
            MB_ICONQUESTION = 0x00000020,
            MB_ICONEXCLAMATION = 0x00000030,
            MB_ICONASTERISK = 0x00000040,

            MB_USERICON = 0x00000080,
            MB_ICONWARNING = MB_ICONEXCLAMATION,
            MB_ICONERROR = MB_ICONHAND,
            MB_ICONINFORMATION = MB_ICONASTERISK,
            MB_ICONSTOP = MB_ICONHAND,

            MB_DEFBUTTON1 = 0x00000000,
            MB_DEFBUTTON2 = 0x00000100,
            MB_DEFBUTTON3 = 0x00000200,
            MB_DEFBUTTON4 = 0x00000300,

            MB_APPLMODAL = 0x00000000,
            MB_SYSTEMMODAL = 0x00001000,
            MB_TASKMODAL = 0x00002000,

            MB_HELP = 0x00004000,
            MB_NOFOCUS = 0x00008000,
            MB_SETFOREGROUND = 0x00010000,
            MB_DEFAULT_DESKTOP_ONLY = 0x00020000,
            MB_TOPMOST = 0x00040000,
            MB_RIGHT = 0x00080000,
            MB_RTLREADING = 0x00100000,
            MB_SERVICE_NOTIFICATION = 0x00200000,

            MB_TYPEMASK = 0x0000000F,
            MB_ICONMASK = 0x000000F0,
            MB_DEFMASK = 0x00000F00,
            MB_MODEMASK = 0x00003000,
            MB_MISCMASK = 0x0000C000
        }

        public enum KeyCode : ushort
        {
            None = 0x00,
            Esc = 0x01,
            D1 = 0x02,
            D2 = 0x03,
            D3 = 0x04,
            D4 = 0x05,
            D5 = 0x06,
            D6 = 0x07,
            D7 = 0x08,
            D8 = 0x09,
            D9 = 0x0A,
            D0 = 0x0B,

            Minus = 0x0C,
            Equals = 0x0D,

            Backspace = 0x0E,
            Tab = 0x0F,

            Q = 0x10, 
            W = 0x11, 
            E = 0x12, 
            R = 0x13, 
            T = 0x14,
            Y = 0x15, 
            U = 0x16, 
            I = 0x17, 
            O = 0x18, 
            P = 0x19,

            LBracket = 0x1A, 
            RBracket = 0x1B, 
            Enter = 0x1C,
            LCtrl = 0x1D,

            A = 0x1E, 
            S = 0x1F, 
            D = 0x20, 
            F = 0x21, 
            G = 0x22,
            H = 0x23, 
            J = 0x24, 
            K = 0x25, 
            L = 0x26,

            Semicolon = 0x27, 
            Apostrophe = 0x28, 
            Grave = 0x29,
            LShift = 0x2A, 
            Backslash = 0x2B,

            Z = 0x2C, 
            X = 0x2D, 
            C = 0x2E, 
            V = 0x2F,
            B = 0x30, 
            N = 0x31, 
            M = 0x32,

            Comma = 0x33, 
            Dot = 0x34, 
            Slash = 0x35,
            RShift = 0x36, 
            NumpadStar = 0x37,
            LAlt = 0x38, 
            Space = 0x39, 
            CapsLock = 0x3A,

            F1 = 0x3B, 
            F2 = 0x3C, 
            F3 = 0x3D, 
            F4 = 0x3E, 
            F5 = 0x3F,
            F6 = 0x40, 
            F7 = 0x41, 
            F8 = 0x42, 
            F9 = 0x43, 
            F10 = 0x44,

            NumLock = 0x45, 
            ScrollLock = 0x46,

            Numpad7 = 0x47, 
            Numpad8 = 0x48, 
            Numpad9 = 0x49, 
            NumpadMinus = 0x4A,
            Numpad4 = 0x4B, 
            Numpad5 = 0x4C, 
            Numpad6 = 0x4D, 
            NumpadPlus = 0x4E,
            Numpad1 = 0x4F, 
            Numpad2 = 0x50, 
            Numpad3 = 0x51,
            Numpad0 = 0x52, 
            NumpadDot = 0x53,

            F11 = 0x57, 
            F12 = 0x58,

            Up = 0x48, 
            Down = 0x50, 
            Left = 0x4B, 
            Right = 0x4D,

            Home = 0x47, 
            End = 0x4F, 
            PageUp = 0x49, 
            PageDown = 0x51,

            Insert = 0x52, 
            Delete = 0x53,

            RCtrl = 0x1D, 
            RAlt = 0x38
        }
    }
}
