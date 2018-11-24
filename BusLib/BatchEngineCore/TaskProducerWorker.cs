using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore.Saga;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore
{
    class TaskProducerWorker
    {
        private IVolumeHandler _volumeHandler;
        private ICacheAside _cacheAside;

        ConcurrentDictionary<int, ITask> _taskExecutors = new ConcurrentDictionary<int, ITask>();
        private ILogger _logger;

        public void Run()
        {
            var taskItem = _volumeHandler.GetNextTaskWithTransaction(out ITransaction transaction);
            
            //var disposable = new Action(() => transaction.Rollback()).ToDisposable();
            //ITaskContext<>

            //todo null checks and other validations

            var processExecutionContext = _cacheAside.GetProcessExecutionContext(taskItem.ProcessId);
            var processKey = processExecutionContext.ProcessState.ProcessKey;
            LoggerFactory.GetTaskLogger(taskItem.Id, taskItem.ProcessId,
                processExecutionContext.ProcessState.CorrelationId);

            // ITaskContext
            string prevState = string.Empty;
            string nextState = string.Empty;
            ConcurrentDictionary<string, string> taskStatesCollection = null;

            if (_taskExecutors.TryGetValue(processKey, out ITask task))
            {
                //if (taskItem.FailedCount>1) 
                //{
                //    //todo load task status from other status table
                //}
                var taskStates = _volumeHandler.GetTaskStates(taskItem.Id, taskItem.ProcessId);
                if (taskStates != null && taskStates.Any())
                {
                    
                    foreach (var pair in taskStates)
                    {
                        if (pair.Key==KeyConstants.TaskPreviousState)
                        {
                            prevState = pair.Value;
                            continue;
                        }
                        else if (pair.Key == KeyConstants.TaskNextState)
                        {
                            nextState = pair.Value;
                            continue;
                        }
                        else
                        {
                            //store custom states
                            if (taskStatesCollection==null)
                                taskStatesCollection=new ConcurrentDictionary<string, string>();

                            taskStatesCollection.AddOrUpdate(pair.Key, pair.Value, (k, val) => pair.Value);
                        }


                    }
                }
            }
            else
            {
                _logger.Error($"Task executor not found for processKey: {processKey}");
            }
        }
    }
}