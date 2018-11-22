using BusLib.BatchEngineCore.Volume;
using BusLib.Helper;

namespace BusLib.BatchEngineCore
{
    class TaskProducerWorker
    {
        private IVolumeHandler _volumeHandler;
        private ICacheAside _cacheAside;
        
        public void Run()
        {
            var taskItem = _volumeHandler.GetNextTask();
            //todo null checks and other validations

            var processExecutionContext = _cacheAside.GetProcessExecutionContext(taskItem.ProcessId);
            var processKey = processExecutionContext.ProcessState.ProcessKey;
            
        }
    }
}