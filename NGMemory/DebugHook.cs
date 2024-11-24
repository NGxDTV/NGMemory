using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static NGMemory.Kernel32;
using static NGMemory.MessageHelper;

namespace NGMemory
{
    public class DebugHook
    {
        private static bool keepDebugging = true;
        private static bool detachRequested = false;
        private static IntPtr hProcess = IntPtr.Zero;
        private static byte[] originalByte = new byte[1];


        /// <summary>
        /// Retrieves the value of a specified CPU register from the given thread context.
        /// Maps the register enumeration to the corresponding field in the CONTEXT structure.
        /// </summary>
        /// <param name="context">The thread context containing the register values.</param>
        /// <param name="register">The specific register to retrieve.</param>
        /// <returns>The value of the specified register.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified register is not recognized.</exception>
        public static ulong GetRegisterValue(CONTEXT context, Register register)
        {
            switch (register)
            {
                case Register.Rax: return context.Rax;
                case Register.Rcx: return context.Rcx;
                case Register.Rdx: return context.Rdx;
                case Register.Rbx: return context.Rbx;
                case Register.Rsp: return context.Rsp;
                case Register.Rbp: return context.Rbp;
                case Register.Rsi: return context.Rsi;
                case Register.Rdi: return context.Rdi;
                case Register.R8: return context.R8;
                case Register.R9: return context.R9;
                case Register.R10: return context.R10;
                case Register.R11: return context.R11;
                case Register.R12: return context.R12;
                case Register.R13: return context.R13;
                case Register.R14: return context.R14;
                case Register.R15: return context.R15;
                case Register.Rip: return context.Rip;
                default: throw new ArgumentOutOfRangeException(nameof(register), register, null);
            }
        }

        /// <summary>
        /// Monitors a specific memory address in a process for a hardware breakpoint and retrieves the value of a specified register when the breakpoint is hit.
        /// Overload method that uses the process ID to identify the target process.
        /// </summary>
        /// <param name="processID">The ID of the target process to attach to.</param>
        /// <param name="targetAddress">The memory address to set the hardware breakpoint on.</param>
        /// <param name="register">The register whose value should be retrieved when the breakpoint is hit.</param>
        /// <returns>The value of the specified register when the breakpoint is hit, or 0 on failure.</returns>
        public static ulong WaitForRegister(int processID, IntPtr targetAddress, Register register)
        {
            string processName = Process.GetProcessById(processID).ProcessName;
            return WaitForRegister(processName, targetAddress, register);
        }

        /// <summary>
        /// Monitors a specific memory address in a process for a hardware breakpoint and retrieves the value of a specified register when the breakpoint is hit.
        /// Attaches a debugger to the process, sets a hardware breakpoint, and waits for the single-step exception triggered by the breakpoint.
        /// </summary>
        /// <param name="processName">The name of the target process to attach to.</param>
        /// <param name="targetAddress">The memory address to set the hardware breakpoint on.</param>
        /// <param name="register">The register whose value should be retrieved when the breakpoint is hit.</param>
        /// <returns>The value of the specified register when the breakpoint is hit, or 0 on failure.</returns>
        public static ulong WaitForRegister(string processName, IntPtr targetAddress, Register register)
        {
            Process p = null;
            ulong currentRegister = 0;
            bool debugAttached = false;

            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0) return 0;

                p = processes[0];
                int processId = p.Id;

                if (!DebugActiveProcess(processId)) return 0;

                debugAttached = true;

                IntPtr hThread = OpenThread(THREAD_ALL_ACCESS, false, (uint)p.Threads[0].Id);
                if (hThread == IntPtr.Zero) return 0;

                CONTEXT context = new CONTEXT { ContextFlags = CONTEXT_DEBUG_REGISTERS };
                if (!GetThreadContext(hThread, ref context))
                {
                    CloseHandle(hThread);
                    return 0;
                }

                DebugSetProcessKillOnExit(false);

                context.Dr0 = (ulong)targetAddress.ToInt64();
                context.Dr7 = 0x1;

                if (!SetThreadContext(hThread, ref context))
                {
                    CloseHandle(hThread);
                    return 0;
                }

                CONTEXT hitContext = new CONTEXT { ContextFlags = CONTEXT_ALL };

                while (true)
                {
                    if (WaitForDebugEvent(out DEBUG_EVENT debugEvent, 1000))
                    {
                        if (debugEvent.dwDebugEventCode == EXCEPTION_DEBUG_EVENT &&
                            debugEvent.u.Exception.ExceptionRecord.ExceptionCode == EXCEPTION_SINGLE_STEP)
                        {
                            if (GetThreadContext(hThread, ref hitContext))
                                currentRegister = GetRegisterValue(hitContext, register);

                            ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_CONTINUE);
                            break;
                        }

                        ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_CONTINUE);
                    }
                }

                context.Dr7 = 0x0;
                SetThreadContext(hThread, ref context);
                CloseHandle(hThread);
            }
            catch
            {
                currentRegister = 0;
            }
            finally
            {
                if (debugAttached)
                    DebugActiveProcessStop(p?.Id ?? 0);
            }

            return currentRegister;
        }
    }
}
