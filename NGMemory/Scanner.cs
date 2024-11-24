using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using static NGMemory.Kernel32;
using System.Threading;
using System.Threading.Tasks;

namespace NGMemory
{
    public class Scanner
    {
        private Process targetProcess;
        private IntPtr processHandle;

        /// <summary>
        /// Initializes a new instance of the Scanner class to scan the memory of a specific process.
        /// Opens a handle to the target process with the required permissions for memory reading and querying.
        /// </summary>
        /// <param name="process">The target process to scan.</param>
        public Scanner(Process process)
        {
            targetProcess = process;
            processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, targetProcess.Id);
        }

        /// <summary>
        /// Converts a string pattern into a byte array and a mask array.
        /// Each element in the string, separated by spaces, represents a byte.
        /// If the element is "??" or "?", it is treated as a wildcard (mask is false, byte is 0).
        /// Otherwise, it is converted to a byte (mask is true, and byte is the parsed value).
        /// </summary>
        /// <param name="pattern">The string pattern containing byte representations or wildcards.</param>
        /// <param name="bytes">The output byte array parsed from the pattern.</param>
        /// <param name="mask">The output boolean array indicating which bytes are fixed (true) or wildcards (false).</param>

        public void ConvertStringToBytes(string pattern, out byte[] bytes, out bool[] mask)
        {
            string[] elements = pattern.Split(' ');
            bytes = new byte[elements.Length];
            mask = new bool[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == "??" || elements[i] == "?")
                {
                    mask[i] = false;
                    bytes[i] = 0;
                }
                else
                {
                    mask[i] = true;
                    bytes[i] = Convert.ToByte(elements[i], 16);
                }
            }
        }

        /// <summary>
        /// Scans the memory of a process for a specific byte pattern.
        /// Converts the pattern into bytes and a mask, then searches memory regions 
        /// between specified or default addresses for the matching sequence.
        /// </summary>
        /// <param name="pattern">The pattern to search for, with bytes and wildcards ("??" or "?").</param>
        /// <param name="baseAddress">Optional base address to start the scan. Defaults to system-defined minimum.</param>
        /// <param name="maxMin">Optional range to extend above and below the base address for scanning.</param>
        /// <returns>
        /// Returns the first address where the pattern is found, or null if not found.
        /// </returns>

        public IntPtr? ScanMemory(string pattern, long? baseAddress = null, long? maxMin = null)
        {
            ConvertStringToBytes(pattern, out byte[] signature, out bool[] mask);

            SYSTEM_INFO sysInfo = new SYSTEM_INFO();
            GetSystemInfo(out sysInfo);

            IntPtr minAddress = baseAddress.HasValue
                ? new IntPtr(Math.Max(sysInfo.lpMinimumApplicationAddress.ToInt64(), baseAddress.Value - (maxMin ?? 0)))
                : sysInfo.lpMinimumApplicationAddress;

            IntPtr maxAddress = baseAddress.HasValue
                ? new IntPtr(Math.Min(sysInfo.lpMaximumApplicationAddress.ToInt64(), baseAddress.Value + (maxMin ?? 0)))
                : sysInfo.lpMaximumApplicationAddress;

            List<MEMORY_BASIC_INFORMATION> memoryRegions = new List<MEMORY_BASIC_INFORMATION>();

            IntPtr currentAddress = minAddress;
            while (currentAddress.ToInt64() < maxAddress.ToInt64())
            {
                MEMORY_BASIC_INFORMATION mbi = new MEMORY_BASIC_INFORMATION();
                UIntPtr mbiSize = (UIntPtr)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));
                UIntPtr result = VirtualQueryEx(processHandle, currentAddress, out mbi, mbiSize);

                if (result == UIntPtr.Zero) break;

                bool isReadable = mbi.State == MEM_COMMIT && (mbi.Protect & PAGE_GUARD) == 0 && (mbi.Protect & PAGE_NOACCESS) == 0 && IsReadableProtection(mbi.Protect);
                if (isReadable) memoryRegions.Add(mbi);

                currentAddress = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
            }

            IntPtr? foundAddress = null;
            foreach (var mbi in memoryRegions)
            {
                long regionSize = mbi.RegionSize.ToInt64();
                IntPtr regionBase = mbi.BaseAddress;

                const int bufferSize = 0x10000;
                long bytesReadTotal = 0;

                while (bytesReadTotal < regionSize)
                {
                    IntPtr address = new IntPtr(regionBase.ToInt64() + bytesReadTotal);
                    int bytesToRead = (int)Math.Min(bufferSize, regionSize - bytesReadTotal);

                    byte[] buffer = new byte[bytesToRead];
                    if (ReadProcessMemory(processHandle, address, buffer, bytesToRead, out IntPtr bytesRead) && bytesRead.ToInt32() > 0)
                    {
                        for (int i = 0; i <= bytesRead.ToInt32() - signature.Length; i++)
                        {
                            bool match = true;
                            for (int j = 0; j < signature.Length; j++)
                            {
                                if (mask[j] && buffer[i + j] != signature[j]) { match = false; break; }
                            }
                            if (match)
                            {
                                foundAddress = new IntPtr(address.ToInt64() + i);
                                return foundAddress;
                            }
                        }
                    }
                    bytesReadTotal += bytesToRead;
                }
            }

            return foundAddress;
        }

        private bool IsReadableProtection(uint protect)
        {
            return protect == PAGE_READONLY ||
                   protect == PAGE_READWRITE ||
                   protect == PAGE_WRITECOPY ||
                   protect == PAGE_EXECUTE_READ ||
                   protect == PAGE_EXECUTE_READWRITE ||
                   protect == PAGE_EXECUTE_WRITECOPY;
        }
    }
}
