using System;
using System.Threading;
using System.Threading.Tasks;
using NGMemory.Waiting;
using NGMemory.WinInteropTools;

namespace NGMemory.Easy
{
    /// <summary>
    /// Einfache Helfer für Bedingungen, Fenster, Prozesse und Aktionsabläufe.
    /// Für detaillierte Ergebnisse steht NGMemory.Waiting.Waiter zur Verfügung.
    /// </summary>
    public static class EasyWait
    {
        /// <summary>
        /// Wartet, bis eine Bedingung erfüllt ist oder das Zeitlimit erreicht wurde.
        /// </summary>
        public static bool Until(
            Func<bool> condition,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.Until(
                condition,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        /// <summary>
        /// Wartet asynchron, bis eine Bedingung erfüllt ist.
        /// </summary>
        public static async Task<bool> UntilAsync(
            Func<bool> condition,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult result = await Waiter.UntilAsync(
                condition,
                CreateOptions(timeout, checkInterval),
                cancellationToken).ConfigureAwait(false);

            return result.Succeeded;
        }

        /// <summary>
        /// Wartet für eine bestimmte Zeit.
        /// </summary>
        public static void ForDuration(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Wartet asynchron für eine bestimmte Zeit und unterstützt Abbruch.
        /// </summary>
        public static async Task<bool> ForDurationAsync(
            int milliseconds,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult result = await Waiter.ForDurationAsync(
                TimeSpan.FromMilliseconds(milliseconds),
                cancellationToken).ConfigureAwait(false);

            return result.Succeeded;
        }

        /// <summary>
        /// Wartet auf ein Top-Level-Fenster eines Prozesses und gibt dessen Handle zurück.
        /// </summary>
        public static IntPtr ForWindow(
            string processName,
            string partialTitle = null,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult<IntPtr> result = Waiter.ForWindow(
                new WindowQuery(processName, partialTitle),
                CreateOptions(timeout, checkInterval),
                cancellationToken);

            return result.Succeeded ? result.Value : IntPtr.Zero;
        }

        /// <summary>
        /// Wartet asynchron auf ein Top-Level-Fenster eines Prozesses.
        /// </summary>
        public static async Task<IntPtr> ForWindowAsync(
            string processName,
            string partialTitle = null,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult<IntPtr> result = await Waiter.ForWindowAsync(
                new WindowQuery(processName, partialTitle),
                CreateOptions(timeout, checkInterval),
                cancellationToken).ConfigureAwait(false);

            return result.Succeeded ? result.Value : IntPtr.Zero;
        }

        /// <summary>
        /// Wartet auf ein Kindfenster, dessen Titel den Suchtext enthält.
        /// </summary>
        public static IntPtr ForChildWindow(
            IntPtr parentHandle,
            string partialTitle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (partialTitle == null)
                throw new ArgumentNullException(nameof(partialTitle));

            WaitResult<IntPtr> result = Waiter.ForChildWindow(
                parentHandle,
                handle => GuiInteropHandler.GetWindowTitle(handle)
                    .IndexOf(partialTitle, StringComparison.OrdinalIgnoreCase) >= 0,
                CreateOptions(timeout, checkInterval),
                cancellationToken);

            return result.Succeeded ? result.Value : IntPtr.Zero;
        }

        public static Task<IntPtr> ForChildWindowAsync(
            IntPtr parentHandle,
            string partialTitle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (partialTitle == null)
                throw new ArgumentNullException(nameof(partialTitle));

            return WindowHandleAsync(Waiter.ForChildWindowAsync(
                parentHandle,
                handle => GuiInteropHandler.GetWindowTitle(handle)
                    .IndexOf(partialTitle, StringComparison.OrdinalIgnoreCase) >= 0,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        /// <summary>
        /// Wartet auf ein klassisches Win32-Control anhand seiner Control-ID.
        /// </summary>
        public static IntPtr ForControl(
            IntPtr parentHandle,
            int controlId,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult<IntPtr> result = Waiter.ForControl(
                parentHandle,
                controlId,
                CreateOptions(timeout, checkInterval),
                cancellationToken);

            return result.Succeeded ? result.Value : IntPtr.Zero;
        }

        public static Task<IntPtr> ForControlAsync(
            IntPtr parentHandle,
            int controlId,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return WindowHandleAsync(Waiter.ForControlAsync(
                parentHandle,
                controlId,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        /// <summary>
        /// Wartet, bis ein Fenster geschlossen beziehungsweise sein Handle ungültig wird.
        /// </summary>
        public static bool ForWindowClosed(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.ForWindowClosed(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        public static Task<bool> ForWindowClosedAsync(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SuccessAsync(Waiter.ForWindowClosedAsync(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        public static bool ForWindowVisible(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.ForWindowVisible(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        public static Task<bool> ForWindowVisibleAsync(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SuccessAsync(Waiter.ForWindowVisibleAsync(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        public static bool ForWindowEnabled(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.ForWindowEnabled(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        public static Task<bool> ForWindowEnabledAsync(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SuccessAsync(Waiter.ForWindowEnabledAsync(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        public static bool ForForegroundWindow(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.ForForegroundWindow(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        public static Task<bool> ForForegroundWindowAsync(
            IntPtr windowHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SuccessAsync(Waiter.ForForegroundWindowAsync(
                windowHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        /// <summary>
        /// Wartet auf den Start eines Prozesses und gibt seine Prozess-ID zurück.
        /// </summary>
        public static int ForProcess(
            string processName,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult<int> result = Waiter.ForProcess(
                processName,
                CreateOptions(timeout, checkInterval),
                cancellationToken);

            return result.Succeeded ? result.Value : 0;
        }

        public static Task<int> ForProcessAsync(
            string processName,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ProcessIdAsync(Waiter.ForProcessAsync(
                processName,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        /// <summary>
        /// Wartet, bis ein Prozess beendet wurde.
        /// </summary>
        public static bool ForProcessExit(
            int processId,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.ForProcessExit(
                processId,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        public static Task<bool> ForProcessExitAsync(
            int processId,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SuccessAsync(Waiter.ForProcessExitAsync(
                processId,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        /// <summary>
        /// Wartet auf ein WaitHandle, beispielsweise ManualResetEvent oder Semaphore.
        /// </summary>
        public static bool ForSignal(
            WaitHandle waitHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.ForSignal(
                waitHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        public static Task<bool> ForSignalAsync(
            WaitHandle waitHandle,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SuccessAsync(Waiter.ForSignalAsync(
                waitHandle,
                CreateOptions(timeout, checkInterval),
                cancellationToken));
        }

        /// <summary>
        /// Führt eine Aktion einmal aus und wartet anschließend auf ihre sichtbare Wirkung.
        /// </summary>
        public static bool DoAndWait(
            Action action,
            Func<bool> completionCondition,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.PerformAndWait(
                action,
                completionCondition,
                CreateOptions(timeout, checkInterval),
                cancellationToken).Succeeded;
        }

        /// <summary>
        /// Führt eine asynchrone Aktion aus und wartet anschließend auf ihre Wirkung.
        /// </summary>
        public static async Task<bool> DoAndWaitAsync(
            Func<CancellationToken, Task> action,
            Func<bool> completionCondition,
            int timeout = 10000,
            int checkInterval = 100,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitResult result = await Waiter.PerformAndWaitAsync(
                action,
                completionCondition,
                CreateOptions(timeout, checkInterval),
                cancellationToken).ConfigureAwait(false);

            return result.Succeeded;
        }

        /// <summary>
        /// Wiederholt eine Aktion, bis die Erfolgskontrolle true liefert.
        /// </summary>
        public static bool RetryUntilSuccess(
            Action action,
            Func<bool> successCheck,
            int maxAttempts = 3,
            int delayBetweenAttempts = 500,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Waiter.Retry(
                action,
                successCheck,
                maxAttempts,
                delayBetweenAttempts,
                cancellationToken).Succeeded;
        }

        public static Task<bool> RetryUntilSuccessAsync(
            Func<CancellationToken, Task> action,
            Func<bool> successCheck,
            int maxAttempts = 3,
            int delayBetweenAttempts = 500,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SuccessAsync(Waiter.RetryAsync(
                action,
                successCheck,
                maxAttempts,
                delayBetweenAttempts,
                cancellationToken));
        }

        private static async Task<bool> SuccessAsync(Task<WaitResult> waitTask)
        {
            WaitResult result = await waitTask.ConfigureAwait(false);
            return result.Succeeded;
        }

        private static async Task<IntPtr> WindowHandleAsync(Task<WaitResult<IntPtr>> waitTask)
        {
            WaitResult<IntPtr> result = await waitTask.ConfigureAwait(false);
            return result.Succeeded ? result.Value : IntPtr.Zero;
        }

        private static async Task<int> ProcessIdAsync(Task<WaitResult<int>> waitTask)
        {
            WaitResult<int> result = await waitTask.ConfigureAwait(false);
            return result.Succeeded ? result.Value : 0;
        }

        private static WaitOptions CreateOptions(int timeout, int checkInterval)
        {
            if (timeout < Timeout.Infinite)
                throw new ArgumentOutOfRangeException(nameof(timeout));
            if (checkInterval < 0)
                throw new ArgumentOutOfRangeException(nameof(checkInterval));

            return new WaitOptions
            {
                Timeout = timeout == Timeout.Infinite
                    ? Timeout.InfiniteTimeSpan
                    : TimeSpan.FromMilliseconds(timeout),
                PollInterval = TimeSpan.FromMilliseconds(checkInterval)
            };
        }
    }
}
