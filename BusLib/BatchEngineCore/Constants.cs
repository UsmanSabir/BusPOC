using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    class Constants
    {
        public const string ReasonTimeOut = "Timeout";
        public const string ReasonCancelled = "Cancelled";
        public const string ReasonCompleted = "Completed";


        //Events
        //public const string EventProcessStopped = "ProcessStopped";
        public const string EventProcessRetry = "ProcessRetry";
        public const string EventInvokeProducer = "InvokeProducer";
        public const string EventProcessStop = "ProcessStop";
        public const string EventProcessFinished = "ProcessFinished";//process is finished and being removed. cleanup resources
        public const string EventCheckGroupCommand = "CheckGroup";//a command to go proceed with group handler for further processes
        public const string EventProcessVolumeGenerated = "ProcessVolumeGenerated";
        public const string EventProcessVolumeError = "ProcessVolumeError";
        public const string EventProcessGroupAdded = "ProcessGroupAdded";
    }
}

