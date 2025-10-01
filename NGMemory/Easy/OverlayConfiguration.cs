using System;
using System.Drawing;
using System.Windows.Forms;

namespace NGMemory.Easy
{
    /// <summary>
    /// Provides a fluent API for configuring overlays.
    /// </summary>
    public class OverlayConfiguration
    {
        private EasyOverlay overlay;

        /// <summary>
        /// Creates a new configuration for the specified overlay.
        /// </summary>
        public OverlayConfiguration(EasyOverlay overlay)
        {
            this.overlay = overlay;
        }

        /// <summary>
        /// Sets the size of the overlay.
        /// </summary>
        public OverlayConfiguration WithSize(int width, int height)
        {
            overlay.Width = width;
            overlay.Height = height;
            return this;
        }

        /// <summary>
        /// Sets the position of the overlay.
        /// </summary>
        public OverlayConfiguration WithPosition(OverlayPosition position)
        {
            overlay.SetPosition(position);
            return this;
        }

        /// <summary>
        /// Sets the offset from the edge of the parent window.
        /// </summary>
        public OverlayConfiguration WithOffset(int x, int y)
        {
            overlay.SetOffset(x, y);
            return this;
        }

        /// <summary>
        /// Sets the background color of the overlay.
        /// </summary>
        public OverlayConfiguration WithBackgroundColor(Color color)
        {
            overlay.BackColor = color;
            return this;
        }

        /// <summary>
        /// Adds a label to the overlay.
        /// </summary>
        public OverlayConfiguration WithLabel(string text, int x, int y, Color? foreColor = null, Font font = null)
        {
            Label lbl = new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent
            };

            if (foreColor.HasValue)
                lbl.ForeColor = foreColor.Value;
            
            if (font != null)
                lbl.Font = font;
            else
                lbl.Font = new Font("Segoe UI", 9f);

            overlay.Controls.Add(lbl);
            return this;
        }

        /// <summary>
        /// Adds a button to the overlay.
        /// </summary>
        public OverlayConfiguration WithButton(string text, int x, int y, int width, int height, 
            EventHandler onClick, Color? backColor = null, Color? foreColor = null)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat
            };

            if (backColor.HasValue)
                btn.BackColor = backColor.Value;
            
            if (foreColor.HasValue)
                btn.ForeColor = foreColor.Value;
            
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            
            overlay.Controls.Add(btn);
            return this;
        }

        /// <summary>
        /// Sets whether the overlay should update its position automatically.
        /// </summary>
        public OverlayConfiguration WithAutoPosition(bool enabled, int interval = 300)
        {
            overlay.SetAutoPosition(enabled, interval);
            return this;
        }

        /// <summary>
        /// Adds a custom control to the overlay.
        /// </summary>
        public OverlayConfiguration WithControl(Control control)
        {
            overlay.Controls.Add(control);
            return this;
        }

        /// <summary>
        /// Applies a custom action to the overlay.
        /// </summary>
        public OverlayConfiguration WithCustomization(Action<EasyOverlay> customize)
        {
            customize(overlay);
            return this;
        }
    }

    /// <summary>
    /// Extension methods for EasyOverlay.
    /// </summary>
    public static class EasyOverlayExtensions
    {
        /// <summary>
        /// Creates a configuration builder for the overlay.
        /// </summary>
        public static OverlayConfiguration Configure(this EasyOverlay overlay)
        {
            return new OverlayConfiguration(overlay);
        }
    }
}
