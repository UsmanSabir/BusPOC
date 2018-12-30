using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public interface IProcessConfiguration
    {
        int ProcessKey { get;  }

        int BatchSize { get; }

        int? ProcessTimeoutMins { get;  }

        int? TaskTimeout { get;  }

        int? ProcessRetries { get; }
        

        int? TaskRetries { get;  }

        int? RetryDelayMilli { get; }

        int MaxVolumeRetries { get; }
        int? QueueSize { get;  }
    }
}
