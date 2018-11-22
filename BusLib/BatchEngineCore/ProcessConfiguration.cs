using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public class ProcessConfiguration
    {
        public string ProcessKey { get; set; }

        public int BatchSize { get; set; }

        public int MaxVolumeRetries { get; set; }
    }
}
