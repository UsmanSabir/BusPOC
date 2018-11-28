using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public class ProcessConfiguration
    {
        public int ProcessKey { get; set; }

        public int BatchSize { get; set; }

        public int? TaskTimeout { get; set; }

        public int? TaskRetries { get; set; }

        public int? RetryDelayMilli { get; set; }

        public int MaxVolumeRetries { get; set; }
        public int? QueueSize { get; set; }
    }
}
