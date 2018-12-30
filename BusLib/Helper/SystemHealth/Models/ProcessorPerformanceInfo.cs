using System;

namespace BusLib.Helper.SystemHealth.Models
{
    public class ProcessorPerformanceInfo
    {
        private string name; // OS's name/id for this CPU used for linking during the updates
        private double speedCurrent; //(in Ghz)
        private double loadTotalCurrent; //(in %)
        private double loadUsageCurrent; //(in %)

        private ProcessorPerformanceInfo() { }

        public ProcessorPerformanceInfo(string name, double speedCurrent, double loadTotalCurrent, double loadUsageCurrent)
            : this()
        {
            this.Name = name;
            this.SpeedCurrent = speedCurrent;
            this.LoadTotalCurrent = loadTotalCurrent;
            this.LoadUsageCurrent = loadUsageCurrent;
        }

        public string Name
        {
            get { return name; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("name");
                if (value.Trim().Length == 0)
                    throw new ArgumentException("Processor name cannot be empty", "name");
                name = value;
            }
        }

        public double SpeedCurrent
        {
            get { return speedCurrent; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Current processor speed cannot be less than zero", "speedCurrent");
                speedCurrent = value;
            }
        }

        public double LoadTotalCurrent
        {
            get { return loadTotalCurrent; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Current total processor load cannot be less than zero", "loadTotalCurrent");
                loadTotalCurrent = value;
            }
        }

        public double LoadUsageCurrent
        {
            get { return loadUsageCurrent; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Current processor load usage cannot be less than zero", "loadUsageCurrent");
                loadUsageCurrent = value;
            }
        }

        public override string ToString()
        {
            return string.Format("CPU {0} : {1} GHz Current Speed : {2}% Current Load Total {3}% Load Usage Current",
                name, speedCurrent, loadTotalCurrent, loadUsageCurrent);
        }
    }
}