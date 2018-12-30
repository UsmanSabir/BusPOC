using System;

namespace BusLib.Helper.SystemHealth
{
    public class DiskPerformanceInfo
    {
        private string root; // OS's name/id for the root used for linking by performance and limit infos
        private long sizeTotalFree; //(in bytes)
        private long sizeUsageCurrent; //(in bytes)

        private DiskPerformanceInfo() { }

        public DiskPerformanceInfo(string root, long sizeTotalFree, long sizeUsageCurrent) : this()
        {
            this.Root = root;
            this.SizeTotalFree = sizeTotalFree;
            this.SizeUsageCurrent = sizeUsageCurrent;
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

        public long SizeTotalFree
        {
            get { return sizeTotalFree; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Size cannot be less than zero", "sizeTotalFree");
                sizeTotalFree = value;
            }
        }

       public long SizeUsageCurrent
        {
            get { return sizeUsageCurrent; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Size cannot be less than zero", "sizeUsageCurrent");
                sizeUsageCurrent = value;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} bytes Total Free. Currently Using {2} bytes", root, sizeTotalFree, sizeUsageCurrent);
        }
    }
}