using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NGMemory.CaptureProtection
{
    public sealed class ProtectedAreaWindow : Form
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int HTTRANSPARENT = -1;
        private const int HTCLIENT = 1;
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const long WS_EX_NOACTIVATE = 0x08000000L;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const int GripSize = 16;

        private Point dragStartMouse;
        private Rectangle dragStartBounds;
        private DragMode dragMode;

        public event EventHandler UserBoundsChanged;

        public ProtectedAreaWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            UpdateStyles();
            MinimumSize = new Size(80, 60);
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        public CaptureMaskEffect Effect { get; set; }
        public bool BorderOnlyForUser { get; set; }
        public bool AllowResize { get; set; }
        public bool AllowInteraction { get; set; } = true;
        public int BorderThickness { get; set; } = 3;
        public bool ShowTitle { get; set; } = true;

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= unchecked((int)WS_EX_TOOLWINDOW);
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (!AllowInteraction && m.Msg == WM_NCHITTEST)
            {
                m.Result = new IntPtr(HTTRANSPARENT);
                return;
            }

            if (BorderOnlyForUser && m.Msg == WM_NCHITTEST)
            {
                Point clientPoint = PointToClient(Cursor.Position);
                if (!IsInteractiveZone(clientPoint))
                {
                    m.Result = new IntPtr(HTTRANSPARENT);
                    return;
                }

                m.Result = new IntPtr(HTCLIENT);
                return;
            }

            base.WndProc(ref m);
        }

        public void ApplyVisuals(CaptureMaskViewModel model)
        {
            Effect = model.Effect;
            BorderOnlyForUser = model.BorderOnlyForUser && !(model.Effect == CaptureMaskEffect.Black && model.ProtectFromCapture);
            AllowResize = model.AllowResize;
            AllowInteraction = model.AllowInteraction;
            BorderThickness = model.BorderThickness;
            TopMost = model.TopMost;
            Opacity = CalculateWindowOpacity(model);
            ApplyInteractionStyle();

            if (BorderOnlyForUser)
            {
                BackColor = Color.Magenta;
                TransparencyKey = Color.Magenta;
            }
            else
            {
                BackColor = Color.FromArgb(30, 34, 42);
                TransparencyKey = Color.Empty;
            }

            Invalidate();
        }

        private void ApplyInteractionStyle()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            long exStyle = GetWindowLongPtrCompat(Handle, GWL_EXSTYLE).ToInt64();
            exStyle |= WS_EX_TOOLWINDOW;

            if (AllowInteraction)
            {
                exStyle &= ~WS_EX_TRANSPARENT;
                exStyle &= ~WS_EX_NOACTIVATE;
            }
            else
            {
                // WS_EX_TRANSPARENT makes the native window click-through for
                // normal mouse hit-testing. WS_EX_NOACTIVATE prevents focus steal,
                // so the user can keep interacting with the window underneath as
                // if this protected area was not there.
                exStyle |= WS_EX_TRANSPARENT;
                exStyle |= WS_EX_NOACTIVATE;
            }

            SetWindowLongPtrCompat(Handle, GWL_EXSTYLE, new IntPtr(exStyle));
            SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (!BorderOnlyForUser)
            {
                PaintVisibleContent(e.Graphics);
            }

            using (Pen pen = new Pen(Color.DeepSkyBlue, BorderThickness))
            {
                if (modelShouldDrawChrome())
                {
                    Rectangle border = ClientRectangle;
                    border.Inflate(-BorderThickness / 2, -BorderThickness / 2);
                    e.Graphics.DrawRectangle(pen, border);
                }
            }

            if (ShowTitle && modelShouldDrawChrome())
            {
                using (Brush titleBrush = new SolidBrush(Color.FromArgb(230, Color.DeepSkyBlue)))
                using (Brush textBrush = new SolidBrush(Color.Black))
                {
                    Rectangle title = new Rectangle(8, 6, 132, 24);
                    e.Graphics.FillRectangle(titleBrush, title);
                    e.Graphics.DrawString("Protected Area", Font, textBrush, title.X + 8, title.Y + 4);
                }
            }

            if (modelShouldDrawChrome())
            {
                PaintCrosshairAndGrips(e.Graphics);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private double CalculateWindowOpacity(CaptureMaskViewModel model)
        {
            if (model.Effect == CaptureMaskEffect.Black && model.ProtectFromCapture && model.OpacityPercent == 0)
            {
                // Fully transparent layered windows are often ignored by capture
                // stacks/composition, so WDA_MONITOR may no longer produce a black
                // rectangle. 1% is visually almost gone but keeps a real window
                // surface for the capture pipeline.
                return 0.01;
            }

            return model.OpacityPercent / 100.0;
        }

        private bool modelShouldDrawChrome()
        {
            return Opacity > 0.02;
        }

        private void PaintVisibleContent(Graphics graphics)
        {
            if (Effect == CaptureMaskEffect.PlaceholderText)
            {
                graphics.Clear(Color.FromArgb(18, 18, 18));
                DrawCenteredText(graphics, "CAPTURE PLACEHOLDER");
                return;
            }

            if (Effect == CaptureMaskEffect.SimulatedBlurDemoOnly)
            {
                PaintSimulatedBlur(graphics);
                return;
            }

            using (Brush brush = new SolidBrush(Color.FromArgb(42, 48, 60)))
            {
                graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        private void PaintSimulatedBlur(Graphics graphics)
        {
            // This is only a placeholder for content controlled by the app. Normal
            // Win32 cannot reliably blur arbitrary foreign windows beneath us.
            graphics.Clear(Color.FromArgb(88, 94, 106));

            using (Brush light = new SolidBrush(Color.FromArgb(150, Color.White)))
            using (Brush dark = new SolidBrush(Color.FromArgb(120, Color.DarkSlateBlue)))
            {
                for (int y = -40; y < Height + 80; y += 34)
                {
                    graphics.FillEllipse(light, new Rectangle(-30, y, Width + 60, 28));
                    graphics.FillEllipse(dark, new Rectangle(20, y + 10, Width - 40, 26));
                }
            }

            DrawCenteredText(graphics, "SIMULIERTER BLUR\nnur Demo-Inhalte");
        }

        private void PaintCrosshairAndGrips(Graphics graphics)
        {
            using (Pen pen = new Pen(Color.FromArgb(180, Color.DeepSkyBlue), 1))
            {
                graphics.DrawLine(pen, Width / 2, 0, Width / 2, Height);
                graphics.DrawLine(pen, 0, Height / 2, Width, Height / 2);
            }

            if (!AllowResize)
            {
                return;
            }

            using (Brush brush = new SolidBrush(Color.DeepSkyBlue))
            {
                graphics.FillRectangle(brush, new Rectangle(Width - GripSize, Height - GripSize, GripSize, GripSize));
            }
        }

        private void DrawCenteredText(Graphics graphics, string text)
        {
            using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (Brush brush = new SolidBrush(Color.White))
            {
                graphics.DrawString(text, Font, brush, ClientRectangle, format);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!AllowInteraction)
            {
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            dragMode = GetDragMode(e.Location);
            dragStartMouse = Cursor.Position;
            dragStartBounds = Bounds;
            Capture = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!AllowInteraction)
            {
                return;
            }

            if (!Capture || dragMode == DragMode.None)
            {
                Cursor = GetDragMode(e.Location) == DragMode.Move ? Cursors.SizeAll : Cursors.SizeNWSE;
                return;
            }

            Point delta = new Point(Cursor.Position.X - dragStartMouse.X, Cursor.Position.Y - dragStartMouse.Y);
            Rectangle next = dragStartBounds;

            if (dragMode == DragMode.Move)
            {
                next.X += delta.X;
                next.Y += delta.Y;
            }
            else
            {
                next.Width = Math.Max(MinimumSize.Width, dragStartBounds.Width + delta.X);
                next.Height = Math.Max(MinimumSize.Height, dragStartBounds.Height + delta.Y);
            }

            Bounds = next;
            RaiseUserBoundsChanged();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!AllowInteraction)
            {
                return;
            }

            Capture = false;
            dragMode = DragMode.None;
        }

        private DragMode GetDragMode(Point point)
        {
            if (AllowResize && point.X >= Width - GripSize && point.Y >= Height - GripSize)
            {
                return DragMode.ResizeBottomRight;
            }

            return DragMode.Move;
        }

        private bool IsInteractiveZone(Point point)
        {
            int border = Math.Max(BorderThickness + 6, 10);
            bool onBorder = point.X <= border || point.Y <= border || point.X >= Width - border || point.Y >= Height - border;
            bool onTitle = ShowTitle && point.X >= 8 && point.X <= 150 && point.Y >= 6 && point.Y <= 34;
            bool onGrip = AllowResize && point.X >= Width - GripSize - 4 && point.Y >= Height - GripSize - 4;
            return onBorder || onTitle || onGrip;
        }

        private void RaiseUserBoundsChanged()
        {
            EventHandler handler = UserBoundsChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private enum DragMode
        {
            None,
            Move,
            ResizeBottomRight
        }

        private static IntPtr GetWindowLongPtrCompat(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr(hWnd, nIndex) : new IntPtr(GetWindowLong(hWnd, nIndex));
        }

        private static IntPtr SetWindowLongPtrCompat(IntPtr hWnd, int nIndex, IntPtr value)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr(hWnd, nIndex, value) : new IntPtr(SetWindowLong(hWnd, nIndex, value.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
    }
}
