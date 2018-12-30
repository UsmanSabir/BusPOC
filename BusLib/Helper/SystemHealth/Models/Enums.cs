namespace BusLib.Helper.SystemHealth.Models
{
    public enum Architecture
    {
        None = 0,
        Amd64 = 1,
        IA64 = 2,
        MSIL = 3,
        X86 = 4
    }
    public enum FileSystemType
    {
        Unknown = 0,
        Fat16 = 1,
        Fat32 = 2,
        Ntfs = 3,
        Ext2 = 4,
        Ext3 = 5
    }
}