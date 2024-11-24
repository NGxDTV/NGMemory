using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGMemory
{
    public class Module
    {
        /// <summary>
        /// Retrieves the base address of a specified module within a process.
        /// Searches for the module in the specified process by name and returns its base address if found.
        /// </summary>
        /// <param name="moduleName">The name of the module to locate.</param>
        /// <param name="processName">The name of the process containing the module.</param>
        /// <returns>The base address of the module if found, otherwise IntPtr.Zero.</returns>

        public IntPtr getModuleBaseAddress(string moduleName, string processName)
        {
            Process GameProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            if (GameProcess == null) return IntPtr.Zero;

            foreach (ProcessModule module in GameProcess.Modules)
            {
                if (module.ModuleName == moduleName)
                {
                    return module.BaseAddress;
                }
            }
            return IntPtr.Zero;
        }
    }
}
