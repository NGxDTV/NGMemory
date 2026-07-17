using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NGMemory.Easy;

namespace NGMemory.CaptureProtection
{
    /// <summary>
    /// Blendet bestimmte Bereiche eines Formulars in Screenshots UND Videos aus
    /// (schwarz), waehrend sie am Bildschirm voellig normal und unveraendert
    /// bleiben.
    ///
    /// Technik: EIN randloses Maskenfenster ueber den sensiblen Rechtecken mit
    /// <c>WDA_MONITOR</c>. Windows zeichnet solche Fenster in Aufnahmen schwarz,
    /// am Bildschirm liegt es mit 1 % Deckkraft praktisch unsichtbar darueber
    /// (0 % waere unsichtbar auch fuer die Capture-Pipeline, dann bliebe der
    /// Schwarz-Effekt aus).
    ///
    /// Im Gegensatz zum <see cref="ScreenshotBlurProtector"/> wird hier KEIN
    /// Schnappschuss des Inhalts gebraucht. Dadurch gibt es keinerlei Artefakte:
    /// nichts flackert, nichts bleibt beim Scrollen stehen, nichts ueberdeckt den
    /// echten Inhalt. Das Fenster ist ausserdem klick-/tastatur-durchlaessig, die
    /// Bedienung darunter bleibt komplett erhalten.
    ///
    /// Trade-off: in der Aufnahme ist der Bereich schwarz (nicht unscharf) —
    /// Windows kann per Anzeige-Affinity nur schwarz, keine Unschaerfe.
    /// </summary>
    public sealed class CaptureBlackoutProtector : IDisposable
    {
        private readonly Form owner;
        private readonly List<Func<IEnumerable<Rectangle>>> providers = new List<Func<IEnumerable<Rectangle>>>();

        private MaskOverlayWindow mask;
        private bool shown;
        private bool enabled;
        private bool disposed;

        public CaptureBlackoutProtector(Form owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.owner = owner;
            owner.LocationChanged += OnOwnerMovedOrResized;
            owner.SizeChanged += OnOwnerMovedOrResized;
            owner.FormClosing += OnOwnerClosing;
        }

        /// <summary>Zusaetzlicher Rand um jedes geschuetzte Rechteck (Pixel).</summary>
        public int Padding { get; set; } = 2;

        public bool IsEnabled { get { return enabled; } }

        public WindowCaptureProtectionResult LastProtectionResult { get; private set; }

        public void ProtectControls(params Control[] controls)
        {
            if (controls == null)
            {
                return;
            }

            foreach (Control c in controls)
            {
                if (c == null)
                {
                    continue;
                }

                Control local = c;
                providers.Add(() => new[] { ControlToOwnerClientRect(local) });
            }

            if (enabled)
            {
                Refresh();
            }
        }

        public void ProtectRegions(params Rectangle[] clientRects)
        {
            if (clientRects == null)
            {
                return;
            }

            foreach (Rectangle r in clientRects)
            {
                Rectangle local = r;
                providers.Add(() => new[] { local });
            }

            if (enabled)
            {
                Refresh();
            }
        }

        /// <summary>
        /// Dynamische Quelle von Rechtecken (Owner-Client-Koordinaten), die bei jedem
        /// <see cref="Refresh"/> neu abgefragt wird (z. B. eine Spalte einer ListView).
        /// </summary>
        public void ProtectDynamic(Func<IEnumerable<Rectangle>> provider)
        {
            if (provider == null)
            {
                return;
            }

            providers.Add(provider);

            if (enabled)
            {
                Refresh();
            }
        }

        public void Enable()
        {
            if (disposed || enabled)
            {
                return;
            }

            if (mask == null || mask.IsDisposed)
            {
                mask = new MaskOverlayWindow();
            }

            enabled = true;
            Refresh();
        }

        public void Disable()
        {
            enabled = false;
            shown = false;
            if (mask != null && !mask.IsDisposed)
            {
                mask.Hide();
            }
        }

