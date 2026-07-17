using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NGMemory.Easy;

namespace NGMemory.CaptureProtection
{
    /// <summary>
    /// Macht bestimmte Bereiche eines Formulars NUR auf Screenshots/Aufnahmen
    /// (Videos) unscharf, waehrend sie am Bildschirm scharf bleiben.
    ///
    /// Technik (zwei Ebenen, auf die Sensiblen-Rechtecke zugeschnitten):
    /// <list type="bullet">
    /// <item>Untere Ebene: ein verschwommener Schnappschuss der Bereiche, normale
    /// Anzeige-Affinity -> landet in Screenshot UND Video.</item>
    /// <item>Obere Ebene: der scharfe Schnappschuss, mit
    /// <c>WDA_EXCLUDEFROMCAPTURE</c> -> am Bildschirm sichtbar, aus jeder Aufnahme
    /// ausgeschlossen.</item>
    /// </list>
    /// Ergebnis: der Nutzer liest die Daten scharf, in Aufnahmen sind sie echt
    /// verschwommen.
    ///
    /// Die Overlays sind vollstaendig klick-/tastatur-durchlaessig
    /// (<c>WS_EX_LAYERED|WS_EX_TRANSPARENT</c> + HTTRANSPARENT), die Bedienung der
    /// echten Controls darunter (Markieren, Tippen, Kopieren) bleibt also erhalten.
    /// Damit man Eingaben/Markierungen auch SIEHT (die scharfe Ebene ist ja ein
    /// Abbild), gibt es den <see cref="LiveTracking"/>-Modus, der den Schnappschuss
    /// laufend auffrischt, solange das Fenster aktiv ist.
    ///
    /// Bereiche: fest (<see cref="ProtectControls"/>/<see cref="ProtectRegions"/>)
    /// oder dynamisch (<see cref="ProtectDynamic"/>). Braucht Win10 2004+.
    /// </summary>
    public sealed class ScreenshotBlurProtector : IDisposable
    {
        private readonly Form owner;
        private readonly List<Func<IEnumerable<Rectangle>>> providers = new List<Func<IEnumerable<Rectangle>>>();

        private BlurOverlayWindow blurWindow;
        private BlurOverlayWindow sharpWindow;
        private System.Windows.Forms.Timer liveTimer;
        private bool overlaysShown;
        private bool ownerActive = true;
        private bool enabled;
        private bool disposed;

        public ScreenshotBlurProtector(Form owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.owner = owner;
            owner.LocationChanged += OnOwnerLayoutChanged;
            owner.SizeChanged += OnOwnerResized;
            owner.Activated += OnOwnerActivated;
            owner.Deactivate += OnOwnerDeactivated;
            owner.FormClosing += OnOwnerClosing;
        }

        /// <summary>Staerke der Unschaerfe (siehe <see cref="ImageBlur"/>).</summary>
        public int BlurStrength { get; set; } = 12;

        /// <summary>Zusaetzlicher Rand um jedes geschuetzte Rechteck (Pixel).</summary>
        public int Padding { get; set; } = 4;

        /// <summary>
        /// Wenn true, wird der scharfe Schnappschuss laufend aufgefrischt, solange das
        /// Fenster aktiv ist. Noetig, damit Eingaben/Markierungen in geschuetzten
        /// EDITIERBAREN Feldern sichtbar sind. Fuer rein lesende Felder aus lassen.
        /// </summary>
        public bool LiveTracking { get; set; }

        /// <summary>Intervall des Live-Refresh in ms.</summary>
        public int LiveIntervalMs { get; set; } = 66;

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
        /// Registriert eine dynamische Quelle von Rechtecken (Owner-Client-Koordinaten),
        /// die bei jedem <see cref="Refresh"/> neu abgefragt wird (z. B. Spalten-/
        /// Zellbereiche einer scrollenden ListView).
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

            EnsureWindows();
            enabled = true;
            Refresh();
            UpdateLiveState();
        }

