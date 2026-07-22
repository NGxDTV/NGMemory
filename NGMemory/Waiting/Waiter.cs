using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NGMemory.WinInteropTools;

namespace NGMemory.Waiting
{
    /// <summary>
    /// Provides cancellable polling primitives and Windows automation wait helpers.
    /// </summary>
    public static class Waiter
    {
        public static WaitResult Until(
            Func<bool> condition,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult<bool> result = ForValue(condition, value => value, options, cancellationToken);
            return WithoutValue(result);
        }

        public static async Task<WaitResult> UntilAsync(
            Func<bool> condition,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult<bool> result = await ForValueAsync(
                condition,
                value => value,
                options,
                cancellationToken).ConfigureAwait(false);

            return WithoutValue(result);
        }

        public static WaitResult<T> ForValue<T>(
            Func<T> valueProvider,
            Func<T, bool> accept,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (valueProvider == null)
                throw new ArgumentNullException(nameof(valueProvider));
            if (accept == null)
                throw new ArgumentNullException(nameof(accept));

            WaitOptions settings = WaitOptions.Snapshot(options);
            Stopwatch stopwatch = Stopwatch.StartNew();
            T lastValue = default(T);
            Exception lastError = null;
            int attempts = 0;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Result(WaitOutcome.Cancelled, lastValue, stopwatch, attempts, lastError);

                attempts++;
                try
                {
                    lastValue = valueProvider();
                    if (accept(lastValue))
                        return Result(WaitOutcome.Succeeded, lastValue, stopwatch, attempts, null);
                }
                catch (Exception error)
                {
                    if (!settings.IgnoreTransientErrors)
                        return Result(WaitOutcome.Faulted, lastValue, stopwatch, attempts, error);

                    lastError = error;
                }

                if (HasTimedOut(stopwatch, settings))
                    return Result(WaitOutcome.TimedOut, lastValue, stopwatch, attempts, lastError);

                if (WaitBeforeNextProbe(stopwatch, settings, cancellationToken))
                    return Result(WaitOutcome.Cancelled, lastValue, stopwatch, attempts, lastError);
            }
        }

