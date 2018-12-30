using System;
using BusLib.Helper.SystemHealth.Models;

namespace BusLib.Helper.SystemHealth
{
    public class DiskSystemInfo
    {

        private string root;
        private FileSystemType fileSystem;
        private long sizeTotal; //(in bytes)

        private DiskSystemInfo() { }

        public DiskSystemInfo(string root, FileSystemType fileSystem, long sizeTotal) : this()
        {
            this.Root = root;
            this.FileSystem = fileSystem;
            this.SizeTotal = sizeTotal;
        }
        
        public string Root
        {
            get { return root; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("root");
                if (value.Trim().Length == 0)
                    throw new ArgumentException("Disk root cannot be empty", "root");
                root = value;
            }
        }

        public FileSystemType FileSystem
        {
            get { return fileSystem; }
            private set
            {
                fileSystem = value;
            }
        }

        public long SizeTotal
        {
            get { return sizeTotal; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Size cannot be less than zero", "sizeTotal");
                sizeTotal = value;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} : {2} bytes Total",
                root, fileSystem, sizeTotal);
        }
    }
}