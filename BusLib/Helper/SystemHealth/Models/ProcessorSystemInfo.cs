using System;

namespace BusLib.Helper.SystemHealth.Models
{
    public class ProcessorSystemInfo
    {

        private string name;
        private string description; // CPU string
        private string vendor;
        private Architecture architecture;
        private double speedMax; //(in  GHz)

        private ProcessorSystemInfo() { }

        public ProcessorSystemInfo(string name, string description, string vendor, Architecture architecture, double speedMax)
            : this()
        {
            this.Name = name;
            this.Description = description;
            this.Vendor = vendor;
            this.Architecture = architecture;
            this.SpeedMax = speedMax;
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

        public string Description
        {
            get { return description; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("description");
                if (value.Trim().Length == 0)
                    throw new ArgumentException("Processor description cannot be empty", "description");
                description = value;
            }
        }

        public string Vendor
        {
            get { return vendor; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("vendor");
                if (value.Trim().Length == 0)
                    throw new ArgumentException("Processor vendor name cannot be empty", "vendor");
                vendor = value;
            }
        }

        public Architecture Architecture
        {
            get { return architecture; }
            private set
            {
                architecture = value;
            }
        }

        public double SpeedMax
        {
            get { return speedMax; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Maximum processor speed cannot be less than zero", "speedMax");
                speedMax = value;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} : {4} Ghz (max.)", name, description, vendor, architecture, speedMax);
        }
    }
}