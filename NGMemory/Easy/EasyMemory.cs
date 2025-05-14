using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGMemory.Easy
{
    /// <summary>
    /// Vereinfachte Speicheroperationen für NGMemory.
    /// </summary>
    public static class EasyMemory
    {
        /// <summary>
        /// Findet ein Muster im Speicher eines Prozesses und gibt die Adresse zurück.
        /// </summary>
        /// <param name="processName">Name des Prozesses</param>
        /// <param name="pattern">Bytemuster (z.B. "90 90 ?? 90" wobei ?? wildcards sind)</param>
        /// <param name="startAddress">Startadresse für die Suche</param>
        /// <param name="endAddress">Endadresse für die Suche</param>
        /// <returns>Gefundene Adresse oder IntPtr.Zero wenn nicht gefunden</returns>
        public static IntPtr FindPattern(string processName, string pattern, IntPtr startAddress = default, IntPtr endAddress = default)
        {
            IntPtr result = IntPtr.Zero;
            
            try
            {
                // Prozess öffnen
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                if (processes.Length == 0) return IntPtr.Zero;
                
                var process = processes[0];
                byte[] patternBytes = ParsePattern(pattern, out byte[] mask);
                
                // Standard-Suchbereich falls nicht angegeben
                if (startAddress == IntPtr.Zero) startAddress = process.MainModule.BaseAddress;
                if (endAddress == IntPtr.Zero) endAddress = new IntPtr(startAddress.ToInt64() + process.MainModule.ModuleMemorySize);
                
                // Speicher in Blöcken scannen
                int blockSize = 4096;
                long currentAddress = startAddress.ToInt64();
                
                while (currentAddress < endAddress.ToInt64())
                {
                    int bytesToRead = (int)Math.Min(blockSize, endAddress.ToInt64() - currentAddress);
                    byte[] buffer = new byte[bytesToRead];

                    NGMemory.Kernel32.ReadProcessMemory(
                        process.Handle,
                        new IntPtr(currentAddress),
                        buffer,
                        bytesToRead,
                        out IntPtr bytesReadPtr);

                    if (bytesReadPtr.ToInt32() > 0)
                    {
                        uint bytesRead = (uint)bytesReadPtr.ToInt32();
                        // Muster im aktuellen Block suchen
                        for (int i = 0; i <= bytesRead - patternBytes.Length; i++)
                        {
                            bool found = true;

                            for (int j = 0; j < patternBytes.Length; j++)
                            {
                                if (mask[j] == 0) continue; // Wildcard überspringen

                                if (buffer[i + j] != patternBytes[j])
                                {
                                    found = false;
                                    break;
                                }
                            }

                            if (found)
                            {
                                return new IntPtr(currentAddress + i);
                            }
                        }
                    }
                    
                    currentAddress += bytesReadPtr.ToInt32();
                }
            }
            catch
            {
                // Fehlerbehandlung
            }
            
            return result;
        }
        
        /// <summary>
        /// Liest einen String aus dem Speicher eines Prozesses.
        /// </summary>
        public static string ReadString(string processName, IntPtr address, int maxLength = 1024, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            
            var processes = System.Diagnostics.Process.GetProcessesByName(processName);
            if (processes.Length == 0) return string.Empty;
            
            var process = processes[0];
            byte[] buffer = new byte[maxLength];
            
            NGMemory.Kernel32.ReadProcessMemory(
                process.Handle, 
                address, 
                buffer, 
                (uint)maxLength, 
                out uint bytesRead);
            
            if (bytesRead > 0)
            {
                // Nach Nullterminierung suchen
                int length = 0;
                while (length < bytesRead && buffer[length] != 0) length++;
                
                return encoding.GetString(buffer, 0, length);
            }
            
            return string.Empty;
        }
        
        private static byte[] ParsePattern(string pattern, out byte[] mask)
        {
            string[] parts = pattern.Split(' ');
            byte[] result = new byte[parts.Length];
            mask = new byte[parts.Length];
            
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "??" || parts[i] == "?")
                {
                    result[i] = 0;
                    mask[i] = 0; // Wildcard
                }
                else
                {
                    result[i] = Convert.ToByte(parts[i], 16);
                    mask[i] = 1; // Exakter Wert
                }
            }
            
            return result;
        }
    }
}
