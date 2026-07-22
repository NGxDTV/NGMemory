using System;
using System.Threading;

namespace NGMemory.Waiting
{
    /// <summary>
    /// Controls timeout, polling, and transient error handling for wait operations.
    /// </summary>
    public sealed class WaitOptions
    {
        public WaitOptions()
        {
            Timeout = TimeSpan.FromSeconds(10);
            PollInterval = TimeSpan.FromMilliseconds(100);
        }

        /// <summary>
        /// Maximum wait duration. Use <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>
        /// to wait indefinitely and combine it with a cancellation token.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Delay between two probes.
        /// </summary>
        public TimeSpan PollInterval { get; set; }

        /// <summary>
        /// Continues polling when a probe throws and exposes the last error on timeout.
        /// </summary>
        public bool IgnoreTransientErrors { get; set; }

        internal static WaitOptions Snapshot(WaitOptions options)
        {
            WaitOptions source = options ?? new WaitOptions();

            if (source.Timeout < TimeSpan.Zero && source.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
                throw new ArgumentOutOfRangeException(nameof(options), "Timeout must be non-negative or infinite.");

            if (source.PollInterval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(options), "PollInterval must be non-negative.");

            if (source.PollInterval.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(options), "PollInterval is too large.");

            return new WaitOptions
            {
                Timeout = source.Timeout,
                PollInterval = source.PollInterval,
                IgnoreTransientErrors = source.IgnoreTransientErrors
            };
        }
    }
}
