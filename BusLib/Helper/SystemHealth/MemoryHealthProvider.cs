using System;
using System.Runtime.InteropServices;
using BusLib.BatchEngineCore.PubSub;

namespace BusLib.Helper.SystemHealth
{
    internal class MemoryHealthProvider
    {
        public struct MEMORYSTATUSEX
        {
            public int dwLength;
            public int dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        static long GetTotalMemory()
        {
            MEMORYSTATUSEX memStat = new MEMORYSTATUSEX();
            memStat.dwLength = 64;
            bool b = GlobalMemoryStatusEx(ref memStat);
            return (long)memStat.ullTotalPhys;
        }

        static long GetAvailableMemory()
        {
            MEMORYSTATUSEX memStat = new MEMORYSTATUSEX();
            memStat.dwLength = 64;
            bool b = GlobalMemoryStatusEx(ref memStat);
            return (long)memStat.ullAvailPhys;
        }

        public static HealthBlock GetHealth()
        {
            HealthBlock block=new HealthBlock()
            {
                Name = "Memory"
            };

            try
            {
                var total = GetTotalMemory();
                var available = GetAvailableMemory();

                block.AddMatrix("Total", total);
                block.AddMatrix("Available", available);
                block.AddMatrix("Remaining", total-available);

            }
            catch (Exception e)
            {
                block.AddMatrix("Error", e.Message);
            }
            return block;
        }
    }
}