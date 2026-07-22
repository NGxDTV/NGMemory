using System;

namespace NGMemory.Waiting
{
    /// <summary>
    /// Contains the outcome and diagnostics of a wait operation.
    /// </summary>
    public class WaitResult
    {
        internal WaitResult(
            WaitOutcome outcome,
            TimeSpan elapsed,
            int attempts,
            Exception error)
        {
            Outcome = outcome;
            Elapsed = elapsed;
            Attempts = attempts;
            Error = error;
        }

        public WaitOutcome Outcome { get; }
        public bool Succeeded => Outcome == WaitOutcome.Succeeded;
        public bool TimedOut => Outcome == WaitOutcome.TimedOut;
        public bool Cancelled => Outcome == WaitOutcome.Cancelled;
        public TimeSpan Elapsed { get; }
        public int Attempts { get; }
        public Exception Error { get; }

        /// <summary>
        /// Throws an exception when the operation did not succeed.
        /// </summary>
        public void ThrowIfFailed()
        {
            if (Succeeded)
                return;

            if (Cancelled)
                throw new OperationCanceledException("The wait operation was cancelled.");

            if (Outcome == WaitOutcome.TimedOut)
                throw new TimeoutException("The wait operation timed out.", Error);

            if (Outcome == WaitOutcome.AttemptsExhausted)
                throw new InvalidOperationException("All retry attempts were exhausted.", Error);

            throw new InvalidOperationException("The wait operation failed.", Error);
        }
    }

    /// <summary>
    /// Contains the last probed value in addition to the wait diagnostics.
    /// </summary>
    public sealed class WaitResult<T> : WaitResult
    {
        internal WaitResult(
            WaitOutcome outcome,
            T value,
            TimeSpan elapsed,
            int attempts,
            Exception error)
            : base(outcome, elapsed, attempts, error)
        {
            Value = value;
        }

        public T Value { get; }

        public T GetValueOrThrow()
        {
            ThrowIfFailed();
            return Value;
        }
    }
}
