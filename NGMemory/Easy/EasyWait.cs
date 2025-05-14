using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NGMemory.Easy
{
    /// <summary>
    /// Hilfsfunktionen für das Warten auf Bedingungen und Ereignisse.
    /// </summary>
    public static class EasyWait
    {
        /// <summary>
        /// Wartet, bis eine Bedingung erfüllt ist oder die Zeitbegrenzung erreicht ist.
        /// </summary>
        /// <param name="condition">Die zu überprüfende Bedingung.</param>
        /// <param name="timeout">Timeout in Millisekunden.</param>
        /// <param name="checkInterval">Intervall zwischen den Überprüfungen.</param>
        /// <returns>True, wenn die Bedingung erfüllt ist, False bei Timeout.</returns>
        public static bool Until(Func<bool> condition, int timeout = 10000, int checkInterval = 100)
        {
            DateTime endTime = DateTime.Now.AddMilliseconds(timeout);
            
            while (DateTime.Now < endTime)
            {
                if (condition())
                    return true;
                
                Thread.Sleep(checkInterval);
            }
            
            return false;
        }

        /// <summary>
        /// Wartet für eine bestimmte Zeit.
        /// </summary>
        public static void ForDuration(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Wiederholt eine Aktion, bis sie erfolgreich ist oder die Zeitbegrenzung erreicht wird.
        /// </summary>
        public static bool RetryUntilSuccess(Action action, Func<bool> successCheck, 
                                             int maxAttempts = 3, int delayBetweenAttempts = 500)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    action();
                    if (successCheck())
                        return true;
                }
                catch
                {
                    // Fehler abfangen und nächsten Versuch starten
                }
                
                if (attempt < maxAttempts - 1)
                    Thread.Sleep(delayBetweenAttempts);
            }
            
            return false;
        }
    }
}
