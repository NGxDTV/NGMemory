using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace NGMemory.Easy
{
    /// <summary>
    /// Enum für Maustasten
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
    
    /// <summary>
    /// Stellt einfache Methoden für Mausoperationen bereit.
    /// </summary>
    public static class EasyMouse
    {
        /// <summary>
        /// Bewegt die Maus an die angegebene Position.
        /// </summary>
        public static void MoveTo(int x, int y)
        {
            WinInteropTools.InputHelper.MouseMoveTo(x, y);
        }

        /// <summary>
        /// Bewegt die Maus mit menschenähnlicher Bewegung zur Zielposition.
        /// </summary>
        public static void MoveWithHumanMotion(int targetX, int targetY, int duration = 500)
        {
            // Aktuelle Position holen
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            int startX = System.Windows.Forms.Cursor.Position.X;
            int startY = System.Windows.Forms.Cursor.Position.Y;
            
            Random random = new Random();
            
            // Mittelpunkt mit leichter Abweichung
            Point control1 = new Point(
                (startX + targetX) / 2 + random.Next(-50, 50),
                (startY + targetY) / 2 + random.Next(-50, 50)
            );
            
            // Anzahl der Schritte basierend auf der Distanz
            double distance = Math.Sqrt(Math.Pow(targetX - startX, 2) + Math.Pow(targetY - startY, 2));
            int steps = Math.Max(5, duration / 10);

            // Verzögerung zwischen Bewegungen
            int delay = Math.Max(1, duration / steps);

            // Bézierkurve für natürliche Bewegung
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                
                // Quadratische Bézierkurve: B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
                double xt = Math.Pow(1 - t, 2) * startX + 
                           2 * (1 - t) * t * control1.X + 
                           Math.Pow(t, 2) * targetX;
                
                double yt = Math.Pow(1 - t, 2) * startY +
                           2 * (1 - t) * t * control1.Y +
                           Math.Pow(t, 2) * targetY;
                
                // Bewegung mit leichtem Zittern
                int moveX = (int)xt + random.Next(-2, 3);
                int moveY = (int)yt + random.Next(-2, 3);
                
                // Auf Bildschirmgrenzen beschränken
                moveX = Math.Max(0, Math.Min(screen.Width, moveX));
                moveY = Math.Max(0, Math.Min(screen.Height, moveY));
                
                WinInteropTools.InputHelper.MouseMoveTo(moveX, moveY);
                Thread.Sleep(delay);
            }
            
            // Final exakte Position setzen
            WinInteropTools.InputHelper.MouseMoveTo(targetX, targetY);
        }

        /// <summary>
        /// Führt einen Mausklick an der aktuellen Position aus.
        /// </summary>
        public static void Click(MouseButton button = MouseButton.Left)
        {
            WinInteropTools.InputHelper.MouseClick(button);
        }

        /// <summary>
        /// Führt einen Mausklick an der angegebenen Position aus.
        /// </summary>
        public static void ClickAt(int x, int y, MouseButton button = MouseButton.Left)
        {
            MoveTo(x, y);
            System.Threading.Thread.Sleep(10);
            Click(button);
        }

        /// <summary>
        /// Bewegt die Maus zur angegebenen Position mit menschlicher Bewegung und klickt.
        /// </summary>
        public static void HumanClickAt(int x, int y, bool doubleClick = false, MouseButton button = MouseButton.Left, int moveTime = 500)
        {
            MoveWithHumanMotion(x, y, 500);
            Thread.Sleep(50 + new Random().Next(30));
            if(doubleClick)
                DoubleClick(button);
            else
                Click(button);
        }

        /// <summary>
        /// Führt einen Doppelklick an der aktuellen Position aus.
        /// </summary>
        public static void DoubleClick(MouseButton button = MouseButton.Left)
        {
            Click(button);
            System.Threading.Thread.Sleep(10);
            Click(button);
        }

        /// <summary>
        /// Führt eine Drag-and-Drop-Operation aus.
        /// </summary>
        public static void DragAndDrop(int fromX, int fromY, int toX, int toY)
        {
            MoveTo(fromX, fromY);
            System.Threading.Thread.Sleep(50);
            WinInteropTools.InputHelper.MouseDown(MouseButton.Left);
            System.Threading.Thread.Sleep(50);
            MoveTo(toX, toY);
            System.Threading.Thread.Sleep(50);
            WinInteropTools.InputHelper.MouseUp(MouseButton.Left);
        }
    }
}