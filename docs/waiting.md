# Waiting And Actions

NGMemory 1.1.0 provides two waiting layers:

- `NGMemory.Easy.EasyWait` returns simple `bool`, `IntPtr`, or process ID values.
- `NGMemory.Waiting.Waiter` returns `WaitResult` diagnostics with outcome, elapsed time, probe count, last value, and last error.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using NGMemory.Easy;
using NGMemory.Waiting;
```

## Easy Window Template

```csharp
IntPtr window = EasyWait.ForWindow(
    processName: "notepad",
    partialTitle: "Notes",
    timeout: 15000,
    checkInterval: 100);

if (window == IntPtr.Zero)
    return;

EasyWindow.FocusWindow(window);
```

`ForWindow` probes immediately. The process name accepts values with or without `.exe`; the optional title is matched as a case-insensitive substring.

## Child Windows And Controls

```csharp
IntPtr dialog = EasyWait.ForChildWindow(window, "Settings", timeout: 5000);
IntPtr saveButton = EasyWait.ForControl(dialog, controlId: 1001, timeout: 5000);
```

## Window State

```csharp
EasyWait.ForWindowVisible(window, timeout: 3000);
EasyWait.ForWindowEnabled(window, timeout: 3000);
EasyWait.ForForegroundWindow(window, timeout: 3000);
EasyWait.ForWindowClosed(window, timeout: 30000);
```

Every method also has an `Async` counterpart.

## Perform An Action And Wait For Its Effect

```csharp
bool saved = EasyWait.DoAndWait(
    action: () => EasyButton.Click(dialog, 1001),
    completionCondition: () => EasyTextBox.GetText(dialog, 2001) == "Saved",
    timeout: 10000,
    checkInterval: 100);
```

The action runs once. The timeout starts after the action returns and applies to the completion condition. Use `RetryUntilSuccess` only when the action itself is safe to repeat.

## Core Result Template

```csharp
var options = new WaitOptions
{
    Timeout = TimeSpan.FromSeconds(15),
    PollInterval = TimeSpan.FromMilliseconds(75)
};

var query = new WindowQuery
{
    ProcessName = "notepad",
    Title = "Notes.txt - Notepad",
    TitleMatch = WindowTitleMatch.Exact,
    RequireVisible = true,
    RequireEnabled = true
};

WaitResult<IntPtr> result = Waiter.ForWindow(query, options);
if (result.Succeeded)
    Console.WriteLine(result.Value);
else
    Console.WriteLine($"{result.Outcome} after {result.Elapsed}");
```

`WaitOutcome` values are `Succeeded`, `TimedOut`, `Cancelled`, `Faulted`, and `AttemptsExhausted`.

## Conditions And Values

```csharp
WaitResult<string> status = Waiter.ForValue(
    () => EasyTextBox.GetText(dialog, 1001),
    value => value == "Completed",
    new WaitOptions
    {
        Timeout = TimeSpan.FromSeconds(20),
        PollInterval = TimeSpan.FromMilliseconds(150),
        IgnoreTransientErrors = true
    });
```

Only enable `IgnoreTransientErrors` for probes that may legitimately fail while a foreign process rebuilds its UI.

## Processes And Signals

```csharp
int processId = EasyWait.ForProcess("myapp.exe", timeout: 30000);
if (processId != 0)
    EasyWait.ForProcessExit(processId, timeout: 60000);

using (var signal = new ManualResetEvent(false))
{
    bool received = EasyWait.ForSignal(signal, timeout: 5000);
}
```

## Async And Cancellation

```csharp
using (var cancellation = new CancellationTokenSource())
{
    IntPtr window = await EasyWait.ForWindowAsync(
        "myapp",
        partialTitle: "Login",
        timeout: 20000,
        checkInterval: 100,
        cancellationToken: cancellation.Token);
}
```

Use asynchronous methods in WinForms event handlers so the UI thread remains responsive. Infinite core waits use `Timeout.InfiniteTimeSpan` and should always receive a cancellation token.

## API Map

- Polling: `Until`, `UntilAsync`, `ForValue`, `ForValueAsync`
- Windows: `ForWindow`, `ForChildWindow`, `ForControl`, plus async variants
- Window state: `ForWindowClosed`, `ForWindowVisible`, `ForWindowEnabled`, `ForForegroundWindow`, plus async variants
- Processes/signals: `ForProcess`, `ForProcessExit`, `ForSignal`, plus async variants
- Actions: `PerformAndWait`, `PerformAndWaitAsync`, `Retry`, `RetryAsync`
- Delay: `ForDuration`, `ForDurationAsync`

See the bilingual HTML chapter at `NGMemoryDoc/pages/waiting.html` for all templates, explanations, and common mistakes.
