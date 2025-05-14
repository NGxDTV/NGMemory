using System;
using System.Text;
using System.Threading;
using static NGMemory.Enums;

namespace NGMemory.Easy
{
    /// <summary>
    /// Erweiterte Tastatureingabe mit Texterkennung.
    /// </summary>
    public static class EasyKeyboard
    {
        /// <summary>
        /// Sendet einen Text als Tastendrücke an die aktive Anwendung.
        /// </summary>
        public static void TypeText(string text, int delayBetweenChars = 10)
        {
            if (string.IsNullOrEmpty(text)) return;

            foreach (char c in text)
            {
                KeyCode keyCode = CharToKeyCode(c);
                if (keyCode != 0)
                {
                    // Großbuchstaben mit Shift
                    if (char.IsUpper(c))
                    {
                        EasyPressKey.PressKeys(false, KeyCode.LShift, keyCode);
                        Thread.Sleep(delayBetweenChars);
                    }
                    else
                    {
                        EasyPressKey.PressKeys(false, keyCode);
                        Thread.Sleep(delayBetweenChars);
                    }
                }
            }
        }

        /// <summary>
        /// Sendet Text asynchron (im Hintergrund).
        /// </summary>
        public static void TypeTextAsync(string text, int delayBetweenChars = 10)
        {
            ThreadPool.QueueUserWorkItem(_ => TypeText(text, delayBetweenChars));
        }

        /// <summary>
        /// Simuliert das Drücken der Strg+C Tastenkombination.
        /// </summary>
        public static void SendCtrlC()
        {
            EasyPressKey.PressKeys(false, KeyCode.LCtrl, KeyCode.C);
        }

        /// <summary>
        /// Simuliert das Drücken der Strg+V Tastenkombination.
        /// </summary>
        public static void SendCtrlV()
        {
            EasyPressKey.PressKeys(false, KeyCode.LCtrl, KeyCode.V);
        }

        /// <summary>
        /// Konvertiert ein Zeichen in den entsprechenden Scancode.
        /// </summary>
        private static KeyCode CharToKeyCode(char c)
        {
            // ASCII-Buchstaben
            if (c >= 'a' && c <= 'z')
                return (KeyCode)((int)KeyCode.A + (c - 'a'));
            
            if (c >= 'A' && c <= 'Z')
                return (KeyCode)((int)KeyCode.A + (c - 'A'));
            
            // Zahlen
            if (c >= '0' && c <= '9')
                return (KeyCode)((int)KeyCode.D0 + (c - '0'));
            
            // Sonderzeichen
            switch (c)
            {
                case ' ': return KeyCode.Space;
                case '.': return KeyCode.Dot;
                case ',': return KeyCode.Comma;
                case '\n': return KeyCode.Enter;
                case '\t': return KeyCode.Tab;
                default: return 0;
            }
        }
    }
}