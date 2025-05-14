using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NGMemory.Easy
{
    /// <summary>
    /// Funktionen für Bildschirmanalyse und Screenshots.
    /// </summary>
    public static class EasyScreen
    {
        /// <summary>
        /// Erstellt einen Screenshot vom gesamten Bildschirm.
        /// </summary>
        public static Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }
            
            return screenshot;
        }

        /// <summary>
        /// Erstellt einen Screenshot vom angegebenen Bereich des Bildschirms.
        /// </summary>
        public static Bitmap CaptureRegion(int x, int y, int width, int height)
        {
            Bitmap screenshot = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }
            
            return screenshot;
        }

        /// <summary>
        /// Erstellt einen Screenshot eines Fensters.
        /// </summary>
        public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            NGMemory.User32.RECT rect;
            NGMemory.User32.GetWindowRect(hWnd, out rect);
            
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            
            return CaptureRegion(rect.Left, rect.Top, width, height);
        }

        /// <summary>
        /// Sucht eine Farbe im angegebenen Bereich des Bildschirms.
        /// </summary>
        public static Point? FindColor(Color targetColor, Rectangle searchArea, int tolerance = 0)
        {
            using (Bitmap screenshot = CaptureRegion(searchArea.X, searchArea.Y, searchArea.Width, searchArea.Height))
            {
                for (int y = 0; y < screenshot.Height; y++)
                {
                    for (int x = 0; x < screenshot.Width; x++)
                    {
                        Color pixelColor = screenshot.GetPixel(x, y);
                        
                        if (ColorMatch(pixelColor, targetColor, tolerance))
                            return new Point(searchArea.X + x, searchArea.Y + y);
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Prüft, ob zwei Farben übereinstimmen (mit Toleranz).
        /// </summary>
        private static bool ColorMatch(Color c1, Color c2, int tolerance)
        {
            return Math.Abs(c1.R - c2.R) <= tolerance &&
                   Math.Abs(c1.G - c2.G) <= tolerance &&
                   Math.Abs(c1.B - c2.B) <= tolerance;
        }
    }
}
