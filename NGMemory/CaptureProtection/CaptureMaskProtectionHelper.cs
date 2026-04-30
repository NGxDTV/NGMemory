using System;
using NGMemory.Easy;

namespace NGMemory.CaptureProtection
{
    public static class CaptureMaskProtectionHelper
    {
        public static WindowCaptureProtectionResult Apply(IntPtr hwnd, bool protect, CaptureMaskEffect effect)
        {
            if (!protect)
            {
                return EasyWindow.SetDisplayAffinity(hwnd, WindowDisplayAffinity.None);
            }

            // WDA_EXCLUDEFROMCAPTURE removes only the protected window from
            // capture. It does not blur other applications behind it. For a black
            // screenshot-only mask, WDA_MONITOR is the useful test mode because
            // many capture stacks replace the window with black.
            if (effect == CaptureMaskEffect.Black)
            {
                return EasyWindow.SetDisplayAffinity(hwnd, WindowDisplayAffinity.Monitor);
            }

            // Placeholder text and simulated blur must be real visible pixels,
            // otherwise a screenshot cannot capture them. They are intentionally
            // unprotected demo modes, not screenshot-only effects.
            return EasyWindow.SetDisplayAffinity(hwnd, WindowDisplayAffinity.None);
        }
    }
}
