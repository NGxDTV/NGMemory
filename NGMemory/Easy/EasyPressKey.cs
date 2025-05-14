using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static NGMemory.Enums;

namespace NGMemory.Easy
{
    /// <summary>
    /// Stellt einfache Methoden zum Senden von Tastatur-Eingaben bereit.
    /// </summary>
    /// <remarks>
    /// Diese Klasse bietet eine vereinfachte API für die Tastatursteuerung 
    /// in anderen Anwendungen durch Scancode-Tastatureingaben.
    /// </remarks>
    public class EasyPressKey
    {
        /// <summary>
        /// Drückt eine Reihe von Tasten nacheinander (erst alle runter, dann alle hoch).
        /// </summary>
        /// <param name="async">True für asynchrone Ausführung, False für synchrone Ausführung</param>
        /// <param name="scanCodes">Die Scancodes der zu drückenden Tasten</param>
        /// <example>
        /// <code>
        /// // Drückt Alt+Tab
        /// EasyPressKey.PressKeys(false, KeyCode.LAlt, KeyCode.Tab);
        /// </code>
        /// </example>
        public static void PressKeys(bool async, params KeyCode[] scanCodes)
        {
            WinInteropTools.InputHelper.PressKeys(async, scanCodes);
        }

        /// <summary>
        /// Drückt eine Reihe von Tasten nacheinander und wartet auf Fertigstellung.
        /// </summary>
        /// <param name="scanCodes">Die Scancodes der zu drückenden Tasten</param>
        public static void PressKeys(params KeyCode[] scanCodes)
        {
            PressKeys(false, scanCodes);
        }

        /// <summary>
        /// Drückt eine Tastenkombination asynchron, ohne auf Fertigstellung zu warten.
        /// </summary>
        /// <param name="scanCodes">Die Scancodes der zu drückenden Tasten</param>
        /// <returns>Ein Task, der die asynchrone Operation repräsentiert</returns>
        public static Task PressKeysAsync(params KeyCode[] scanCodes)
        {
            PressKeys(true, scanCodes);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sendet einzelne Tastendrücke mit individueller Verzögerung zwischen den Tasten.
        /// </summary>
        /// <param name="delayBetweenKeysMs">Verzögerung zwischen Tastendrücken in Millisekunden</param>
        /// <param name="scanCodes">Die Scancodes der zu drückenden Tasten</param>
        public static void PressKeysWithDelay(int delayBetweenKeysMs, params KeyCode[] scanCodes)
        {
            WinInteropTools.InputHelper.PressKeysWithDelay(delayBetweenKeysMs, scanCodes);
        }
    }
}
