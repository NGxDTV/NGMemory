using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NGMemory.Easy;
using System.Runtime.InteropServices;

using static NGMemory.User32;

namespace NGMemory.Overlay
{
    /// <summary>
    /// Types of target windows to search for when creating overlays.
    /// </summary>
    public enum TargetWindowType
    {
        MDIClient = 0,
        DialogWindows,
        NormalWindows,
        All
    }

    /// <summary>
    /// Manages multiple overlays and provides methods to find and create overlays for specific windows.
    /// </summary>
    public class OverlayManager
    {
        private readonly List<EasyOverlay> overlays = new List<EasyOverlay>();

        /// <summary>
        /// Gets all active overlays.
        /// </summary>
        public List<EasyOverlay> Overlays => overlays;

        /// <summary>
        /// Creates and shows a new overlay for the specified target window.
        /// </summary>
        /// <param name="targetWindow">Handle to the target window</param>
        /// <param name="configure">Optional action to configure the overlay</param>
        /// <returns>The created overlay</returns>
        public EasyOverlay CreateOverlay(IntPtr targetWindow, Action<EasyOverlay> configure = null)
        {
            // Check if window is valid
            if (targetWindow == IntPtr.Zero || !IsWindow(targetWindow))
                return null;

            // Check if overlay already exists
            for (int i = overlays.Count - 1; i >= 0; i--)
            {
                var existing = overlays[i];
                if (existing == null || existing.IsDisposed)
                {
                    overlays.RemoveAt(i);
                    continue;
                }

                if (existing.TargetWindow == targetWindow)
                    return existing;
            }

            // Create new overlay
            var overlay = new EasyOverlay(targetWindow);
            
            // Apply custom configuration if provided
            configure?.Invoke(overlay);
            
            // Add to managed collection
            overlays.Add(overlay);
            overlay.FormClosed += (s, e) => overlays.Remove(overlay);
            
            // Show the overlay
            overlay.Show();
            
            return overlay;
        }

