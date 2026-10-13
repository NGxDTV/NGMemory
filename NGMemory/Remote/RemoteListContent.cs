using System;
using System.Collections.Generic;

namespace NGMemory.Remote
{
    public sealed class RemoteListContent
    {
        public RemoteListContent(string[] columns, IReadOnlyList<string[]> rows)
        {
            Columns = columns ?? new string[0];
            Rows = rows ?? new List<string[]>();
        }

        public string[] Columns { get; }

        public IReadOnlyList<string[]> Rows { get; }

        public int ColumnIndex(string header)
        {
            string wanted = (header ?? string.Empty).Trim();

            if (wanted.Length == 0) return -1;

            int exact = Array.FindIndex(Columns, column => string.Equals((column ?? string.Empty).Trim(), wanted, StringComparison.OrdinalIgnoreCase));

            if (exact >= 0) return exact;

            return Array.FindIndex(Columns, column => (column ?? string.Empty).IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public string Cell(int row, int column)
        {
            if (row < 0 || row >= Rows.Count) return string.Empty;

            string[] cells = Rows[row];

            if (cells == null || column < 0 || column >= cells.Length) return string.Empty;

            return cells[column] ?? string.Empty;
        }
    }
}
