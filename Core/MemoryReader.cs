using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VerSehen.Core
{
    public class MemoryReader
    {
        public static Process FindProcess(string processName)
        {
            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.ProcessName == processName)
                {
                    return proc;
                }
            }
            return null;
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        out int lpNumberOfBytesRead);

        // Beispiel für eine ReadMemory-Methode
        public byte[] ReadMemory(Process process, IntPtr address, int size)
        {
            byte[] buffer = new byte[size];
            ReadProcessMemory(process.Handle, address, buffer, buffer.Length, out int bytesRead);
            return buffer;
        }

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        out int lpNumberOfBytesWritten);

        // Beispiel für eine WriteMemory-Methode
        public void WriteMemory(Process process, IntPtr address, byte[] bytes)
        {
            WriteProcessMemory(process.Handle, address, bytes, bytes.Length, out int bytesWritten);
        }
    }
}
