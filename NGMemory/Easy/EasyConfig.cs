using System;

namespace NGMemory.Easy
{
    /// <summary>
    /// Zentrale Konfiguration für die Easy-Klassen.
    /// </summary>
    public static class EasyConfig
    {
        /// <summary>
        /// Standardverzögerung zwischen Tastendrücken in Millisekunden.
        /// </summary>
        public static int DefaultKeyDelay { get; set; } = 10;
        
        /// <summary>
        /// Standardverzögerung zwischen Mausbewegungen in Millisekunden.
        /// </summary>
        public static int DefaultMouseDelay { get; set; } = 50;
        
        /// <summary>
        /// Aktiviert detaillierte Fehlerprotokollierung.
        /// </summary>
        public static bool EnableVerboseLogging { get; set; } = false;
        
        /// <summary>
        /// Aktiviert die automatische Wiederholung bei Fehlern.
        /// </summary>
        public static bool EnableAutoRetry { get; set; } = false;
        
        /// <summary>
        /// Maximale Anzahl von Wiederholungsversuchen.
        /// </summary>
        public static int MaxRetryCount { get; set; } = 3;
    }
}