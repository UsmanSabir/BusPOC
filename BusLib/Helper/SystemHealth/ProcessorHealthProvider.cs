using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Helper.SystemHealth.Models;
using Microsoft.Win32;

namespace BusLib.Helper.SystemHealth
{
    // Because of the problem described here: 
    // http://softwaredevscott.spaces.live.com/blog/cns!1A9E939F7373F3B7!146.entry
    // we may end up getting negative numbers in the lower order
    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    internal struct FILETIME
    {
        public UInt32 dwLowDateTime;
        public UInt32 dwHighDateTime;
    }

    internal class ProcessorHealthProvider
    {
        private bool firstRun = true;
        private bool canUseCounters = false;
        private PerformanceCounter[] cpuUsageCounters = null;

        //private readonly IFrameworkLogger logger= BatchLoggerFactory.GetSystemLogger();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(
            out FILETIME lpIdleTime,
            out FILETIME lpKernelTime,
            out FILETIME lpUserTime
        );

        // Used to determine the CPU usage for this process.
        private DateTime timeOfLastCallForProcessTime = DateTime.Now;
        private TimeSpan prevProcessTime;
        // Used to determine CPU usage for system 
        private FILETIME prevIdleTime = new FILETIME();
        private FILETIME prevKernelTime = new FILETIME();
        private FILETIME prevUserTime = new FILETIME();

        //private static readonly TimeSpan interval = TimeSpan.FromMilliseconds(300); //300 ms some MAGIC!

        private const string CentralProcessorKeyName = @"HARDWARE\DESCRIPTION\System\CentralProcessor";

        private int processorCount = Environment.ProcessorCount;

        //cache the processor static properties:
        List<ProcessorSystemInfo> processors;

        static ProcessorHealthProvider _instance=new ProcessorHealthProvider();

