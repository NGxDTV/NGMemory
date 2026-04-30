using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NGMemory.Easy
{
    /// <summary>
    /// Einfacher Helfer für das Arbeiten mit Fenstern und Unterfenstern.
    /// </summary>
    public static class EasyWindow
    {
        /// <summary>
        /// Sucht ein Hauptfenster anhand des Prozessnamens (z.B. "twe") und optional eines Teil-Titels.
        /// Gibt IntPtr.Zero zurück, wenn nichts gefunden wird.
        /// </summary>
        public static IntPtr GetMainWindow(string processName, string partialTitle = null)
        {
            Process[] procs = Process.GetProcessesByName(processName);
            if (procs.Length == 0) return IntPtr.Zero;

            if (!string.IsNullOrEmpty(partialTitle))
            {
                foreach (var proc in procs)
                {
                    var handles = WinInteropTools.GuiInteropHandler.EnumerateProcessWindowHandles(proc);
                    foreach (var hWnd in handles)
                    {
                        string title = WinInteropTools.GuiInteropHandler.GetWindowTitle(hWnd);
                        if (title.Contains(partialTitle))
                            return hWnd;
                    }
                }

                return IntPtr.Zero;
            }

            foreach (var proc in procs)
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                    return proc.MainWindowHandle;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Liefert alle Kindfenster (Child-Handles) eines Fensters zurück.
        /// </summary>
        public static List<IntPtr> GetAllChildWindows(IntPtr parentHandle)
        {
            return WinInteropTools.GuiInteropHandler.getChildList(parentHandle);
        }

        /// <summary>
        /// Sucht in allen Kindfenstern nach einem Titel, der den Suchtext enthält.
        /// Gibt IntPtr.Zero zurück, wenn nichts gefunden wird.
        /// </summary>
        public static IntPtr GetChildByTitle(IntPtr parentHandle, string partialTitle)
        {
            var children = GetAllChildWindows(parentHandle);
            foreach (var child in children)
            {
                string title = WinInteropTools.GuiInteropHandler.GetWindowTitle(child);
                if (title.Contains(partialTitle))
                    return child;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Bringt das angegebene Fenster in den Vordergrund.
        /// </summary>
        public static void FocusWindow(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;
            NGMemory.User32.ShowWindow(windowHandle, 9); // SW_RESTORE
            NGMemory.User32.SetForegroundWindow(windowHandle);
        }

        /// <summary>
        /// Ermittelt ein Fenster eines Prozesses (per Name) und fokussiert es optional.
        /// </summary>
        public static IntPtr FindAndFocus(string processName, string partialTitle = null)
        {
            IntPtr hWnd = GetMainWindow(processName, partialTitle);
            if (hWnd != IntPtr.Zero) FocusWindow(hWnd);
            return hWnd;
        }

        /// <summary>
        /// Ermittelt ein Fenster eines Prozesses (per Name).
        /// </summary>
        public static IntPtr Find(string processName, string partialTitle = null)
        {
            IntPtr hWnd = GetMainWindow(processName, partialTitle);
            return hWnd;
        }

        /// <summary>
        /// Aktiviert oder deaktiviert Windows Screen-Capture-Schutz fuer ein
        /// Top-Level-Fenster. Einzelne normale Controls koennen damit nicht
        /// geschuetzt werden; dafuer sind separate Top-Level-Overlay-Fenster noetig.
        /// </summary>
        public static WindowCaptureProtectionResult SetCaptureProtection(IntPtr windowHandle, bool protect)
        {
            return WindowCaptureProtection.SetProtection(windowHandle, protect);
        }

        /// <summary>
        /// Setzt eine konkrete Window-Display-Affinity auf einem Top-Level-Fenster.
        /// WDA_EXCLUDEFROMCAPTURE wird ab Windows 10 Version 2004 korrekt unterstuetzt.
        /// </summary>
        public static WindowCaptureProtectionResult SetDisplayAffinity(
            IntPtr windowHandle,
            WindowDisplayAffinity affinity)
        {
            return WindowCaptureProtection.SetAffinity(windowHandle, affinity);
        }

        /// <summary>
        /// Entfernt den Windows Screen-Capture-Schutz von einem Top-Level-Fenster.
        /// </summary>
        public static WindowCaptureProtectionResult ClearCaptureProtection(IntPtr windowHandle)
        {
            return WindowCaptureProtection.Clear(windowHandle);
        }
    }
}
