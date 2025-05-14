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
                if (Enums.SpecialKeys.TryGetValue(c, out var sk))
                {
                    if (sk.shift)
                        EasyPressKey.PressKeys(false, KeyCode.LShift, sk.code);
                    else
                        EasyPressKey.PressKeys(false, sk.code);
                }
                else if (char.IsLetter(c))
                {
                    var code = (KeyCode)Enum.Parse(typeof(KeyCode), char.ToUpper(c).ToString());
                    if (char.IsUpper(c))
                        EasyPressKey.PressKeys(false, KeyCode.LShift, code);
                    else
                        EasyPressKey.PressKeys(false, code);
                }
                else if (char.IsDigit(c))
                {
                    var code = (KeyCode)Enum.Parse(typeof(KeyCode), "D" + c);
                    EasyPressKey.PressKeys(false, code);
                }

                Thread.Sleep(delayBetweenChars);
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
        public static KeyCode CharToKeyCode(char c)
        {
            if (char.IsLetter(c))
                if (Enum.TryParse(c.ToString().ToUpper(), out KeyCode letterCode))
                    return letterCode;

            if (char.IsDigit(c))
                if (Enum.TryParse("D" + c, out KeyCode digitCode))
                    return digitCode;

            switch (c)
            {
                case ' ': return KeyCode.Space;
                case '.': return KeyCode.Dot;
                case ',': return KeyCode.Comma;
                case '\n': return KeyCode.Enter;
                case '\t': return KeyCode.Tab;
                default: return KeyCode.None;
            }
        }
    }
}