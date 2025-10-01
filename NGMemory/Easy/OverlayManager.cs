using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static NGMemory.User32; // Add static import

namespace NGMemory.Easy
{
    /// <summary>
    /// Manages multiple overlays and provides methods to find and create overlays for specific windows.
    /// </summary>
    public class OverlayManager
    {
        private List<EasyOverlay> overlays = new List<EasyOverlay>();

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
            foreach (var existing in overlays)
            {
                if (existing.TargetWindow == targetWindow)
                    return existing;
            }

            // Create new overlay
            var overlay = new EasyOverlay(targetWindow);
            
            // Apply custom configuration if provided
            configure?.Invoke(overlay);
            
            // Add to managed collection
            overlays.Add(overlay);
            
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

        /// <summary>
        /// Creates overlays for all windows matching the process name and title filter.
        /// </summary>
        /// <param name="processName">Name of the process (without .exe)</param>
        /// <param name="windowTitleFilter">Text to search for in window titles</param>
        /// <param name="configure">Optional action to configure each overlay</param>
        /// <returns>Count of overlays created</returns>
        public int CreateOverlaysForAllMatching(string processName, string windowTitleFilter, Action<EasyOverlay> configure = null)
        {
            int count = 0;
            var processes = Process.GetProcessesByName(processName);
            
            foreach (var process in processes)
            {
                // Get main window handle
                IntPtr mainWindow = process.MainWindowHandle;
                if (mainWindow == IntPtr.Zero)
                    continue;

                // Get MDI client if present
                IntPtr mdiClient = FindWindowEx(mainWindow, IntPtr.Zero, "MDIClient", null);
                if (mdiClient != IntPtr.Zero)
                {
                    // Enumerate child windows
                    List<IntPtr> childWindows = new List<IntPtr>();
                    EnumChildWindows(mdiClient, (childHwnd, lParam) =>
                    {
                        int textLength = GetWindowTextLength(childHwnd);
                        if (textLength > 0)
                        {
                            var buffer = new StringBuilder(textLength + 1);
                            GetWindowText(childHwnd, buffer, buffer.Capacity);
                            string title = buffer.ToString();

                            if (string.IsNullOrEmpty(windowTitleFilter) || title.Contains(windowTitleFilter))
                            {
                                // Create overlay for this child window
                                if (CreateOverlay(childHwnd, configure) != null)
                                    count++;
                            }
                        }
                        return true;
                    }, IntPtr.Zero);
                }
                else if (string.IsNullOrEmpty(windowTitleFilter) || 
                        process.MainWindowTitle.Contains(windowTitleFilter))
                {
                    // Just use the main window
                    if (CreateOverlay(mainWindow, configure) != null)
                        count++;
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
            foreach (var overlay in overlays)
            {
                overlay.Close();
            }
            overlays.Clear();
        }

        /// <summary>
        /// Starts a timer to scan for new windows matching the criteria.
        /// </summary>
        /// <param name="processName">Name of the process (without .exe)</param>
        /// <param name="windowTitleFilter">Text to search for in window titles</param>
        /// <param name="interval">Scan interval in milliseconds</param>
        /// <param name="configure">Optional action to configure each overlay</param>
        /// <returns>The timer controlling the scanning</returns>
        public System.Windows.Forms.Timer StartWindowScan(
            string processName, 
            string windowTitleFilter, 
            int interval = 1000, 
            Action<EasyOverlay> configure = null)
        {
            var timer = new System.Windows.Forms.Timer { Interval = interval };
            timer.Tick += (s, e) => CreateOverlaysForAllMatching(processName, windowTitleFilter, configure);
            timer.Start();
            return timer;
        }
    }
}
