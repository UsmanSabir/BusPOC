using System;
using System.Collections.Generic;
using System.IO;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Helper.SystemHealth.Models;

namespace BusLib.Helper.SystemHealth
{
    internal static class DiskInfoHealthProvider
    {
        private static List<DriveInfo> GetDrives()
        {
            List<DriveInfo> drives = new List<DriveInfo>();
            drives.AddRange(DriveInfo.GetDrives());
            return drives;
        }

        public static DiskSystemInfo[] GetDiskInfo()
        {
            List<DriveInfo> drives = GetDrives();
            List<DiskSystemInfo> tmp = new List<DiskSystemInfo>();

            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType.Equals(DriveType.Fixed) && drive.IsReady)
                    tmp.Add(new DiskSystemInfo(
                        drive.RootDirectory.ToString(),
                        (FileSystemType)Enum.Parse(typeof(FileSystemType), drive.DriveFormat, true),
                        drive.TotalSize));
            }

            return tmp.ToArray();
        }

        public static DiskPerformanceInfo[] GetDiskPerfInfo()
        {
            // Should only really check Drives that were identified earlier by GetDiskInfo()
            // Should we keep a local copy of the SystemInfo objects and pass them around when collecting perf. data?

            List<DriveInfo> drives = GetDrives();
            List<DiskPerformanceInfo> tmp = new List<DiskPerformanceInfo>();

            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType.Equals(DriveType.Fixed) && drive.IsReady)
                    tmp.Add(new DiskPerformanceInfo(
                        drive.RootDirectory.ToString(),
                        drive.TotalFreeSpace,
                        drive.TotalSize - drive.TotalFreeSpace));
                
            }
            return tmp.ToArray();
        }

        public static HealthBlock GetHealth()
        {
            HealthBlock block = new HealthBlock()
            {
                Name = "Disk"
            };

            try
            {
                var infos = GetDiskPerfInfo();
                block.AddMatrix("Count", infos.Length);

                foreach (var info in infos)
                {
                    HealthBlock b = new HealthBlock();
                    b.Name = info.Root;
                    b.AddMatrix(nameof(info.SizeTotalFree), info.SizeTotalFree);
                    b.AddMatrix(nameof(info.SizeUsageCurrent), info.SizeUsageCurrent);

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