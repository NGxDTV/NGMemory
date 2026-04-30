using System;
using System.Drawing;
using System.Windows.Forms;

namespace NGMemory.CaptureProtection
{
    public sealed class ProtectedAreaManager : IDisposable
    {
        private readonly Form owner;
        private readonly CaptureMaskViewModel model;
        private ProtectedAreaWindow window;
        private bool syncingFromWindow;
        private bool applyingProtection;

        public ProtectedAreaManager(Form owner, CaptureMaskViewModel model)
        {
            this.owner = owner;
            this.model = model;
            model.Changed += OnModelChanged;
            owner.Move += OnOwnerMoved;
            owner.Resize += OnOwnerMoved;
            owner.FormClosing += (sender, args) => Remove();
        }

        public bool HasWindow { get { return window != null && !window.IsDisposed; } }
        public IntPtr WindowHandle { get { return HasWindow ? window.Handle : IntPtr.Zero; } }

        public void Create()
        {
            if (HasWindow)
            {
                SyncWindowFromModel();
                ApplyProtection();
                return;
            }

            window = new ProtectedAreaWindow();
            window.UserBoundsChanged += OnUserMovedWindow;
            window.FormClosed += (sender, args) => window = null;
            SyncWindowFromModel();
            if (model.BindToOwner)
            {
                window.Show(owner);
            }
            else
            {
                window.Show();
            }
            ApplyProtection();
        }

        public void Remove()
        {
            if (window != null && !window.IsDisposed)
            {
                window.Close();
            }

            window = null;
        }

        public void ApplyProtection()
        {
            if (!HasWindow)
            {
                return;
            }

            applyingProtection = true;
            try
            {
                model.SetProtectionResult(CaptureMaskProtectionHelper.Apply(window.Handle, model.ProtectFromCapture, model.Effect));
            }
            finally
            {
                applyingProtection = false;
            }
        }

        private void OnModelChanged(object sender, EventArgs e)
        {
            if (syncingFromWindow || applyingProtection)
            {
                return;
            }

            SyncWindowFromModel();
            ApplyProtection();
        }

        private void OnOwnerMoved(object sender, EventArgs e)
        {
            if (model.BindToOwner)
            {
                SyncWindowFromModel();
            }
        }

        private void OnUserMovedWindow(object sender, EventArgs e)
        {
            if (!HasWindow)
            {
                return;
            }

            syncingFromWindow = true;
            model.SetBounds(ScreenToModelBounds(window.Bounds));
            syncingFromWindow = false;
        }

        private void SyncWindowFromModel()
        {
            if (!HasWindow)
            {
                return;
            }

            if (owner.WindowState == FormWindowState.Minimized && model.BindToOwner)
            {
                window.Hide();
                return;
            }

            window.Bounds = ModelToScreenBounds(model.Bounds);
            window.ApplyVisuals(model);
            if (!window.Visible)
            {
                if (model.BindToOwner)
                {
                    window.Show(owner);
                }
                else
                {
                    window.Show();
                }
            }
            window.BringToFront();
        }

        private Rectangle ModelToScreenBounds(Rectangle bounds)
        {
            if (!model.BindToOwner)
            {
                return bounds;
            }

            return new Rectangle(owner.PointToScreen(bounds.Location), bounds.Size);
        }

        private Rectangle ScreenToModelBounds(Rectangle bounds)
        {
            if (!model.BindToOwner)
            {
                return bounds;
            }

            return new Rectangle(owner.PointToClient(bounds.Location), bounds.Size);
        }

        public void Dispose()
        {
            model.Changed -= OnModelChanged;
            Remove();
        }
    }
}