        /// <summary>
        /// Creates an overlay for a window found by process name and window title.
        /// </summary>
        /// <param name="processName">Name of the process (without .exe)</param>
        /// <param name="windowTitleFilter">Text to search for in window titles</param>
        /// <param name="configure">Optional action to configure the overlay</param>
        /// <returns>The created overlay, or null if window not found</returns>
        public EasyOverlay CreateOverlayForWindow(string processName, string windowTitleFilter, Action<EasyOverlay> configure = null)
        {
            var targetWindow = EasyWindow.GetMainWindow(processName, windowTitleFilter);
            if (targetWindow != IntPtr.Zero)
                return CreateOverlay(targetWindow, configure);
            return null;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        // For enumerating top-level windows
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Creates overlays for windows matching the process name and title filter.
        /// You can choose which window types to consider using the optional windowType parameter.
        /// Default is TargetWindowType.MDIClient to preserve previous behavior.
        /// </summary>
        /// <param name="processName">Name of the process (without .exe)</param>
        /// <param name="windowTitleFilter">Text to search for in window titles</param>
        /// <param name="configure">Optional action to configure each overlay</param>
        /// <param name="windowType">Which types of windows to consider (optional, default MDIClient)</param>
        /// <returns>Count of overlays created</returns>
        public int CreateOverlaysForAllMatching(string processName, string windowTitleFilter, Action<EasyOverlay> configure = null, TargetWindowType windowType = TargetWindowType.MDIClient)
        {
            int count = 0;
            var processes = Process.GetProcessesByName(processName);
            
            foreach (var process in processes)
            {
                // Get main window handle (may be zero for simple dialogs)
                IntPtr mainWindow = process.MainWindowHandle;

                IntPtr mdiClient = IntPtr.Zero;

                // If looking for MDIClient windows (or All/DialogWindows includes MDI children) and we have a main window
                if (mainWindow != IntPtr.Zero && (windowType == TargetWindowType.MDIClient || windowType == TargetWindowType.All || windowType == TargetWindowType.DialogWindows))
                {
                    mdiClient = FindWindowEx(mainWindow, IntPtr.Zero, "MDIClient", null);
                    if (mdiClient != IntPtr.Zero)
                    {
                        // Enumerate child windows inside the MDI client
                        EnumChildWindows(mdiClient, (childHwnd, lParam) =>
                        {
                            int textLength = GetWindowTextLength(childHwnd);
                            var buffer = new StringBuilder(textLength + 1);
                            GetWindowText(childHwnd, buffer, buffer.Capacity);
                            string title = buffer.ToString();

                            if (string.IsNullOrEmpty(windowTitleFilter) || title.Contains(windowTitleFilter))
                            {
                                if (windowType == TargetWindowType.MDIClient || windowType == TargetWindowType.All)
                                {
                                    if (CreateOverlayIfMissing(childHwnd, configure))
                                        count++;
                                }
                                else if (windowType == TargetWindowType.DialogWindows)
                                {
                                    var classNameBuf = new StringBuilder(256);
                                    if (GetClassName(childHwnd, classNameBuf, classNameBuf.Capacity) > 0)
                                    {
                                        string className = classNameBuf.ToString();
                                        if (className == "#32770") // standard dialog class
                                        {
                                            if (CreateOverlayIfMissing(childHwnd, configure))
                                                count++;
                                        }
                                    }
                                }
                            }
                            return true;
                        }, IntPtr.Zero);
                    }
                }

                // Enumerate top-level windows for this process (covers dialogs and windows even if MainWindowHandle == 0)
                List<IntPtr> topLevelWindows = new List<IntPtr>();
                EnumWindows((hWnd, lParam) =>
                {
                    uint pid;
                    GetWindowThreadProcessId(hWnd, out pid);
                    if (pid == (uint)process.Id)
                        topLevelWindows.Add(hWnd);
                    return true;
                }, IntPtr.Zero);

                foreach (var hWnd in topLevelWindows)
                {
                    // Skip MDI client itself (already processed)
                    if (hWnd == mdiClient)
                        continue;

                    // Get window text
                    int textLength = GetWindowTextLength(hWnd);
                    var buffer = new StringBuilder(textLength + 1);
                    GetWindowText(hWnd, buffer, buffer.Capacity);
                    string title = buffer.ToString();

                    // Get class name
                    var classNameBuf = new StringBuilder(256);
                    GetClassName(hWnd, classNameBuf, classNameBuf.Capacity);
                    string className = classNameBuf.ToString();

                    if (!string.IsNullOrEmpty(windowTitleFilter) && !title.Contains(windowTitleFilter))
                        continue;

                    // Decide based on windowType
                    if (windowType == TargetWindowType.All)
                    {
                        if (CreateOverlayIfMissing(hWnd, configure))
                            count++;
                    }
                    else if (windowType == TargetWindowType.DialogWindows)
                    {
                        if (className == "#32770")
                        {
                            if (CreateOverlayIfMissing(hWnd, configure))
                                count++;
                        }
                    }
                    else if (windowType == TargetWindowType.NormalWindows)
                    {
                        // Treat normal as top-level windows that are not dialogs
                        if (className != "#32770")
                        {
                            if (CreateOverlayIfMissing(hWnd, configure))
                                count++;
                        }
                    }
                    else if (windowType == TargetWindowType.MDIClient)
                    {
                        // If user explicitly asked for MDIClient but process has top-level windows, handle main window here
                        if (hWnd == mainWindow && mainWindow != IntPtr.Zero)
                        {
                            if (CreateOverlayIfMissing(mainWindow, configure))
                                count++;
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Removes and disposes an overlay for the specified window.
        /// </summary>
        public bool RemoveOverlay(IntPtr targetWindow)
        {
            for (int i = overlays.Count - 1; i >= 0; i--)
            {
                if (overlays[i].TargetWindow == targetWindow)
                {
                    var overlay = overlays[i];
                    overlays.RemoveAt(i);
                    overlay.Close();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes and disposes all overlays.
        /// </summary>
        public void RemoveAllOverlays()
        {
            var snapshot = overlays.ToArray();
            overlays.Clear();

            foreach (var overlay in snapshot)
            {
                if (overlay == null || overlay.IsDisposed)
                    continue;

                overlay.Close();
            }
        }

        /// <summary>
        /// Starts a timer to scan for new windows matching the criteria.
        /// </summary>
        /// <param name="processName">Name of the process (without .exe)</param>
        /// <param name="windowTitleFilter">Text to search for in window titles</param>
        /// <param name="interval">Scan interval in milliseconds</param>
        /// <param name="configure">Optional action to configure each overlay</param>
        /// <param name="windowType">Which types of windows to consider (optional, default MDIClient)</param>
        /// <returns>The timer controlling the scanning</returns>
        public System.Windows.Forms.Timer StartWindowScan(
            string processName, 
            string windowTitleFilter,
            int interval = 1000, 
            Action<EasyOverlay> configure = null,
            TargetWindowType windowType = TargetWindowType.MDIClient)
        {
            CreateOverlaysForAllMatching(processName, windowTitleFilter, configure, windowType);
            var timer = new System.Windows.Forms.Timer { Interval = interval };
            timer.Tick += (s, e) => CreateOverlaysForAllMatching(processName, windowTitleFilter, configure, windowType);
            timer.Start();
            return timer;
        }

        private bool CreateOverlayIfMissing(IntPtr targetWindow, Action<EasyOverlay> configure)
        {
            int before = overlays.Count;
            return CreateOverlay(targetWindow, configure) != null && overlays.Count > before;
        }
    }
}
