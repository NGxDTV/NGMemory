using System;
using System.Diagnostics;
using System.Threading;

namespace NGMemory.Remote
{
    internal static class RemotePoll
    {
        internal static bool Until(Func<bool> condition, int timeoutMs)
        {
            var watch = Stopwatch.StartNew();

            while (true)
            {
                if (condition()) return true;

                if (watch.ElapsedMilliseconds >= timeoutMs) return false;

                Thread.Sleep(20);
            }
        }
    }
}
