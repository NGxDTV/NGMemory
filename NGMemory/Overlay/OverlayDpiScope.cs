using System;
using System.Runtime.InteropServices;

namespace NGMemory.Overlay
{
    internal sealed class OverlayDpiScope : IDisposable
    {
        private static readonly IntPtr[] KnownContexts =
        {
            new IntPtr(-4),
            new IntPtr(-5),
            new IntPtr(-3),
            new IntPtr(-2),
            new IntPtr(-1)
        };

        private readonly IntPtr previous;

        private OverlayDpiScope(IntPtr previous)
        {
            this.previous = previous;
        }

        public static OverlayDpiScope EnterContextOf(IntPtr window)
        {
            try
            {
                IntPtr context = FindContextOf(window);

                return new OverlayDpiScope(
                    context == IntPtr.Zero ? IntPtr.Zero : SetThreadDpiAwarenessContext(context));
            }
            catch (EntryPointNotFoundException)
            {
                return new OverlayDpiScope(IntPtr.Zero);
            }
        }

        private static IntPtr FindContextOf(IntPtr window)
        {
            IntPtr windowContext = GetWindowDpiAwarenessContext(window);

            if (windowContext == IntPtr.Zero)
                return IntPtr.Zero;

            foreach (IntPtr known in KnownContexts)
            {
                if (AreDpiAwarenessContextsEqual(windowContext, known))
                    return known;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (previous != IntPtr.Zero)
                SetThreadDpiAwarenessContext(previous);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA, IntPtr dpiContextB);
    }
}
