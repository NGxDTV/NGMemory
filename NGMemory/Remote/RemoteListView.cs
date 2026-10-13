using System;
using System.Collections.Generic;

namespace NGMemory.Remote
{
    public static class RemoteListView
    {
        private const uint LVM_GETITEMCOUNT = 0x1004;
        private const uint LVM_GETNEXTITEM = 0x100C;
        private const uint LVM_ENSUREVISIBLE = 0x1013;
        private const uint LVM_GETHEADER = 0x101F;
        private const uint LVM_SETITEMSTATE = 0x102B;
        private const uint LVM_GETITEMSTATE = 0x102C;
        private const uint HDM_GETITEMCOUNT = 0x1200;
        private const int LVNI_SELECTED = 0x0002;
        private const int Focused = 0x0001;
        private const int SelectedState = 0x0002;

        public static int Count(IntPtr listView)
        {
            return Math.Max(0, RemoteMessage.Number(listView, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero, 0));
        }

        public static RemoteListContent Read(IntPtr listView, int maxRows = 2000)
        {
            int rowCount = Math.Min(Count(listView), maxRows);
            int columnCount = ColumnCount(listView);

            using (RemoteListReader reader = RemoteListReader.Open(listView))
            {
                if (reader == null) return new RemoteListContent(null, null);

                var columns = new string[columnCount];
                var rows = new List<string[]>();

                for (int column = 0; column < columnCount; column++)
                {
                    if ((columns[column] = reader.Column(column)) == null) return new RemoteListContent(null, null);
                }

                for (int row = 0; row < rowCount; row++)
                {
                    var cells = new string[Math.Max(columnCount, 1)];

                    for (int column = 0; column < cells.Length; column++)
                    {
                        if ((cells[column] = reader.Cell(row, column)) == null) return new RemoteListContent(null, null);
                    }

                    rows.Add(cells);
                }

                return new RemoteListContent(columns, rows);
            }
        }

        public static int Selected(IntPtr listView)
        {
            return RemoteMessage.Number(listView, LVM_GETNEXTITEM, new IntPtr(-1), new IntPtr(LVNI_SELECTED), -1);
        }

        public static bool Select(IntPtr listView, int index)
        {
            if (index < 0 || index >= Count(listView)) return false;

            using (RemoteMemory memory = RemoteMemory.Open(listView, 256))
            {
                if (memory == null) return false;

                IntPtr result;
                int both = Focused | SelectedState;

                if (!memory.Write(RemoteListItem.Build(memory.PointerSize, RemoteListItem.State, 0, 0, 0, both, IntPtr.Zero, 0))) return false;

                if (!memory.Send(listView, LVM_SETITEMSTATE, new IntPtr(-1), out result)) return false;

                if (!memory.Write(RemoteListItem.Build(memory.PointerSize, RemoteListItem.State, 0, 0, both, both, IntPtr.Zero, 0))) return false;

                if (!memory.Send(listView, LVM_SETITEMSTATE, new IntPtr(index), out result)) return false;
            }

            RemoteMessage.Send(listView, LVM_ENSUREVISIBLE, new IntPtr(index), IntPtr.Zero);

            int state = RemoteMessage.Number(listView, LVM_GETITEMSTATE, new IntPtr(index), new IntPtr(SelectedState), 0);

            return (state & SelectedState) != 0;
        }

        private static int ColumnCount(IntPtr listView)
        {
            IntPtr header;

            if (!RemoteMessage.Send(listView, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero, out header) || header == IntPtr.Zero) return 0;

            return Math.Max(0, RemoteMessage.Number(header, HDM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero, 0));
        }
    }
}
