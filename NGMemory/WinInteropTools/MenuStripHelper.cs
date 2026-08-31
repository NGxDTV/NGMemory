using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static NGMemory.User32;
using static NGMemory.Constants;
using System.Threading;

namespace NGMemory.WinInteropTools
{
    public class MenuStripHelper
    {
        /// <summary>GetMenuItemID liefert das für Trenner, Untermenüs und ungültige Positionen.</summary>
        private const uint InvalidMenuId = 0xFFFFFFFF;

        private const uint MF_BYPOSITION = 0x00000400;

        public static void ClickMenu(IntPtr hWnd, bool async, params int[] path)
        {
            TryClickMenu(hWnd, async, path);
        }

        /// <summary>
        /// Löst den Menüpfad auf und schickt das zugehörige WM_COMMAND.
        /// Liefert false, wenn der Pfad ins Leere zeigt - etwa weil die Anwendung ihre
        /// Menüstruktur geändert hat und die Position jetzt auf einem Trenner oder einem
        /// Untermenü landet. In dem Fall wird bewusst nichts gesendet.
        /// </summary>
        public static bool TryClickMenu(IntPtr hWnd, bool async, params int[] path)
        {
            uint id = GetMenuId(hWnd, path);
            if (id == 0 || id == InvalidMenuId) return false;

            Send(hWnd, id, async);
            return true;
        }

        /// <summary>
        /// Klickt einen Menüpunkt anhand seiner Beschriftung, z.B.
        /// ClickMenuByText(hWnd, true, "Auswertungen", "Lager", "Teileselektion").
        /// Unempfindlich gegen verschobene Menüpositionen.
        /// </summary>
        public static bool ClickMenuByText(IntPtr hWnd, bool async, params string[] path)
        {
            int[] indexes = FindMenuPath(hWnd, path);
            return indexes != null && TryClickMenu(hWnd, async, indexes);
        }

        /// <summary>
        /// Sucht die Positionen eines Menüpfads anhand der Beschriftungen.
        /// Accelerator-Zeichen und Tastenkürzel hinter dem Tabulator werden ignoriert.
        /// Liefert null, sobald ein Eintrag nicht gefunden wird.
        /// </summary>
        public static int[] FindMenuPath(IntPtr hWnd, params string[] path)
        {
            if (path == null || path.Length == 0) return null;

            IntPtr menu = GetMenu(hWnd);
            if (menu == IntPtr.Zero) return null;

            int[] indexes = new int[path.Length];

            for (int level = 0; level < path.Length; level++)
            {
                int index = IndexOfItem(menu, path[level]);
                if (index < 0) return null;

                indexes[level] = index;
                if (level == path.Length - 1) break;

                menu = GetSubMenu(menu, index);
                if (menu == IntPtr.Zero) return null;
            }

            return indexes;
        }

        /// <summary>
        /// Ermittelt die Kommando-ID eines Menüpfads. InvalidMenuId, wenn der Pfad nicht
        /// aufgelöst werden kann.
        /// </summary>
        public static uint GetMenuId(IntPtr hWnd, int[] path)
        {
            if (path == null || path.Length == 0) return InvalidMenuId;

            IntPtr menu = GetMenu(hWnd);
            if (menu == IntPtr.Zero) return InvalidMenuId;

            for (int i = 0; i < path.Length - 1; i++)
            {
                menu = GetSubMenu(menu, path[i]);
                if (menu == IntPtr.Zero) return InvalidMenuId;
            }

            return GetMenuItemID(menu, path[path.Length - 1]);
        }

        public static void Core(IntPtr hWnd, int[] path)
        {
            TryClickMenu(hWnd, false, path);
        }

        private static void Send(IntPtr hWnd, uint id, bool async)
        {
            // Bewusst unchecked: in einem 32-Bit-Prozess wirft (IntPtr)id für große
            // Werte eine OverflowException.
            IntPtr wParam = new IntPtr(unchecked((int)id));

            if (async) ThreadPool.QueueUserWorkItem(_ => SafeSend(hWnd, wParam));
            else SendMessage(hWnd, WM_COMMAND, wParam, IntPtr.Zero);
        }

        private static void SafeSend(IntPtr hWnd, IntPtr wParam)
        {
            try
            {
                SendMessage(hWnd, WM_COMMAND, wParam, IntPtr.Zero);
            }
            catch
            {
                // Eine unbehandelte Ausnahme im ThreadPool würde den Host-Prozess beenden.
            }
        }

        private static int IndexOfItem(IntPtr menu, string text)
        {
            string wanted = Normalize(text);
            if (wanted.Length == 0) return -1;

            int count = GetMenuItemCount(menu);
            for (int i = 0; i < count; i++)
            {
                if (Normalize(GetItemText(menu, i)) == wanted) return i;
            }

            return -1;
        }

        private static string GetItemText(IntPtr menu, int index)
        {
            StringBuilder buffer = new StringBuilder(512);
            GetMenuString(menu, (uint)index, buffer, buffer.Capacity, MF_BYPOSITION);
            return buffer.ToString();
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            int shortcut = text.IndexOf('\t');
            if (shortcut >= 0) text = text.Substring(0, shortcut);

            return text.Replace("&", string.Empty).Trim().ToLowerInvariant();
        }
    }
}
