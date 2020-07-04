using ScriptKidAntiCheat.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text;

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
                return Encoding.ASCII.GetString(buffer);
            }
            else
            {
                return null;
            }
        }

    }
}
