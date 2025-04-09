using System;

namespace NGMemory.Easy
{
    /// <summary>
    /// Schnelle Hilfsfunktionen für Debugging / Breakpoints.
    /// </summary>
    public static class EasyDebugHook
    {
        /// <summary>
        /// Wartet auf einen Hardware-Breakpoint an der angegebenen Adresse
        /// und gibt den Wert eines Registers im Trefferfall zurück.
        /// </summary>
        public static ulong WaitForRegister(string processName, IntPtr targetAddress, NGMemory.Enums.Register register)
        {
            return NGMemory.DebugHook.WaitForRegister(processName, targetAddress, register);
        }

        /// <summary>
        /// Wartet auf einen Hardware-Breakpoint in einem Prozess
        /// (per Prozess-ID) und gibt den Wert eines Registers zurück.
        /// </summary>
        public static ulong WaitForRegister(int processID, IntPtr targetAddress, NGMemory.Enums.Register register)
        {
            return NGMemory.DebugHook.WaitForRegister(processID, targetAddress, register);
        }
    }
}
