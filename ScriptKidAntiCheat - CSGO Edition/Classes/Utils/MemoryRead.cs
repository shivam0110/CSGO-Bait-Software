using ScriptKidAntiCheat.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Utils
{
    static class MemoryRead
    {
        public static T Read<T>(this System.Diagnostics.Process process, IntPtr lpBaseAddress) where T : unmanaged
        {
            return Read<T>(process.Handle, lpBaseAddress);
        }

        public static T Read<T>(this Module module, int offset) where T : unmanaged
        {
            if (module != null)
            {
                return Read<T>(module.Process.Handle, module.ProcessModule.BaseAddress + offset);
            }

            return default;
        }
        
        public static string ReadString(this Module module, IntPtr lpBaseAddress, int offset, int bytesToRead)
        {
            return ReadString(module.Process.Handle, lpBaseAddress + offset, bytesToRead);
        }

        public static T Read<T>(IntPtr hProcess, IntPtr lpBaseAddress) where T : unmanaged
        {
            var size = Marshal.SizeOf<T>();
            var buffer = (object)default(T);

            if (Program.GameProcess.IsValid)
            {
                Kernel32.ReadProcessMemory(hProcess, lpBaseAddress, buffer, size, out var lpNumberOfBytesRead);
                return lpNumberOfBytesRead == size ? (T)buffer : default;
            } else
            {
                return default;
            }

        }

        public static string ReadString(IntPtr hProcess, IntPtr BaseAddress, int bytesToRead)
        {
            var size = bytesToRead;
            byte[] buffer = new byte[bytesToRead];
            Kernel32.ReadProcessMemory(hProcess, BaseAddress, buffer, size, out var lpNumberOfBytesRead);

            if (lpNumberOfBytesRead == size)
            {
                // Remove unused bytes from string
                var length = buffer.TakeWhile(b => b != 0).Count();
                var text = Encoding.UTF8.GetString(buffer, 0, length);
                return text;
            }
            else
            {
                return null;
            }
        }

        public static int PatternScan(Module module, byte[] pattern, string mask)
        {
            int moduleSize;
            IntPtr BaseAddress = module.ProcessModule.BaseAddress;

            moduleSize = module.ProcessModule.ModuleMemorySize;

            if (moduleSize == 0) return 0;

            byte[] moduleBytes = new byte[moduleSize];
            int numBytes;

            if (Kernel32.ReadProcessMemory(module.Process.Handle, BaseAddress, moduleBytes, moduleSize, out numBytes)) //do one large RPM vs many small RPM - this will be your most significant speedup
            {
                for (int i = 0; i < moduleSize; i++) //you can subtract your mask length here but the difference in speed is negligible
                {
                    bool found = true;

                    for (int l = 0; l < mask.Length; l++)
                    {
                        found = mask[l] == '?' || moduleBytes[l + i] == pattern[l];

                        if (!found)
                            break;
                    }

                    if (found)
                        return i;
                }
            }

            return 0;
        }
    }
}