        public static async Task<WaitResult<T>> ForValueAsync<T>(
            Func<T> valueProvider,
            Func<T, bool> accept,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (valueProvider == null)
                throw new ArgumentNullException(nameof(valueProvider));
            if (accept == null)
                throw new ArgumentNullException(nameof(accept));

            WaitOptions settings = WaitOptions.Snapshot(options);
            Stopwatch stopwatch = Stopwatch.StartNew();
            T lastValue = default(T);
            Exception lastError = null;
            int attempts = 0;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Result(WaitOutcome.Cancelled, lastValue, stopwatch, attempts, lastError);

                attempts++;
                try
                {
                    lastValue = valueProvider();
                    if (accept(lastValue))
                        return Result(WaitOutcome.Succeeded, lastValue, stopwatch, attempts, null);
                }
                catch (Exception error)
                {
                    if (!settings.IgnoreTransientErrors)
                        return Result(WaitOutcome.Faulted, lastValue, stopwatch, attempts, error);

                    lastError = error;
                }

                if (HasTimedOut(stopwatch, settings))
                    return Result(WaitOutcome.TimedOut, lastValue, stopwatch, attempts, lastError);

                try
                {
                    TimeSpan delay = GetNextDelay(stopwatch, settings);
                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    else
                        await Task.Yield();
                }
                catch (OperationCanceledException)
                {
                    return Result(WaitOutcome.Cancelled, lastValue, stopwatch, attempts, lastError);
                }
            }
        }

        public static WaitResult<IntPtr> ForWindow(
            WindowQuery query,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            query.Validate();
            return ForValue(
                () => FindWindow(query),
                handle => handle != IntPtr.Zero,
                options,
                cancellationToken);
        }

        public static Task<WaitResult<IntPtr>> ForWindowAsync(
            WindowQuery query,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            query.Validate();
            return ForValueAsync(
                () => FindWindow(query),
                handle => handle != IntPtr.Zero,
                options,
                cancellationToken);
        }

        public static WaitResult<IntPtr> ForChildWindow(
            IntPtr parentHandle,
            Func<IntPtr, bool> predicate,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(parentHandle, nameof(parentHandle));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return ForValue(
                () => FindChildWindow(parentHandle, predicate),
                handle => handle != IntPtr.Zero,
                options,
                cancellationToken);
        }

        public static Task<WaitResult<IntPtr>> ForChildWindowAsync(
            IntPtr parentHandle,
            Func<IntPtr, bool> predicate,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(parentHandle, nameof(parentHandle));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return ForValueAsync(
                () => FindChildWindow(parentHandle, predicate),
                handle => handle != IntPtr.Zero,
                options,
                cancellationToken);
        }

        public static WaitResult<IntPtr> ForControl(
            IntPtr parentHandle,
            int controlId,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(parentHandle, nameof(parentHandle));

            return ForValue(
                () => GuiInteropHandler.FindChildByControlId(parentHandle, controlId),
                handle => handle != IntPtr.Zero,
                options,
                cancellationToken);
        }

        public static Task<WaitResult<IntPtr>> ForControlAsync(
            IntPtr parentHandle,
            int controlId,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(parentHandle, nameof(parentHandle));

            return ForValueAsync(
                () => GuiInteropHandler.FindChildByControlId(parentHandle, controlId),
                handle => handle != IntPtr.Zero,
                options,
                cancellationToken);
        }

        public static WaitResult ForWindowClosed(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return Until(() => !User32.IsWindow(windowHandle), options, cancellationToken);
        }

        public static Task<WaitResult> ForWindowClosedAsync(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return UntilAsync(() => !User32.IsWindow(windowHandle), options, cancellationToken);
        }

        public static WaitResult ForWindowVisible(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return Until(
                () => User32.IsWindow(windowHandle) && User32.IsWindowVisible(windowHandle),
                options,
                cancellationToken);
        }

        public static Task<WaitResult> ForWindowVisibleAsync(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return UntilAsync(
                () => User32.IsWindow(windowHandle) && User32.IsWindowVisible(windowHandle),
                options,
                cancellationToken);
        }

        public static WaitResult ForWindowEnabled(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return Until(
                () => User32.IsWindow(windowHandle) && User32.IsWindowEnabled(windowHandle),
                options,
                cancellationToken);
        }

        public static Task<WaitResult> ForWindowEnabledAsync(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return UntilAsync(
                () => User32.IsWindow(windowHandle) && User32.IsWindowEnabled(windowHandle),
                options,
                cancellationToken);
        }

        public static WaitResult ForForegroundWindow(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return Until(
                () => User32.IsWindow(windowHandle) && User32.GetForegroundWindow() == windowHandle,
                options,
                cancellationToken);
        }

        public static Task<WaitResult> ForForegroundWindowAsync(
            IntPtr windowHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWindowHandle(windowHandle, nameof(windowHandle));
            return UntilAsync(
                () => User32.IsWindow(windowHandle) && User32.GetForegroundWindow() == windowHandle,
                options,
                cancellationToken);
        }

        public static WaitResult<int> ForProcess(
            string processName,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string normalizedName = NormalizeProcessName(processName);
            return ForValue(
                () => FindProcessId(normalizedName),
                processId => processId > 0,
                options,
                cancellationToken);
        }

        public static Task<WaitResult<int>> ForProcessAsync(
            string processName,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string normalizedName = NormalizeProcessName(processName);
            return ForValueAsync(
                () => FindProcessId(normalizedName),
                processId => processId > 0,
                options,
                cancellationToken);
        }

        public static WaitResult ForProcessExit(
            int processId,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (processId <= 0)
                throw new ArgumentOutOfRangeException(nameof(processId));

            return Until(() => HasProcessExited(processId), options, cancellationToken);
        }

        public static Task<WaitResult> ForProcessExitAsync(
            int processId,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (processId <= 0)
                throw new ArgumentOutOfRangeException(nameof(processId));

            return UntilAsync(() => HasProcessExited(processId), options, cancellationToken);
        }

        public static WaitResult ForSignal(
            WaitHandle waitHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (waitHandle == null)
                throw new ArgumentNullException(nameof(waitHandle));

            return Until(() => waitHandle.WaitOne(0), options, cancellationToken);
        }

        public static Task<WaitResult> ForSignalAsync(
            WaitHandle waitHandle,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (waitHandle == null)
                throw new ArgumentNullException(nameof(waitHandle));

            return UntilAsync(() => waitHandle.WaitOne(0), options, cancellationToken);
        }

        /// <summary>
        /// Executes an action once and then waits for its observable effect.
        /// The timeout starts after the action returns.
        /// </summary>
        public static WaitResult PerformAndWait(
            Action action,
            Func<bool> completionCondition,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (completionCondition == null)
                throw new ArgumentNullException(nameof(completionCondition));

            Stopwatch actionWatch = Stopwatch.StartNew();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                action();
            }
            catch (OperationCanceledException)
            {
                return new WaitResult(WaitOutcome.Cancelled, actionWatch.Elapsed, 0, null);
            }
            catch (Exception error)
            {
                return new WaitResult(WaitOutcome.Faulted, actionWatch.Elapsed, 0, error);
            }

            actionWatch.Stop();
            WaitResult waitResult = Until(completionCondition, options, cancellationToken);
            return AddElapsed(waitResult, actionWatch.Elapsed);
        }

        /// <summary>
        /// Executes an asynchronous action once and then waits for its observable effect.
        /// The timeout starts after the action completes.
        /// </summary>
        public static async Task<WaitResult> PerformAndWaitAsync(
            Func<CancellationToken, Task> action,
            Func<bool> completionCondition,
            WaitOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (completionCondition == null)
                throw new ArgumentNullException(nameof(completionCondition));

            Stopwatch actionWatch = Stopwatch.StartNew();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return new WaitResult(WaitOutcome.Cancelled, actionWatch.Elapsed, 0, null);
            }
            catch (Exception error)
            {
                return new WaitResult(WaitOutcome.Faulted, actionWatch.Elapsed, 0, error);
            }

            actionWatch.Stop();
            WaitResult waitResult = await UntilAsync(
                completionCondition,
                options,
                cancellationToken).ConfigureAwait(false);

            return AddElapsed(waitResult, actionWatch.Elapsed);
        }

        public static WaitResult Retry(
            Action action,
            Func<bool> successCheck,
            int maxAttempts = 3,
            int delayBetweenAttempts = 500,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (successCheck == null)
                throw new ArgumentNullException(nameof(successCheck));
            if (maxAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            if (delayBetweenAttempts < 0)
                throw new ArgumentOutOfRangeException(nameof(delayBetweenAttempts));

            Stopwatch stopwatch = Stopwatch.StartNew();
            Exception lastError = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return new WaitResult(WaitOutcome.Cancelled, stopwatch.Elapsed, attempt - 1, lastError);

                try
                {
                    action();
                    if (successCheck())
                        return new WaitResult(WaitOutcome.Succeeded, stopwatch.Elapsed, attempt, null);

                    lastError = null;
                }
                catch (Exception error)
                {
                    lastError = error;
                }

                if (attempt < maxAttempts && cancellationToken.WaitHandle.WaitOne(delayBetweenAttempts))
                    return new WaitResult(WaitOutcome.Cancelled, stopwatch.Elapsed, attempt, lastError);
            }

            return new WaitResult(WaitOutcome.AttemptsExhausted, stopwatch.Elapsed, maxAttempts, lastError);
        }

        public static async Task<WaitResult> RetryAsync(
            Func<CancellationToken, Task> action,
            Func<bool> successCheck,
            int maxAttempts = 3,
            int delayBetweenAttempts = 500,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (successCheck == null)
                throw new ArgumentNullException(nameof(successCheck));
            if (maxAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            if (delayBetweenAttempts < 0)
                throw new ArgumentOutOfRangeException(nameof(delayBetweenAttempts));

            Stopwatch stopwatch = Stopwatch.StartNew();
            Exception lastError = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return new WaitResult(WaitOutcome.Cancelled, stopwatch.Elapsed, attempt - 1, lastError);

                try
                {
                    await action(cancellationToken).ConfigureAwait(false);
                    if (successCheck())
                        return new WaitResult(WaitOutcome.Succeeded, stopwatch.Elapsed, attempt, null);

                    lastError = null;
                }
                catch (OperationCanceledException error)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return new WaitResult(WaitOutcome.Cancelled, stopwatch.Elapsed, attempt, lastError);

                    lastError = error;
                }
                catch (Exception error)
                {
                    lastError = error;
                }

                if (attempt >= maxAttempts)
                    continue;

                try
                {
                    await Task.Delay(delayBetweenAttempts, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return new WaitResult(WaitOutcome.Cancelled, stopwatch.Elapsed, attempt, lastError);
                }
            }

            return new WaitResult(WaitOutcome.AttemptsExhausted, stopwatch.Elapsed, maxAttempts, lastError);
        }

        public static WaitResult ForDuration(
            TimeSpan duration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration));
            if (duration.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration is too large.");

            Stopwatch stopwatch = Stopwatch.StartNew();
            bool cancelled = cancellationToken.WaitHandle.WaitOne(duration);
            return new WaitResult(
                cancelled ? WaitOutcome.Cancelled : WaitOutcome.Succeeded,
                stopwatch.Elapsed,
                0,
                null);
        }

        public static async Task<WaitResult> ForDurationAsync(
            TimeSpan duration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration));
            if (duration.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration is too large.");

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                await Task.Delay(duration, cancellationToken).ConfigureAwait(false);
                return new WaitResult(WaitOutcome.Succeeded, stopwatch.Elapsed, 0, null);
            }
            catch (OperationCanceledException)
            {
                return new WaitResult(WaitOutcome.Cancelled, stopwatch.Elapsed, 0, null);
            }
        }

        private static IntPtr FindWindow(WindowQuery query)
        {
            Process[] processes = GetProcesses(query);

            try
            {
                foreach (Process process in processes)
                {
                    List<IntPtr> handles = new List<IntPtr>();
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                            handles.Add(process.MainWindowHandle);

                        handles.AddRange(GuiInteropHandler.EnumerateProcessWindowHandles(process));
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                    }

                    foreach (IntPtr handle in handles.Distinct())
                    {
                        if (MatchesWindow(handle, query))
                            return handle;
                    }
                }
            }
            finally
            {
                foreach (Process process in processes)
                    process.Dispose();
            }

            return IntPtr.Zero;
        }

        private static Process[] GetProcesses(WindowQuery query)
        {
            if (query.ProcessId.HasValue)
            {
                try
                {
                    return new[] { Process.GetProcessById(query.ProcessId.Value) };
                }
                catch (ArgumentException)
                {
                    return new Process[0];
                }
            }

            string processName = NormalizeProcessName(query.ProcessName);
            return Process.GetProcessesByName(processName);
        }

        private static bool MatchesWindow(IntPtr handle, WindowQuery query)
        {
            if (handle == IntPtr.Zero || !User32.IsWindow(handle))
                return false;
            if (query.RequireVisible && !User32.IsWindowVisible(handle))
                return false;
            if (query.RequireEnabled && !User32.IsWindowEnabled(handle))
                return false;

            if (query.Title != null)
            {
                string actualTitle = GuiInteropHandler.GetWindowTitle(handle);
                StringComparison comparison = query.CaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                bool titleMatches;
                switch (query.TitleMatch)
                {
                    case WindowTitleMatch.Exact:
                        titleMatches = string.Equals(actualTitle, query.Title, comparison);
                        break;
                    case WindowTitleMatch.StartsWith:
                        titleMatches = actualTitle.StartsWith(query.Title, comparison);
                        break;
                    case WindowTitleMatch.EndsWith:
                        titleMatches = actualTitle.EndsWith(query.Title, comparison);
                        break;
                    default:
                        titleMatches = actualTitle.IndexOf(query.Title, comparison) >= 0;
                        break;
                }

                if (!titleMatches)
                    return false;
            }

            return query.Predicate == null || query.Predicate(handle);
        }

        private static IntPtr FindChildWindow(IntPtr parentHandle, Func<IntPtr, bool> predicate)
        {
            if (!User32.IsWindow(parentHandle))
                return IntPtr.Zero;

            IntPtr found = IntPtr.Zero;
            Exception predicateError = null;
            User32.EnumChildWindows(parentHandle, (handle, parameter) =>
            {
                try
                {
                    if (!predicate(handle))
                        return true;

                    found = handle;
                }
                catch (Exception error)
                {
                    predicateError = error;
                }

                return false;
            }, IntPtr.Zero);

            if (predicateError != null)
                throw predicateError;

            return found;
        }

        private static int FindProcessId(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            try
            {
                foreach (Process process in processes)
                {
                    try
                    {
                        return process.Id;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }

                return 0;
            }
            finally
            {
                foreach (Process process in processes)
                    process.Dispose();
            }
        }

        private static bool HasProcessExited(int processId)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                    return process.HasExited;
            }
            catch (ArgumentException)
            {
                return true;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        private static string NormalizeProcessName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                throw new ArgumentException("A process name is required.", nameof(processName));

            string normalizedName = Path.GetFileNameWithoutExtension(processName.Trim());
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("A valid process name is required.", nameof(processName));

            return normalizedName;
        }

        private static void ValidateWindowHandle(IntPtr handle, string parameterName)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("A non-zero window handle is required.", parameterName);
        }

        private static bool HasTimedOut(Stopwatch stopwatch, WaitOptions options)
        {
            return options.Timeout != System.Threading.Timeout.InfiniteTimeSpan &&
                   stopwatch.Elapsed >= options.Timeout;
        }

        private static TimeSpan GetNextDelay(Stopwatch stopwatch, WaitOptions options)
        {
            if (options.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
                return options.PollInterval;

            TimeSpan remaining = options.Timeout - stopwatch.Elapsed;
            return remaining < options.PollInterval ? remaining : options.PollInterval;
        }

        private static bool WaitBeforeNextProbe(
            Stopwatch stopwatch,
            WaitOptions options,
            CancellationToken cancellationToken)
        {
            TimeSpan delay = GetNextDelay(stopwatch, options);
            if (delay <= TimeSpan.Zero)
            {
                Thread.Yield();
                return cancellationToken.IsCancellationRequested;
            }

            return cancellationToken.WaitHandle.WaitOne(delay);
        }

        private static WaitResult<T> Result<T>(
            WaitOutcome outcome,
            T value,
            Stopwatch stopwatch,
            int attempts,
            Exception error)
        {
            return new WaitResult<T>(outcome, value, stopwatch.Elapsed, attempts, error);
        }

        private static WaitResult WithoutValue<T>(WaitResult<T> result)
        {
            return new WaitResult(result.Outcome, result.Elapsed, result.Attempts, result.Error);
        }

        private static WaitResult AddElapsed(WaitResult result, TimeSpan elapsed)
        {
            return new WaitResult(result.Outcome, result.Elapsed + elapsed, result.Attempts, result.Error);
        }
    }
}
