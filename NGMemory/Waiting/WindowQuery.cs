using System;

namespace NGMemory.Waiting
{
    public enum WindowTitleMatch
    {
        Contains,
        Exact,
        StartsWith,
        EndsWith
    }

    /// <summary>
    /// Describes a top-level window to wait for.
    /// </summary>
    public sealed class WindowQuery
    {
        public WindowQuery()
        {
            TitleMatch = WindowTitleMatch.Contains;
        }

        public WindowQuery(string processName, string title = null)
            : this()
        {
            ProcessName = processName;
            Title = title;
        }

        public string ProcessName { get; set; }
        public int? ProcessId { get; set; }
        public string Title { get; set; }
        public WindowTitleMatch TitleMatch { get; set; }
        public bool CaseSensitive { get; set; }
        public bool RequireVisible { get; set; }
        public bool RequireEnabled { get; set; }
        public Func<IntPtr, bool> Predicate { get; set; }

        internal void Validate()
        {
            if (!ProcessId.HasValue && string.IsNullOrWhiteSpace(ProcessName))
                throw new InvalidOperationException("ProcessName or ProcessId must be provided.");

            if (ProcessId.HasValue && ProcessId.Value <= 0)
                throw new InvalidOperationException("ProcessId must be greater than zero.");
        }
    }
}