        public void Toggle()
        {
            if (enabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }

        /// <summary>
        /// Aktualisiert Position und Zuschnitt der Maske. Guenstig (kein Schnappschuss),
        /// darf oft aufgerufen werden.
        /// </summary>
        public void Refresh()
        {
            if (!enabled || disposed || !owner.IsHandleCreated)
            {
                return;
            }

            if (owner.WindowState == FormWindowState.Minimized)
            {
                shown = false;
                mask.Hide();
                return;
            }

            List<Rectangle> rects = CollectRects();
            if (rects.Count == 0)
            {
                shown = false;
                mask.Hide();
                return;
            }

            mask.Bounds = owner.RectangleToScreen(owner.ClientRectangle);
            mask.SetClipRegion(rects);

            if (!shown)
            {
                if (!mask.Visible)
                {
                    mask.Show(owner);
                }

                mask.BringToFront();
                LastProtectionResult = EasyWindow.SetDisplayAffinity(
                    mask.Handle, WindowDisplayAffinity.Monitor);
                shown = true;
            }
        }

        private List<Rectangle> CollectRects()
        {
            List<Rectangle> rects = new List<Rectangle>();
            Rectangle client = owner.ClientRectangle;

            foreach (Func<IEnumerable<Rectangle>> provider in providers)
            {
                IEnumerable<Rectangle> provided;
                try
                {
                    provided = provider();
                }
                catch
                {
                    continue;
                }

                if (provided == null)
                {
                    continue;
                }

                foreach (Rectangle raw in provided)
                {
                    Rectangle r = raw;
                    r.Inflate(Padding, Padding);
                    r.Intersect(client);
                    if (r.Width > 0 && r.Height > 0)
                    {
                        rects.Add(r);
                    }
                }
            }

            return rects;
        }

        private Rectangle ControlToOwnerClientRect(Control control)
        {
            if (control == null || control.Parent == null || !control.Visible)
            {
                return Rectangle.Empty;
            }

            Rectangle screen = control.Parent.RectangleToScreen(control.Bounds);
            Point clientTopLeft = owner.PointToClient(screen.Location);
            return new Rectangle(clientTopLeft, screen.Size);
        }

        private void OnOwnerMovedOrResized(object sender, EventArgs e)
        {
            if (enabled)
            {
                Refresh();
            }
        }

        private void OnOwnerClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            enabled = false;
            owner.LocationChanged -= OnOwnerMovedOrResized;
            owner.SizeChanged -= OnOwnerMovedOrResized;
            owner.FormClosing -= OnOwnerClosing;

            if (mask != null && !mask.IsDisposed)
            {
                mask.Close();
            }

            mask = null;
        }

        /// <summary>
        /// Randloses, klick-durchlaessiges, praktisch unsichtbares schwarzes Fenster.
        /// Mit <c>WDA_MONITOR</c> erscheint es in Aufnahmen als schwarze Flaeche.
        /// </summary>
        private sealed class MaskOverlayWindow : Form
        {
            private const int WM_NCHITTEST = 0x0084;
            private const int HTTRANSPARENT = -1;
            private const int WS_EX_TOOLWINDOW = 0x00000080;
            private const int WS_EX_TRANSPARENT = 0x00000020;
            private const int WS_EX_NOACTIVATE = 0x08000000;

            public MaskOverlayWindow()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                ControlBox = false;
                MinimizeBox = false;
                MaximizeBox = false;
                BackColor = Color.Black;
                // 1 % statt 0 %: unsichtbar fuers Auge, aber die Capture-Pipeline
                // sieht eine echte Fensterflaeche -> WDA_MONITOR wird schwarz.
                Opacity = 0.01;
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            public void SetClipRegion(IEnumerable<Rectangle> rects)
            {
                Region region = new Region();
                region.MakeEmpty();
                foreach (Rectangle r in rects)
                {
                    region.Union(r);
                }

                Region = region;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = new IntPtr(HTTRANSPARENT);
                    return;
                }

                base.WndProc(ref m);
            }
        }
    }
}
