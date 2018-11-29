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
    internal class TaskProducerWorker:RepeatingProcess
    {
        private IVolumeHandler _volumeHandler;
        private ICacheAside _cacheAside;
        
        private void Run()
        {
            var taskItem = _volumeHandler.GetNextTaskWithTransaction(out ITransaction transaction);

            while (taskItem != null && Interrupter?.IsCancellationRequested==false)
            {
                TransactionWrapper transactionWrapper = new TransactionWrapper(transaction);

                var processExecutionContext = _cacheAside.GetProcessExecutionContext(taskItem.ProcessId);
                var processKey = processExecutionContext.ProcessState.ProcessKey;
                var logger = LoggerFactory.GetTaskLogger(taskItem.Id, taskItem.ProcessId,
                    processExecutionContext.ProcessState.CorrelationId);
                TaskMessage taskMessage = new TaskMessage(taskItem, transaction,
                    new SafeDisposableActions(transactionWrapper.Rollback), logger);
                taskMessage.ProcessContext = processExecutionContext;

                logger.Info($"Task picked at Node {NodeSettings.Instance.Name}");

                Bus.Instance.HandleTaskMessage(taskMessage);

                taskItem = _volumeHandler.GetNextTaskWithTransaction(out transaction);
            }
        }

        public TaskProducerWorker(ILogger logger, ICacheAside cacheAside, IVolumeHandler volumeHandler) : base("TaskProducer", logger)
        {
            _cacheAside = cacheAside;
            _volumeHandler = volumeHandler;
        }

        internal override void PerformIteration()
        {
            Run();
        }
    }
}