        public ProcessorSystemInfo[] GetProcessorInfo()
        {
            if (processors == null)
            {
                int id = 0;
                String identifier = "";
                String name = "";
                String vendor = "";
                double speed = 0.0;
                Architecture arch = Architecture.None;

                // Get Processor Architecture. All CPUs on the system should have the same architecture.
                arch = GetProcessorArchitecture();

                // Get the list of Processors
                RegistryKey hklm = Registry.LocalMachine;
                hklm = hklm.OpenSubKey(CentralProcessorKeyName);
                string[] cpus = hklm.GetSubKeyNames();

                processors = new List<ProcessorSystemInfo>();
                for (int i = 0; i < cpus.Length; i++)
                {
                    hklm = Registry.LocalMachine;
                    hklm = hklm.OpenSubKey(CentralProcessorKeyName + @"\" + cpus[i]);

                    id = i;
                    identifier = hklm.GetValue("Identifier", "-").ToString();
                    vendor = hklm.GetValue("VendorIdentifier", "-").ToString();
                    name = hklm.GetValue("ProcessorNameString", "-").ToString();

                    //can't directly cast it to string, because the underlying value is seems to be an Int32.
                    object mhz = hklm.GetValue("~MHz", 0);
                    if (mhz != null) //may happen if the key is not found
                    {
                        //        PC booted in power saving mode (ie. windows will
                        //        store the low-power CPU speed in this value.
                        double.TryParse(hklm.GetValue("~MHz").ToString(), out speed);
                    }

                    // Convert MHz to GHz
                    speed = speed / 1000.00;

                    ProcessorSystemInfo processor = new ProcessorSystemInfo(id.ToString(), name, vendor, arch, speed);
                    processors.Add(processor);
                }
            }
            return processors.ToArray();
        }

        public Architecture GetProcessorArchitecture()
        {
            RegistryKey hklm = Registry.LocalMachine;
            hklm = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
            string arch = (string)hklm.GetValue("PROCESSOR_ARCHITECTURE") ?? string.Empty;
            hklm.Close();

            Architecture archValue = Architecture.None;
            try
            {
                if (!string.IsNullOrEmpty(arch))
                    archValue = (Architecture)Enum.Parse(typeof(Architecture), arch, true);

            }
            catch (ArgumentException) { }

            return archValue;
        }

        public ProcessorPerformanceInfo[] GetProcessorPerfInfo()
        {
            // We can get the Processor load a number of different ways :
            // 1. Using Performance Counters, however they might not be running on all machines 
            //    (can programatically change registry to fix kindof)
            //    Advantage: we can get load of each CPU independently.
            //    Disadvantage: on Vista/Win2003/8 the user needs to be admin/part of Performance Monitor Users group.
            // 2. Using PInvoke call to GetSystemTimes then we can just divide the load between CPUs (ok but not accurate)
            // 3. Using PInvoke call to NTQuerySystemInformation (disadvantage is that this API is not guaranteed to be available 
            //      in the next version of Windows and is not recommended by Microsoft. 
            //      However, the TaskManager does use this API, and it has proper multi-proc/core support.
            // 4. Using WMI: too much overhead? Need to have WMI service running.

            //We are currently using #1, 2

            //in general the CPU speed is the same for all the CPUs in a system.
            //so let us just get the first CPU speed, and set it as the current speed - for now.
            //an accurate current speed can be obtained from WMI, but that may be too much overhead
            //this method can be safely called without a perf hit, since it caches the processor info
            GetProcessorInfo();
            double currentCpuSpeed = 0;
            //we know that the list is now initialised
            if (processors != null && processors.Count > 0)
            {
                currentCpuSpeed = processors[0].SpeedMax;
            }

            List<ProcessorPerformanceInfo> perfInfos = new List<ProcessorPerformanceInfo>(processorCount);

            // Initialise things on the first run //

            // Check if performance counters are enabled
            // And initialise some variables for the first run
            if (firstRun)
            {
                //For Mono, we know PerformanceCounters are not implemented at the moment (March 2008). So, we just compile it out.

                canUseCounters = EnablePerformanceCounters(true);

                firstRun = false;
                //logger.Trace("Performance Counters Enabled? " + canUseCounters);
                // Initialise this for the first time.
                timeOfLastCallForProcessTime = DateTime.Now;
                prevProcessTime = Process.GetCurrentProcess().TotalProcessorTime;
                if (!canUseCounters)
                {
                    bool success = GetSystemTimes(out prevIdleTime, out prevKernelTime, out prevUserTime);
                }
                Thread.Sleep(500); // sleep a little on the first run to put some time between calls to check proc. usage
            }


            // Calculate CPU Usage for Executor Process //

            // For both canUseCounters and !canUseCounters
            // Calculate the difference between successive calls to this method.
            DateTime timeNow = DateTime.Now;
            TimeSpan interval = timeNow.Subtract(timeOfLastCallForProcessTime);
            // todoLater: processTime actually needs to also include process time of DotNetExecutor.exe/JavaExecutor which may be running Jobs AND which processor it's running on
            TimeSpan processTime = Process.GetCurrentProcess().TotalProcessorTime;
            double executorCPUUsagePercent = 100 * (processTime.TotalMilliseconds - prevProcessTime.TotalMilliseconds) / interval.TotalMilliseconds;
            if (executorCPUUsagePercent > 100.00)
            {
                //logger.Warn("CPU Usage for this process measured over 100%. Setting value to 100%.");
                executorCPUUsagePercent = 100.00;
            }
            timeOfLastCallForProcessTime = timeNow;
            prevProcessTime = processTime;


            // Calculate Overall CPU Usage //

            try
            {
                if (canUseCounters)
                {
                    int numCpus = CpuUsageCounters.Length;
                    for (int i = 0; i < numCpus; i++)
                    {

                        double nextValue = (double)CpuUsageCounters[i].NextValue();

                        if (nextValue > 100.00)
                        {
                            //logger.Warn("CPU Usage for overall system measured over 100%. Setting value to 100%.");
                            nextValue = 100.00;
                        }
                        ProcessorPerformanceInfo perfInfo =
                            new ProcessorPerformanceInfo(i.ToString(), currentCpuSpeed, nextValue, executorCPUUsagePercent);
                        perfInfos.Add(perfInfo);
                    }
                }
            }
            catch (System.UnauthorizedAccessException ux)
            {
                //logger.Warn("Cannot access performance counter data: " + ux.Message);
                canUseCounters = false;
            }

            if (!canUseCounters)
            {
                // refer : http://www.codeproject.com/KB/threads/Get_CPU_Usage.aspx
                // If we cant use Performance Counters then try GetSystemTimes
                FILETIME idleTime, kernelTime, userTime;
                double cpuUsagePercent = 0.0;

                if (GetSystemTimes(out idleTime, out kernelTime, out userTime))
                {
                    ulong idleTimeDifference = GetDifference(idleTime, prevIdleTime);
                    ulong kernelTimeDifference = GetDifference(kernelTime, prevKernelTime);
                    ulong userTimeDifference = GetDifference(userTime, prevUserTime);

                    ulong sysTime = (kernelTimeDifference + userTimeDifference);
                    cpuUsagePercent = 100 * (sysTime - idleTimeDifference) / sysTime; // This is total time over ALL processors so needs to be divided

                    prevIdleTime = idleTime;
                    prevKernelTime = kernelTime;
                    prevUserTime = userTime;

                    //logger.Debug("refreshInterval: " + interval.Ticks);
                    //logger.Debug("Idle time diff: " + idleTimeDifference);
                    //logger.Debug("Kernel time diff: " + kernelTimeDifference);
                    //logger.Debug("User time diff: " + userTimeDifference);
                    //logger.Debug("System time diff: " + sysTime);
                    //logger.Debug("Sys + Idle - Interval = " + (sysTime + idleTimeDifference - (ulong)interval.Ticks));
                    //this seemed logical, but is wrong! always gives a weird 100+ value
                    //logger.Debug("CPU Usage: % " + (100 * sysTime / (ulong)(interval.Ticks)));
                    //this is confusing but correct!
                    //logger.Debug("Alt CPU Usage: % " + cpuUsagePercent);
                }
                else
                {
                    //logger.Warn("GetSystemTimes returned false");
                }

                double individualCpuUsagePercent = cpuUsagePercent;
                if (processorCount > 1)
                    individualCpuUsagePercent = (double)(cpuUsagePercent / (double)processorCount);

                if (individualCpuUsagePercent > 100.00)
                {
                    //logger.Warn("CPU Usage for overall system measured over 100% (per core). Setting value to 100% (per core).");
                    individualCpuUsagePercent = 100.00;
                }

                //return empty values for performance for now
                for (int i = 0; i < processorCount; i++)
                {
                    ProcessorPerformanceInfo perfInfo =
                        new ProcessorPerformanceInfo(i.ToString(), currentCpuSpeed, individualCpuUsagePercent, executorCPUUsagePercent);
                    perfInfos.Add(perfInfo);
                }
            }

            return perfInfos.ToArray();
        }

        private ulong GetDifference(FILETIME t1, FILETIME t2)
        {
            ulong t1Long = ((ulong)(t1.dwHighDateTime << 32)) + (ulong)t1.dwLowDateTime;
            ulong t2Long = ((ulong)(t2.dwHighDateTime << 32)) + (ulong)t2.dwLowDateTime;
            return (t1Long - t2Long);
        }

        private PerformanceCounter[] CpuUsageCounters
        {
            get
            {
                if (cpuUsageCounters == null)
                {
                    // Check # CPUs & create a counter for each one.
                    //logger.Trace("Initialising counters...");
                    // Create a counter for each CPU (we need to use the 0, 1, 2... to identify CPUS)
                    cpuUsageCounters = new PerformanceCounter[processorCount];
                    for (int i = 0; i < processorCount; i++)
                    {
                        cpuUsageCounters[i] = new PerformanceCounter();
                        cpuUsageCounters[i].ReadOnly = true;
                        cpuUsageCounters[i].CategoryName = "Processor";
                        cpuUsageCounters[i].CounterName = "% Processor Time";
                        cpuUsageCounters[i].InstanceName = i.ToString();
                        //logger.Trace("Created Counter for CPU #" + i);
                    }
                }
                return cpuUsageCounters;
            }
        }

        private bool EnablePerformanceCounters(bool shouldEnable)
        {
            bool enabled = false;

            int disable = -1;
            int disabled = -1;

            if (shouldEnable)
                disable = 0;
            else
                disable = 1;

            //logger.Debug("Checkng for 2000 or XP");
            // Check OS. Only need to do this for XP or 2000
            // NOTE : For now just ignore 98/Me etc.
            //if (!Is2000orXP())
            //    return true;

            RegistryKey hklm = null;
            try
            {
                //logger.Trace("Checking Registry if Performance Counters Enabled");
                // Check registry to see if Performance counter is enabled for Processor (only needed for 2000 and XP)
                hklm = Registry.LocalMachine;
                hklm = hklm.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\PerfOS\\Performance");
                if (hklm == null)
                    return false;
                Object value = hklm.GetValue("Disable Performance Counters");
                Int32.TryParse((string)value, out disabled);
                if (disabled < 0)
                {
                    enabled = false;
                }
                else
                {
                    //logger.Trace("Value is : " + (string)value);

                    // If enable and disable are equal we need to set the registry otherwise we are fine.
                    if (disable != disabled)
                    {
                        //logger.Trace("Need to set the Registry");
                        //logger.Trace("Checking for access to Registry");
                        hklm = Registry.LocalMachine;
                        hklm = hklm.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\PerfOS\\Performance", true);
                        if (hklm == null)
                            enabled = false;
                    }
                    else
                    {
                        //logger.Trace("Dont need to set the Registry");
                        enabled = true;
                    }
                }
            }
            catch (Exception e)
            {
                // Failed to set the key
                //logger.Warn("Failed to set the Performance Counter. {0}", e);
            }
            finally
            {
                if (hklm != null)
                {
                    //try to close it
                    try
                    {
                        hklm.Close();
                    }
                    catch { }
                }
            }

            // We can also enable this one if we need to...
            //HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PerfProc\Performance

            return enabled;
        }

        public static HealthBlock GetHealth()
        {
            HealthBlock block = new HealthBlock()
            {
                Name = "Processor"
            };

            try
            {
                var infos = _instance.GetProcessorPerfInfo();
                block.AddMatrix("Count", infos.Length);

                foreach (var info in infos)
                {
                    HealthBlock b=new HealthBlock();
                    b.Name = info.Name;
                    b.AddMatrix(nameof(info.LoadTotalCurrent), info.LoadTotalCurrent);
                    b.AddMatrix(nameof(info.LoadUsageCurrent), info.LoadUsageCurrent);
                    b.AddMatrix(nameof(info.SpeedCurrent), info.SpeedCurrent);
                    
                    block.AddNested(b);
                }
            }
            catch (Exception e)
            {
                block.AddMatrix("Error", e.Message);
            }
            return block;
        }
    }
}