# NGMemory v1.1.0 Release Notes

Version 1.1.0 adds a complete waiting layer for Windows automation.

## Added

- `NGMemory.Waiting.Waiter` for conditions, values, windows, controls, window states, processes, signals, actions, retries, and delays
- `WaitOptions` for timeout, polling interval, and transient error handling
- `WaitResult` and `WaitResult<T>` with outcome, elapsed time, attempts, last error, and value
- `WaitOutcome` values for success, timeout, cancellation, failure, and exhausted attempts
- `WindowQuery` with process/title matching, visibility, enabled state, and custom predicates
- Synchronous and asynchronous variants with `CancellationToken` support
- Expanded `EasyWait` methods for the same common workflows

## Compatibility

Existing `EasyWait.Until`, `ForDuration`, and `RetryUntilSuccess` calls remain source-compatible. The project continues to target .NET Framework 4.7.2.

## Documentation

- [Waiting And Actions](waiting.md)
- Bilingual HTML chapter: `NGMemoryDoc/pages/waiting.html`