        public void Disable()
        {
            enabled = false;
            HideWindows();
            UpdateLiveState();
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
        /// Aktualisiert Schnappschuss und Zuschnitt-Region. Aufrufen, wenn sich die
        /// geschuetzten Inhalte oder ihre Positionen geaendert haben (z. B. Scrollen).
        /// </summary>
        public void Refresh()
        {
            if (!enabled || disposed || !owner.IsHandleCreated)
            {
                return;
            }

            if (owner.WindowState == FormWindowState.Minimized)
            {
                HideWindows();
                return;
            }

            List<Rectangle> rects = CollectRects();
            if (rects.Count == 0)
            {
                HideWindows();
                return;
            }

            Rectangle client = owner.ClientRectangle;
            if (client.Width <= 0 || client.Height <= 0)
            {
                return;
            }

            using (Bitmap sharp = CaptureOwnerClient(client))
            {
                if (sharp == null)
                {
                    return;
                }

                Bitmap blurred = ImageBlur.Blur(sharp, BlurStrength);
                sharpWindow.SetImage((Bitmap)sharp.Clone());
                blurWindow.SetImage(blurred);
            }

            PositionWindows();
            blurWindow.SetClipRegion(rects);
            sharpWindow.SetClipRegion(rects);

            if (!overlaysShown)
            {
                // Reihenfolge: Blur unten, Scharf oben. Nur EINMAL zeigen/holen/Affinity
                // setzen (nicht bei jedem Live-Refresh -> kein Flackern/Z-Order-Churn).
                if (!blurWindow.Visible)
                {
                    blurWindow.Show(owner);
                }

                if (!sharpWindow.Visible)
                {
                    sharpWindow.Show(owner);
                }

                blurWindow.BringToFront();
                sharpWindow.BringToFront();
                ApplyAffinity();
                overlaysShown = true;
            }
        }

        private void HideWindows()
        {
            overlaysShown = false;
            if (blurWindow != null && !blurWindow.IsDisposed)
            {
                blurWindow.Hide();
            }

            if (sharpWindow != null && !sharpWindow.IsDisposed)
            {
                sharpWindow.Hide();
            }
        }

        private void EnsureWindows()
        {
            if (blurWindow == null || blurWindow.IsDisposed)
            {
                blurWindow = new BlurOverlayWindow();
            }

            if (sharpWindow == null || sharpWindow.IsDisposed)
            {
                sharpWindow = new BlurOverlayWindow();
            }
        }

        private void ApplyAffinity()
        {
            if (blurWindow != null && blurWindow.IsHandleCreated)
            {
                EasyWindow.SetDisplayAffinity(blurWindow.Handle, WindowDisplayAffinity.None);
            }

            if (sharpWindow != null && sharpWindow.IsHandleCreated)
            {
                LastProtectionResult = EasyWindow.SetDisplayAffinity(
                    sharpWindow.Handle, WindowDisplayAffinity.ExcludeFromCapture);
            }
        }

        private void PositionWindows()
        {
            Rectangle screenClient = owner.RectangleToScreen(owner.ClientRectangle);
            if (blurWindow != null)
            {
                blurWindow.Bounds = screenClient;
            }

            if (sharpWindow != null)
            {
                sharpWindow.Bounds = screenClient;
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

        /// <summary>
        /// Rendert den Client-Bereich des Owners in ein Bitmap. Bevorzugt
        /// <c>PrintWindow(PW_RENDERFULLCONTENT)</c> (rendert auch owner-gezeichnete
        /// ListViews korrekt und faengt die eigenen Overlays NICHT mit ein); faellt
        /// bei Bedarf auf <c>DrawToBitmap</c> zurueck.
        /// </summary>
        private Bitmap CaptureOwnerClient(Rectangle client)
        {
            try
            {
                RECT wr;
                if (GetWindowRect(owner.Handle, out wr))
                {
                    int winWidth = wr.Right - wr.Left;
                    int winHeight = wr.Bottom - wr.Top;
                    if (winWidth > 0 && winHeight > 0)
                    {
                        using (Bitmap windowBmp = new Bitmap(winWidth, winHeight))
                        {
                            bool ok;
                            using (Graphics g = Graphics.FromImage(windowBmp))
                            {
                                IntPtr hdc = g.GetHdc();
                                ok = PrintWindow(owner.Handle, hdc, PW_RENDERFULLCONTENT);
                                g.ReleaseHdc(hdc);
                            }

                            if (ok)
                            {
                                Point clientOnScreen = owner.PointToScreen(Point.Empty);
                                int offX = clientOnScreen.X - wr.Left;
                                int offY = clientOnScreen.Y - wr.Top;

                                Bitmap clientBmp = new Bitmap(client.Width, client.Height);
                                using (Graphics g = Graphics.FromImage(clientBmp))
                                {
                                    g.DrawImage(
                                        windowBmp,
                                        new Rectangle(0, 0, client.Width, client.Height),
                                        new Rectangle(offX, offY, client.Width, client.Height),
                                        GraphicsUnit.Pixel);
                                }

                                return clientBmp;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Faellt unten auf DrawToBitmap zurueck.
            }

            try
            {
                Bitmap fallback = new Bitmap(client.Width, client.Height);
                owner.DrawToBitmap(fallback, new Rectangle(0, 0, client.Width, client.Height));
                return fallback;
            }
            catch
            {
                return null;
            }
        }

        // ---- Live-Refresh (fuer editierbare Felder) ------------------------
        private void UpdateLiveState()
        {
            bool run = enabled && LiveTracking && ownerActive && !disposed;
            if (run)
            {
                if (liveTimer == null)
                {
                    liveTimer = new System.Windows.Forms.Timer();
                    liveTimer.Tick += (s, e) => { if (enabled) Refresh(); };
                }

                liveTimer.Interval = Math.Max(20, LiveIntervalMs);
                if (!liveTimer.Enabled)
                {
                    liveTimer.Start();
                }
            }
            else if (liveTimer != null && liveTimer.Enabled)
            {
                liveTimer.Stop();
            }
        }

        private void OnOwnerActivated(object sender, EventArgs e)
        {
            ownerActive = true;
            UpdateLiveState();
        }

        private void OnOwnerDeactivated(object sender, EventArgs e)
        {
            ownerActive = false;
            UpdateLiveState();
        }

        private void OnOwnerLayoutChanged(object sender, EventArgs e)
        {
            if (enabled)
            {
                PositionWindows();
            }
        }

        private void OnOwnerResized(object sender, EventArgs e)
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
            owner.LocationChanged -= OnOwnerLayoutChanged;
            owner.SizeChanged -= OnOwnerResized;
            owner.Activated -= OnOwnerActivated;
            owner.Deactivate -= OnOwnerDeactivated;
            owner.FormClosing -= OnOwnerClosing;

            if (liveTimer != null)
            {
                liveTimer.Stop();
                liveTimer.Dispose();
                liveTimer = null;
            }

            if (blurWindow != null && !blurWindow.IsDisposed)
            {
                blurWindow.Close();
            }

            if (sharpWindow != null && !sharpWindow.IsDisposed)
            {
                sharpWindow.Close();
            }

            blurWindow = null;
            sharpWindow = null;
        }

        private const uint PW_RENDERFULLCONTENT = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        /// <summary>
        /// Randloses, VOLL klick-/tastatur-durchlaessiges Overlay-Fenster
        /// (<c>WS_EX_LAYERED|WS_EX_TRANSPARENT</c>), das ein Bild zeichnet und per
        /// Fensterregion auf die geschuetzten Rechtecke zugeschnitten ist.
        /// </summary>
        private sealed class BlurOverlayWindow : Form
        {
            private const int WM_NCHITTEST = 0x0084;
            private const int HTTRANSPARENT = -1;
            private const int WS_EX_TOOLWINDOW = 0x00000080;
            private const int WS_EX_TRANSPARENT = 0x00000020;
            private const int WS_EX_LAYERED = 0x00080000;
            private const int WS_EX_NOACTIVATE = 0x08000000;
            private const int LWA_ALPHA = 0x00000002;

            private Bitmap image;

            public BlurOverlayWindow()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                ControlBox = false;
                MinimizeBox = false;
                MaximizeBox = false;
                DoubleBuffered = true;
                SetStyle(
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
                UpdateStyles();
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
                    // WS_EX_LAYERED zusammen mit WS_EX_TRANSPARENT = zuverlaessiges
                    // Click-Through zu den echten Controls darunter.
                    cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_LAYERED;
                    return cp;
                }
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                // Vollstaendig deckend (Alpha 255) -> das Bild ist normal sichtbar und
                // wird von Aufnahmen erfasst; die Layered-Flag dient nur dem
                // Input-Durchlass.
                SetLayeredWindowAttributes(Handle, 0, 255, LWA_ALPHA);
            }

            public void SetImage(Bitmap bmp)
            {
                Bitmap old = image;
                image = bmp;
                if (old != null)
                {
                    old.Dispose();
                }

                Invalidate();
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
                // Hit-Tests immer durchreichen (zusaetzlich zu WS_EX_TRANSPARENT),
                // damit Maus/Selektion unter dem Overlay ankommt.
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = new IntPtr(HTTRANSPARENT);
                    return;
                }

                base.WndProc(ref m);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Innerhalb der Region deckt das Bild alles ab.
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (image != null)
                {
                    e.Graphics.DrawImageUnscaled(image, 0, 0);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && image != null)
                {
                    image.Dispose();
                    image = null;
                }

                base.Dispose(disposing);
            }

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, int dwFlags);
        }
    }
}
