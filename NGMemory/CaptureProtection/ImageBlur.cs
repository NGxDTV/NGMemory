using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NGMemory.CaptureProtection
{
    /// <summary>
    /// Erzeugt eine echte, weiche Unschaerfe (kein schwarzer Balken, kein Muster).
    /// Verfahren: Herunterskalieren und wieder Hochskalieren mit bilinearer
    /// Interpolation. Das macht Text zuverlaessig unlesbar und ist reines GDI+
    /// (kein unsicherer Code, keine LockBits), also robust auf allen Systemen.
    /// </summary>
    public static class ImageBlur
    {
        /// <summary>
        /// Liefert eine unscharfe Kopie von <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Quellbild (wird nicht veraendert).</param>
        /// <param name="strength">
        /// Staerke der Unschaerfe (Faktor zum Herunterskalieren). Groesser = mehr
        /// Unschaerfe. Sinnvoll sind Werte zwischen 6 und 16. Werte &lt; 2 werden
        /// auf 2 angehoben.
        /// </param>
        public static Bitmap Blur(Bitmap source, int strength)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            int factor = Math.Max(2, strength);
            int smallWidth = Math.Max(1, source.Width / factor);
            int smallHeight = Math.Max(1, source.Height / factor);

            // Zwei Stufen (runter -> hoch) ergeben eine gleichmaessige,
            // milchglasartige Unschaerfe.
            using (Bitmap small = new Bitmap(smallWidth, smallHeight))
            {
                using (Graphics g = Graphics.FromImage(small))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(source, new Rectangle(0, 0, smallWidth, smallHeight));
                }

                Bitmap result = new Bitmap(source.Width, source.Height);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(small, new Rectangle(0, 0, source.Width, source.Height));
                }

                return result;
            }
        }
    }
}
