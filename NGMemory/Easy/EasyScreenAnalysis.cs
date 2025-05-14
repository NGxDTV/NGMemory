using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace NGMemory.Easy
{
    /// <summary>
    /// Erweiterte Funktionen für die Bildschirmanalyse und Bilderkennung.
    /// </summary>
    public static class EasyScreenAnalysis
    {
        /// <summary>
        /// Findet alle Vorkommen einer Farbe in einem Bereich.
        /// </summary>
        /// <returns>Liste aller Positionen mit der gesuchten Farbe</returns>
        public static List<Point> FindAllColorMatches(Color targetColor, Rectangle searchArea, int tolerance = 0)
        {
            List<Point> matches = new List<Point>();
            
            using (Bitmap screenshot = EasyScreen.CaptureRegion(searchArea.X, searchArea.Y, searchArea.Width, searchArea.Height))
            {
                // Performance-Optimierung: BitmapData direkt verwenden statt GetPixel
                BitmapData bmpData = screenshot.LockBits(
                    new Rectangle(0, 0, screenshot.Width, screenshot.Height),
                    ImageLockMode.ReadOnly, 
                    screenshot.PixelFormat);
                
                int bytesPerPixel = Image.GetPixelFormatSize(screenshot.PixelFormat) / 8;
                int stride = bmpData.Stride;
                int bufferSize = stride * screenshot.Height;
                
                byte[] pixelData = new byte[bufferSize];
                Marshal.Copy(bmpData.Scan0, pixelData, 0, bufferSize);
                
                for (int y = 0; y < screenshot.Height; y++)
                {
                    for (int x = 0; x < screenshot.Width; x++)
                    {
                        int index = y * stride + x * bytesPerPixel;
                        byte b = pixelData[index];
                        byte g = pixelData[index + 1];
                        byte r = pixelData[index + 2];

                        if (Math.Abs(r - targetColor.R) <= tolerance &&
                            Math.Abs(g - targetColor.G) <= tolerance &&
                            Math.Abs(b - targetColor.B) <= tolerance)
                        {
                            matches.Add(new Point(searchArea.X + x, searchArea.Y + y));
                        }
                    }
                }

                screenshot.UnlockBits(bmpData);
            }
            
            return matches;
        }
        
        /// <summary>
        /// Vergleicht zwei Bilder und gibt den Ähnlichkeitswert zurück (0-100%).
        /// </summary>
        public static double CompareImages(Bitmap image1, Bitmap image2, int samplingRate = 10)
        {
            if (image1.Width != image2.Width || image1.Height != image2.Height)
                return 0; // Bilder müssen gleiche Größe haben
                
            int width = image1.Width;
            int height = image1.Height;
            long differences = 0;
            long totalPixels = 0;
            
            for (int y = 0; y < height; y += samplingRate)
            {
                for (int x = 0; x < width; x += samplingRate)
                {
                    Color pixel1 = image1.GetPixel(x, y);
                    Color pixel2 = image2.GetPixel(x, y);
                    
                    differences += Math.Abs(pixel1.R - pixel2.R);
                    differences += Math.Abs(pixel1.G - pixel2.G);
                    differences += Math.Abs(pixel1.B - pixel2.B);
                    
                    totalPixels++;
                }
            }
            
            // Maximaler Unterschied wäre 255*3 pro Pixel
            double maxDifference = totalPixels * 3 * 255;
            double similarityPercentage = 100 - ((differences * 100) / maxDifference);
            
            return similarityPercentage;
        }
        
        /// <summary>
        /// Sucht ein Template-Bild im Screenshot.
        /// </summary>
        /// <returns>Position des gefundenen Bildes oder null wenn nicht gefunden</returns>
        public static Point? FindImageOnScreen(Bitmap templateImage, Rectangle searchArea, double minSimilarity = 90)
        {
            using (Bitmap screenshot = EasyScreen.CaptureRegion(searchArea.X, searchArea.Y, searchArea.Width, searchArea.Height))
            {
                int maxX = screenshot.Width - templateImage.Width;
                int maxY = screenshot.Height - templateImage.Height;
                
                if (maxX < 0 || maxY < 0)
                    return null;
                
                double bestSimilarity = 0;
                Point bestPosition = new Point(0, 0);
                
                for (int y = 0; y <= maxY; y += 5) // Schrittweite für Performance
                {
                    for (int x = 0; x <= maxX; x += 5)
                    {
                        using (Bitmap region = screenshot.Clone(
                            new Rectangle(x, y, templateImage.Width, templateImage.Height), 
                            screenshot.PixelFormat))
                        {
                            double similarity = CompareImages(templateImage, region, 5);
                            if (similarity > bestSimilarity)
                            {
                                bestSimilarity = similarity;
                                bestPosition = new Point(x, y);
                                
                                if (bestSimilarity >= minSimilarity)
                                {
                                    return new Point(searchArea.X + x, searchArea.Y + y);
                                }
                            }
                        }
                    }
                }
                
                if (bestSimilarity >= minSimilarity)
                    return new Point(searchArea.X + bestPosition.X, searchArea.Y + bestPosition.Y);
            }
            
            return null;
        }
    }
}