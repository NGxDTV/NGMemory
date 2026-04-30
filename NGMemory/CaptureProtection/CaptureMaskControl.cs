using System;
using System.Drawing;
using System.Windows.Forms;
using NGMemory.Easy;

namespace NGMemory.CaptureProtection
{
    public partial class CaptureMaskControl : UserControl
    {
        private readonly CaptureMaskViewModel model = new CaptureMaskViewModel();
        private ProtectedAreaManager manager;
        private bool updatingControls;

        public CaptureMaskControl()
        {
            InitializeComponent();
            effectComboBox.SelectedIndex = 0;
            model.Changed += (sender, args) =>
            {
                SyncControlsFromModel();
                UpdateDebugText();
            };
            WireEvents();
            SyncControlsFromModel();
            UpdateDebugText();
        }

        public CaptureMaskViewModel Model
        {
            get { return model; }
        }

        public IntPtr MaskHandle
        {
            get { return manager == null ? IntPtr.Zero : manager.WindowHandle; }
        }

        public void BindToOwner(Form owner)
        {
            if (manager != null)
            {
                manager.Dispose();
            }

            manager = new ProtectedAreaManager(owner, model);
            UpdateDebugText();
        }

        public void RemoveMask()
        {
            if (manager != null)
            {
                manager.Remove();
            }

            UpdateDebugText();
        }

        private void WireEvents()
        {
            createButton.Click += (sender, args) =>
            {
                EnsureManager();
                manager.Create();
                UpdateDebugText();
            };
            removeButton.Click += (sender, args) => RemoveMask();
            protectCheckBox.CheckedChanged += (sender, args) => { if (!updatingControls) model.ProtectFromCapture = protectCheckBox.Checked; };
            borderOnlyCheckBox.CheckedChanged += (sender, args) => { if (!updatingControls) model.BorderOnlyForUser = borderOnlyCheckBox.Checked; };
            topMostCheckBox.CheckedChanged += (sender, args) => { if (!updatingControls) model.TopMost = topMostCheckBox.Checked; };
            allowResizeCheckBox.CheckedChanged += (sender, args) => { if (!updatingControls) model.AllowResize = allowResizeCheckBox.Checked; };
            allowInteractionCheckBox.CheckedChanged += (sender, args) => { if (!updatingControls) model.AllowInteraction = allowInteractionCheckBox.Checked; };
            bindToOwnerCheckBox.CheckedChanged += (sender, args) => { if (!updatingControls) model.BindToOwner = bindToOwnerCheckBox.Checked; };
            effectComboBox.SelectedIndexChanged += (sender, args) =>
            {
                if (!updatingControls && effectComboBox.SelectedIndex >= 0)
                {
                    model.Effect = (CaptureMaskEffect)effectComboBox.SelectedIndex;
                    ApplyModeDefaults();
                }
            };

            xInput.ValueChanged += (sender, args) => UpdateBoundsFromInputs();
            yInput.ValueChanged += (sender, args) => UpdateBoundsFromInputs();
            widthInput.ValueChanged += (sender, args) => UpdateBoundsFromInputs();
            heightInput.ValueChanged += (sender, args) => UpdateBoundsFromInputs();
            borderInput.ValueChanged += (sender, args) => { if (!updatingControls) model.BorderThickness = (int)borderInput.Value; };
            opacitySlider.ValueChanged += (sender, args) => { if (!updatingControls) model.OpacityPercent = opacitySlider.Value; };
        }

        private void EnsureManager()
        {
            if (manager == null)
            {
                Form owner = FindForm();
                if (owner == null)
                {
                    throw new InvalidOperationException("CaptureMaskControl must be placed on a Form or bound with BindToOwner before creating a mask.");
                }

                BindToOwner(owner);
            }
        }

        private void UpdateBoundsFromInputs()
        {
            if (updatingControls)
            {
                return;
            }

            model.SetBounds(new Rectangle(
                (int)xInput.Value,
                (int)yInput.Value,
                (int)widthInput.Value,
                (int)heightInput.Value));
        }

        private void SyncControlsFromModel()
        {
            updatingControls = true;
            protectCheckBox.Checked = model.ProtectFromCapture;
            borderOnlyCheckBox.Checked = model.BorderOnlyForUser;
            topMostCheckBox.Checked = model.TopMost;
            allowResizeCheckBox.Checked = model.AllowResize;
            allowInteractionCheckBox.Checked = model.AllowInteraction;
            bindToOwnerCheckBox.Checked = model.BindToOwner;
            effectComboBox.SelectedIndex = (int)model.Effect;
            xInput.Value = Clamp(model.X, xInput.Minimum, xInput.Maximum);
            yInput.Value = Clamp(model.Y, yInput.Minimum, yInput.Maximum);
            widthInput.Value = Clamp(model.Width, widthInput.Minimum, widthInput.Maximum);
            heightInput.Value = Clamp(model.Height, heightInput.Minimum, heightInput.Maximum);
            borderInput.Value = Clamp(model.BorderThickness, borderInput.Minimum, borderInput.Maximum);
            opacitySlider.Value = Math.Max(opacitySlider.Minimum, Math.Min(opacitySlider.Maximum, model.OpacityPercent));
            bool screenshotOnlyMode = model.Effect == CaptureMaskEffect.Black;
            protectCheckBox.Enabled = screenshotOnlyMode;
            borderOnlyCheckBox.Enabled = !screenshotOnlyMode;
            updatingControls = false;
        }

        private void ApplyModeDefaults()
        {
            if (model.Effect == CaptureMaskEffect.Black)
            {
                model.ProtectFromCapture = true;
                model.BorderOnlyForUser = false;
                model.OpacityPercent = 0;
                return;
            }

            // Placeholder and blur are visible demo modes. They need real pixels,
            // so border-only transparency and capture exclusion must be off.
            model.BorderOnlyForUser = false;
            model.ProtectFromCapture = false;
            model.OpacityPercent = Math.Max(70, model.OpacityPercent);
        }

        private void UpdateDebugText()
        {
            WindowCaptureProtectionResult result = model.LastProtectionResult;
            string modeNote = model.Effect == CaptureMaskEffect.Black
                ? "Schwarz via WDA_MONITOR, intern min. 1% bei Opacity 0"
                : "Demo sichtbar, kein screenshot-only Effekt";
            debugLabel.Text = string.Format(
                "HWND: 0x{0:X}\r\n{1}\r\nAffinity: {2} | Return: {3} | Win32: {4}\r\nX/Y/B/H: {5}/{6}/{7}/{8}",
                MaskHandle.ToInt64(),
                modeNote,
                result.Affinity,
                result.ReturnValue ? "TRUE" : "FALSE",
                result.Win32Error,
                model.X,
                model.Y,
                model.Width,
                model.Height);
        }

        private static decimal Clamp(int value, decimal minimum, decimal maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
