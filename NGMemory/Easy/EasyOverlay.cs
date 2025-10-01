using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static NGMemory.User32;
using static NGMemory.Constants;
using NGMemory.WinInteropTools; // Add this import for WindowStyleHelper

namespace NGMemory.Easy
{
    /// <summary>
    /// Provides easy-to-use methods for creating and managing window overlays.
    /// </summary>
    public class EasyOverlay : Form
    {
        private IntPtr targetWindow;
        private Timer positionTimer;
        private bool autoPosition = true;
        private Point offset = new Point(10, 10);
        private OverlayPosition position = OverlayPosition.BottomRight;

        /// <summary>
        /// Gets the target window handle this overlay is attached to.
        /// </summary>
        public IntPtr TargetWindow => targetWindow;

        /// <summary>
        /// Creates a new overlay for the specified target window.
        /// </summary>
        public EasyOverlay(IntPtr targetWindowHandle)
        {
            // Set basic form properties
            targetWindow = targetWindowHandle;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopLevel = true;
            StartPosition = FormStartPosition.Manual;

            // Set up transparent background
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            // Set up events
            Load += EasyOverlay_Load;
            FormClosed += EasyOverlay_FormClosed;
        }

        /// <summary>
        /// Sets up the positioning timer and applies window styles.
        /// </summary>
        private void EasyOverlay_Load(object sender, EventArgs e)
        {
            if (!IsWindow(targetWindow))
            {
                Close();
                return;
            }

            // Set the overlay as a child of the target window
            WindowStyleHelper.SetParent(Handle, targetWindow);

            // Modify window styles
            int style = WindowStyleHelper.GetWindowLong(Handle, GWL_STYLE);
            style &= ~WS_POPUP;
            style |= WS_CHILD;
            WindowStyleHelper.SetWindowLong(Handle, GWL_STYLE, style);

            // Initial position update
            UpdatePosition();

            // Set up timer for continuous position updates
            if (autoPosition)
            {
                positionTimer = new Timer { Interval = 300 };
                positionTimer.Tick += (s, ev) =>
                {
                    if (!IsWindow(targetWindow))
                    {
                        positionTimer.Stop();
                        Close();
                        return;
                    }
                    UpdatePosition();
                };
                positionTimer.Start();
            }

            BringWindowToTop(Handle);
        }

        /// <summary>
        /// Cleans up resources when the form is closed.
        /// </summary>
        private void EasyOverlay_FormClosed(object sender, FormClosedEventArgs e)
        {
            positionTimer?.Stop();
            positionTimer?.Dispose();
        }

        /// <summary>
        /// Updates the overlay's position according to its settings.
        /// </summary>
        public void UpdatePosition()
        {
            if (!GetClientRect(targetWindow, out RECT clientRect))
                return;

            int x = 0, y = 0;

            // Calculate position based on specified placement
            switch (position)
            {
                case OverlayPosition.TopLeft:
                    x = clientRect.Left + offset.X;
                    y = clientRect.Top + offset.Y;
                    break;
                case OverlayPosition.TopRight:
                    x = clientRect.Right - Width - offset.X;
                    y = clientRect.Top + offset.Y;
                    break;
                case OverlayPosition.BottomLeft:
                    x = clientRect.Left + offset.X;
                    y = clientRect.Bottom - Height - offset.Y;
                    break;
                case OverlayPosition.BottomRight:
                    x = clientRect.Right - Width - offset.X;
                    y = clientRect.Bottom - Height - offset.Y;
                    break;
                case OverlayPosition.Center:
                    x = clientRect.Left + (clientRect.Right - clientRect.Left - Width) / 2;
                    y = clientRect.Top + (clientRect.Bottom - clientRect.Top - Height) / 2;
                    break;
            }

            // Set the window position
            WindowStyleHelper.SetWindowPos(Handle, IntPtr.Zero, x, y, Width, Height,
                WindowStyleHelper.SWP_NOZORDER | WindowStyleHelper.SWP_NOACTIVATE);

            // Ensure the window is visible
            ShowWindow(Handle, SW_SHOW);

            // Force a redraw
            RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero,
                RDW_INVALIDATE | RDW_UPDATENOW | RDW_FRAME | RDW_ALLCHILDREN);
        }

        /// <summary>
        /// Sets the offset from the edge of the target window.
        /// </summary>
        public EasyOverlay SetOffset(int x, int y)
        {
            offset = new Point(x, y);
            if (IsHandleCreated)
                UpdatePosition();
            return this;
        }

        /// <summary>
        /// Sets the position of the overlay relative to the target window.
        /// </summary>
        public EasyOverlay SetPosition(OverlayPosition newPosition)
        {
            position = newPosition;
            if (IsHandleCreated)
                UpdatePosition();
            return this;
        }

        /// <summary>
        /// Sets whether the overlay should automatically update its position.
        /// </summary>
        public EasyOverlay SetAutoPosition(bool enabled, int interval = 300)
        {
            autoPosition = enabled;

            if (positionTimer != null)
            {
                positionTimer.Stop();
                if (enabled)
                {
                    positionTimer.Interval = interval;
                    positionTimer.Start();
                }
            }
            return this;
        }

        #region Native Methods
        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        // Constants
        const uint RDW_INVALIDATE = 0x0001;
        const uint RDW_UPDATENOW = 0x0100;
        const uint RDW_FRAME = 0x0400;
        const uint RDW_ALLCHILDREN = 0x0080;
        #endregion
    }

    /// <summary>
    /// Defines the possible positioning options for an overlay.
    /// </summary>
    public enum OverlayPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
}