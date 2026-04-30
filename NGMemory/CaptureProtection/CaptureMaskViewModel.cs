using System;
using System.Drawing;
using NGMemory.Easy;

namespace NGMemory.CaptureProtection
{
    public sealed class CaptureMaskViewModel
    {
        public event EventHandler Changed;

        private int x = 180;
        private int y = 160;
        private int width = 360;
        private int height = 220;
        private int borderThickness = 3;
        private int opacityPercent = 92;
        private bool protectFromCapture = true;
        private bool borderOnlyForUser = true;
        private bool topMost = true;
        private bool allowResize = true;
        private bool allowInteraction = true;
        private bool bindToOwner;
        private CaptureMaskEffect effect = CaptureMaskEffect.Black;
        private WindowCaptureProtectionResult lastProtectionResult =
            new WindowCaptureProtectionResult(IntPtr.Zero, WindowDisplayAffinity.None, true, 0);

        public int X { get { return x; } set { Set(ref x, value); } }
        public int Y { get { return y; } set { Set(ref y, value); } }
        public int Width { get { return width; } set { Set(ref width, Math.Max(80, value)); } }
        public int Height { get { return height; } set { Set(ref height, Math.Max(60, value)); } }
        public int BorderThickness { get { return borderThickness; } set { Set(ref borderThickness, Math.Max(1, value)); } }
        public int OpacityPercent { get { return opacityPercent; } set { Set(ref opacityPercent, Math.Max(0, Math.Min(100, value))); } }
        public bool ProtectFromCapture { get { return protectFromCapture; } set { Set(ref protectFromCapture, value); } }
        public bool BorderOnlyForUser { get { return borderOnlyForUser; } set { Set(ref borderOnlyForUser, value); } }
        public bool TopMost { get { return topMost; } set { Set(ref topMost, value); } }
        public bool AllowResize { get { return allowResize; } set { Set(ref allowResize, value); } }
        public bool AllowInteraction { get { return allowInteraction; } set { Set(ref allowInteraction, value); } }
        public bool BindToOwner { get { return bindToOwner; } set { Set(ref bindToOwner, value); } }
        public CaptureMaskEffect Effect { get { return effect; } set { Set(ref effect, value); } }
        public WindowCaptureProtectionResult LastProtectionResult { get { return lastProtectionResult; } }

        public Rectangle Bounds
        {
            get { return new Rectangle(X, Y, Width, Height); }
        }

        public void SetBounds(Rectangle bounds)
        {
            x = bounds.X;
            y = bounds.Y;
            width = Math.Max(80, bounds.Width);
            height = Math.Max(60, bounds.Height);
            RaiseChanged();
        }

        public void SetProtectionResult(WindowCaptureProtectionResult result)
        {
            lastProtectionResult = result;
            RaiseChanged();
        }

        private void Set<T>(ref T field, T value)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            EventHandler handler = Changed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
