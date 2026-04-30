using System;
using System.Runtime.InteropServices;

namespace NGMemory.Easy
{
    /// <summary>
    /// Display affinity values for SetWindowDisplayAffinity.
    /// </summary>
    public enum WindowDisplayAffinity : uint
    {
        /// <summary>
        /// No capture protection.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Shows the window only on a physical monitor.
        /// </summary>
        Monitor = 0x00000001,

        /// <summary>
        /// Excludes the window from capture. Correctly supported starting with
        /// Windows 10 version 2004.
        /// </summary>
        ExcludeFromCapture = 0x00000011
    }

    /// <summary>
    /// Result returned after applying display affinity to a window.
    /// </summary>
    public sealed class WindowCaptureProtectionResult
    {
        public WindowCaptureProtectionResult(
            IntPtr windowHandle,
            WindowDisplayAffinity affinity,
            bool returnValue,
            int win32Error)
        {
            WindowHandle = windowHandle;
            Affinity = affinity;
            ReturnValue = returnValue;
            Win32Error = win32Error;
        }

        /// <summary>
        /// The HWND that was passed to SetWindowDisplayAffinity.
        /// </summary>
        public IntPtr WindowHandle { get; }

        /// <summary>
        /// The requested display affinity.
        /// </summary>
        public WindowDisplayAffinity Affinity { get; }

        /// <summary>
        /// Native return value of SetWindowDisplayAffinity.
        /// </summary>
        public bool ReturnValue { get; }

        /// <summary>
        /// Last Win32 error when ReturnValue is false, otherwise 0.
        /// </summary>
        public int Win32Error { get; }

        /// <summary>
        /// True when an affinity other than None was requested.
        /// </summary>
        public bool IsProtectionActive
        {
            get { return Affinity != WindowDisplayAffinity.None; }
        }

        public override string ToString()
        {
            return string.Format(
                "Protection: {0} | Affinity: {1} | SetWindowDisplayAffinity: {2} | Win32Error: {3}",
                IsProtectionActive ? "active" : "inactive",
                Affinity,
                ReturnValue ? "TRUE" : "FALSE",
                Win32Error);
        }
    }

    /// <summary>
    /// Helper for Windows screen-capture protection based on SetWindowDisplayAffinity.
    /// </summary>
    public static class WindowCaptureProtection
    {
        /// <summary>
        /// Applies or removes WDA_EXCLUDEFROMCAPTURE on a top-level window.
        /// </summary>
        /// <remarks>
        /// SetWindowDisplayAffinity only works for top-level windows owned by the
        /// current process. Normal controls such as labels, buttons, panels, WPF
        /// elements, or WinForms child controls cannot be protected individually
        /// because they are not separate top-level windows in the capture pipeline.
        /// Use separate borderless top-level overlay windows when only selected
        /// regions should be hidden from screenshots or streams.
        ///
        /// WDA_EXCLUDEFROMCAPTURE is correctly supported starting with Windows 10
        /// version 2004. Older Windows builds can behave like WDA_MONITOR or fail.
        /// </remarks>
        public static WindowCaptureProtectionResult SetProtection(IntPtr windowHandle, bool protect)
        {
            return SetAffinity(
                windowHandle,
                protect ? WindowDisplayAffinity.ExcludeFromCapture : WindowDisplayAffinity.None);
        }

        /// <summary>
        /// Applies a specific display affinity value to a top-level window.
        /// </summary>
        public static WindowCaptureProtectionResult SetAffinity(
            IntPtr windowHandle,
            WindowDisplayAffinity affinity)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return new WindowCaptureProtectionResult(windowHandle, affinity, false, 6);
            }

            bool ok = User32.SetWindowDisplayAffinity(windowHandle, (uint)affinity);
            int error = ok ? 0 : Marshal.GetLastWin32Error();
            return new WindowCaptureProtectionResult(windowHandle, affinity, ok, error);
        }

        /// <summary>
        /// Removes display affinity from a top-level window.
        /// </summary>
        public static WindowCaptureProtectionResult Clear(IntPtr windowHandle)
        {
            return SetAffinity(windowHandle, WindowDisplayAffinity.None);
        }
    }
}
