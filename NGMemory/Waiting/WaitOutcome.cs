namespace NGMemory.Waiting
{
    /// <summary>
    /// Describes how a wait operation ended.
    /// </summary>
    public enum WaitOutcome
    {
        Succeeded,
        TimedOut,
        Cancelled,
        Faulted,
        AttemptsExhausted
    }
}
