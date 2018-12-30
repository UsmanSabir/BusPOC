using System;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Helper.SystemHealth.Models;

namespace BusLib.Helper.SystemHealth
{
    internal class OsInfoUtil
    {
        // For the moment this is just for Windows. Maybe this should be moved to the OsInfo class itself.
        public const string WINDOWS_95 = "95";
        public const string WINDOWS_98SE = "98 Second Edition";
        public const string WINDOWS_98 = "98";
        public const string WINDOWS_ME = "Me";
        public const string WINDOWS_NT_3 = "NT 3.51";
        public const string WINDOWS_NT_4 = "NT 4.0";
        public const string WINDOWS_2000 = "2000";
        public const string WINDOWS_XP = "XP";
        public const string WINDOWS_VISTA = "Vista";
        public const string UNKNOWN = "Unknown";//todo US: add other OS e.g. windows server or 10

        public static (string name, string version) GetOsInfo()
        {
            string OsName = "Windows";
            string OsVersion = "";

            System.OperatingSystem Os = System.Environment.OSVersion;

            // Determine the platform.
            switch (Os.Platform)
            {
                // Platform is Windows 95, Windows 98, 
                // Windows 98 Second Edition, or Windows Me.
                case System.PlatformID.Win32Windows:
                    switch (Os.Version.Minor)
                    {
                        case 0:
                            OsVersion = WINDOWS_95;
                            break;
                        case 10:
                            if (Os.Version.Revision.ToString() == "2222A")
                                OsVersion = WINDOWS_98SE;
                            else
                                OsVersion = WINDOWS_98;
                            break;
                        case 90:
                            OsVersion = WINDOWS_ME;
                            break;
                    }
                    break;
                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                // or Windows XP.
                case System.PlatformID.Win32NT:
                    switch (Os.Version.Major)
                    {
                        case 3:
                            OsVersion = WINDOWS_NT_3; //this should never happen: we don't run on NT 3/4/2000
                            break;
                        case 4:
                            OsVersion = WINDOWS_NT_4;
                            break;
                        case 5:
                            if (Os.Version.Minor == 0)
                                OsVersion = WINDOWS_2000;
                            else
                                OsVersion = WINDOWS_XP;
                            break;
                        case 6:
                            OsVersion = WINDOWS_VISTA; //TODO: correctly identify Win 2008, Win 2008 R2, and Win 7
                            break;
                        default:
                            OsVersion = UNKNOWN;
                            break;
                    }
                    break;
            }

            return (OsName, OsVersion);
        }

        public static HealthBlock GetHealth()
        {
            HealthBlock block = new HealthBlock()
            {
                Name = "OS"
            };

            try
            {
                var info = GetOsInfo();
                block.AddMatrix("Name", info.name);
                block.AddMatrix("Version", info.version);

            }
            catch (Exception e)
            {
                block.AddMatrix("Error", e.Message);
            }
            return block;
        }
    }
}