using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NGMemory.Easy
{
    /// <summary>
    /// Schnelle Hilfsfunktionen für GUI-Interop.
    /// </summary>
    public static class EasyGuiInterop
    {
        /// <summary>
        /// Setzt den Text in ein Steuerelement (z.B. TextBox) innerhalb eines Fensters.
        /// </summary>
        public static void SetText(IntPtr dialogHandle, int controlId, string text)
        {
            WinInteropTools.GuiInteropHandler.InteropSetText(dialogHandle, controlId, text);
        }

        /// <summary>
        /// Liefert den Fenstertitel zu einem Handle zurück.
        /// </summary>
        public static string GetWindowTitle(IntPtr hWnd)
        {
            return WinInteropTools.GuiInteropHandler.GetWindowTitle(hWnd);
        }

        /// <summary>
        /// Sucht in mehreren Fenstern nach einer bestimmten Klassenname und gibt das passende Handle zurück.
        /// </summary>
        public static IntPtr GetWindowByClassName(IEnumerable<IntPtr> windows, string className)
        {
            return WinInteropTools.GuiInteropHandler.GetWindowByClassName(windows, className);
        }

        /// <summary>
        /// Sucht in einem Fenster mehrmals nach einer bestimmten Klassenname und gibt das passende Handle zurück.
        /// </summary>
        public static IntPtr GetWindowByClassName(IntPtr window, string className)
        {
            return WinInteropTools.GuiInteropHandler.GetWindowByClassName(window, className);
        }

        /// <summary>
        /// Ermittelt alle Fensterhandles eines bestimmten Prozesses.
        /// </summary>
        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(Process process)
        {
            return WinInteropTools.GuiInteropHandler.EnumerateProcessWindowHandles(process);
        }

        /// <summary>
        /// Holt das Steuerelement-Handle zu einer Dialog- und Control-ID als HandleRef.
        /// </summary>
        public static IntPtr GetControlHandle(IntPtr hWnd, int controlId)
        {
            return WinInteropTools.GuiInteropHandler.getRef(hWnd, controlId).Handle;
        }

        /// <summary>
        /// Sucht nach einem Fenster im angegebenen Fenster-Set, das einen bestimmten Titel enthält.
        /// </summary>
        public static IntPtr GetWindowByContainsName(IEnumerable<IntPtr> handles, string titlePart)
        {
            return WinInteropTools.GuiInteropHandler.getList(handles, titlePart);
        }

        /// <summary>
        /// Gibt eine Liste aller Fenster mit zugehörigen Titeln zurück.
        /// </summary>
        public static List<WinInteropTools.GuiInteropHandler.ListItemData> GetAllWindows(IEnumerable<IntPtr> handles)
        {
            return WinInteropTools.GuiInteropHandler.getAllList(handles);
        }

        /// <summary>
        /// Bringt das Fenster des angegebenen Prozesses in den Vordergrund.
        /// </summary>
        public static void ShowProcessWindow(Process[] processes)
        {
            WinInteropTools.GuiInteropHandler.showProcessWindow(processes);
        }

        /// <summary>
        /// Liefert das Handle aus einer Liste über den Index.
        /// </summary>
        public static IntPtr GetHandleByIndex(int index, List<IntPtr> handleList)
        {
            return WinInteropTools.GuiInteropHandler.GetHandleByIndex(index, handleList);
        }

        /// <summary>
        /// Liefert alle Unterfenster (Child) des angegebenen Fensters.
        /// </summary>
        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            return WinInteropTools.GuiInteropHandler.getChildList(parent);
        }

        /// <summary>
        /// Sucht per Fenstername (enthält) in einem Prozessarray und gibt das Handle zurück.
        /// </summary>
        public static IntPtr GetWindowByContainsName(Process[] processList, string windowName)
        {
            return WinInteropTools.GuiInteropHandler.getWindowByContainsName(processList, windowName);
        }
    }
}
