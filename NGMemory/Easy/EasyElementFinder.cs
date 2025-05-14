using System;
using System.Collections.Generic;
using System.Drawing;

namespace NGMemory.Easy
{
    /// <summary>
    /// Fortgeschrittene Suchfunktionen für UI-Elemente.
    /// </summary>
    public static class EasyElementFinder
    {
        /// <summary>
        /// Findet ein Element basierend auf mehreren Kriterien.
        /// </summary>
        public static IntPtr FindElement(IntPtr parentWindow, string className = null, string windowText = null, int controlId = -1)
        {
            var allChildren = EasyGuiInterop.GetChildWindows(parentWindow);
            
            foreach (var hwnd in allChildren)
            {
                bool matches = true;
                
                if (className != null)
                {
                    string currentClass = EasyGuiInterop.GetClassName(hwnd);
                    if (!currentClass.Equals(className, StringComparison.OrdinalIgnoreCase))
                        matches = false;
                }
                
                if (windowText != null && matches)
                {
                    string currentText = EasyGuiInterop.GetWindowTitle(hwnd);
                    if (!currentText.Contains(windowText))
                        matches = false;
                }
                
                if (controlId != -1 && matches)
                {
                    int id = NGMemory.User32.GetDlgCtrlID(hwnd);
                    if (id != controlId)
                        matches = false;
                }
                
                if (matches)
                    return hwnd;
            }
            
            return IntPtr.Zero;
        }
        
        /// <summary>
        /// Ermittelt die Position eines Fensters im Bildschirmkoordinaten.
        /// </summary>
        public static Rectangle GetWindowRect(IntPtr hwnd)
        {
            NGMemory.User32.RECT rect;
            NGMemory.User32.GetWindowRect(hwnd, out rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
    }
}