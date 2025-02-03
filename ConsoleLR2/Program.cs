using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLR2
{
    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess,
            bool bInheritHandle, uint dwOptions);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;

        const uint TH32CS_SNAPHEAPLIST = 0x00000001;
        const uint TH32CS_SNAPMODULE = 0x00000008;
        const uint TH32CS_SNAPTHREAD = 0x00000004;
        const uint TH32CS_SNAPPROCESS = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int dwPriorityClass;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        static void Main(string[] args)
        {
            uint currentProcessId = GetCurrentProcessId();
            Console.WriteLine($"ID текущего процесса {currentProcessId}");

            IntPtr currentProcessHandle = GetCurrentProcess();
            Console.WriteLine($"Псевдодескриптор текущего процесса {currentProcessHandle}");

            IntPtr duplicatedHandle;
            if (DuplicateHandle(currentProcessHandle, currentProcessHandle, currentProcessHandle,
                out duplicatedHandle, 0, false, 0))
            {
                Console.WriteLine($"Дескриптор текущего процесса {duplicatedHandle}, {currentProcessHandle}");
            }
            else
            {
                return;
            }

            IntPtr openProcessHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, currentProcessId);
            if (openProcessHandle == IntPtr.Zero)
            {
                return;
            }
            Console.WriteLine($"Функция OpenProcess {openProcessHandle}");

            CloseHandle(duplicatedHandle);
            CloseHandle(openProcessHandle);

            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (snapshot == IntPtr.Zero)
            {
                return;
            }

            PROCESSENTRY32 processEntry = new PROCESSENTRY32();
            processEntry.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));

            if (Process32First(snapshot, ref processEntry))
            {
                do
                {
                    Console.WriteLine($"Процесс: {processEntry.szExeFile}, ID: {processEntry.th32ProcessID}");

                } while (Process32Next(snapshot, ref processEntry));
            }

            CloseHandle(snapshot);

            Console.ReadKey();
        }
    }
}
