using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Saga;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Helper;

namespace BusLib.BatchEngineCore
{
    internal class TaskProducerWorker:RepeatingTriggeringProcess
    {
        private readonly IVolumeHandler _volumeHandler;
        private readonly ICacheAside _cacheAside;
        private TinyMessageSubscriptionToken _retrySub;
        private TinyMessageSubscriptionToken _volumeSub;
        private TinyMessageSubscriptionToken _healthSub;

        public TaskProducerWorker(ILogger logger, ICacheAside cacheAside, IVolumeHandler volumeHandler) : base("TaskProducer", logger)
        {
            _cacheAside = cacheAside;
            _volumeHandler = volumeHandler;
        }

        private void Run()
        {
            var taskItem = _volumeHandler.GetNextTaskWithTransaction(out ITransaction transaction);

            while (taskItem != null && Interrupter?.IsCancellationRequested==false)
            {
                SafeTransactionWrapper transactionWrapper=null;
                try
                {
                    transactionWrapper = new SafeTransactionWrapper(transaction);
                    var processExecutionContext = _cacheAside.GetProcessExecutionContext(taskItem.ProcessId);
                    if (processExecutionContext == null)
                    {
                        Logger.Error($"TaskPicker: Process configuration not found for Process {taskItem?.ProcessId} against task {taskItem.Id.ToString()} ");
                        transactionWrapper.Rollback();
                        transactionWrapper.Dispose();
                        continue;
                    }

                    var processKey = processExecutionContext.ProcessState.ProcessKey;
                    var logger = LoggerFactory.GetTaskLogger(taskItem.Id, taskItem.ProcessId, processExecutionContext.ProcessState.CorrelationId);
                    TaskMessage taskMessage = new TaskMessage(taskItem, transaction, new SafeDisposableActions(transactionWrapper.Rollback), logger);
                    taskMessage.ProcessContext = processExecutionContext;

                    logger.Info($"Task picked at Node {NodeSettings.Instance.Name}");

                    Bus.Instance.HandleTaskMessage(taskMessage);
                }
                catch (Exception e)
                {
                    Logger.Error($"Error picking task {taskItem?.Id.ToString()} for Process {taskItem?.ProcessId} with message {e.Message}", e);
                    transactionWrapper?.Dispose();
                }
                taskItem = _volumeHandler.GetNextTaskWithTransaction(out transaction);
            }
            transaction?.Dispose();
        }

        

        internal override void PerformIteration()
        {
            Run();
        }


        internal override void OnStart()
        {
            base.OnStart();

            _volumeSub = Bus.Instance.EventAggregator.Subscribe<TextMessage>(VolumeGeneratedHandler, Constants.EventProcessVolumeGenerated);
            _retrySub = Bus.Instance.EventAggregator.Subscribe<TextMessage>(ProcessRetryTasksHandler, Constants.EventProcessRetry);

            this._healthSub = Bus.Instance.EventAggregator.Subscribe4Broadcast<HealthMessage>(PublishHealth);
        }

        private void PublishHealth(HealthMessage health)
        {
            Logger.Trace($"HealthCheck '{nameof(TaskProducerWorker)}' Start");
            HealthBlock block=new HealthBlock()
            {
                Name = "TaskProducer"
            };

            block.AddMatrix("IsActive", IsActive);

            health.Add(block);
            Logger.Trace($"HealthCheck '{nameof(TaskProducerWorker)}' End");
        }

        protected override void OnStopping()
        {
            base.OnStopping();
            Bus.Instance.EventAggregator.Unsubscribe(_healthSub);
            Bus.Instance.EventAggregator.Unsubscribe(_volumeSub);
            Bus.Instance.EventAggregator.Unsubscribe(_retrySub);
            
            _volumeSub = _retrySub = null;
        }

        private void ProcessRetryTasksHandler(TextMessage msg)
        {
            Logger.Trace($"{nameof(TaskProducerWorker)} Received process retry message for processId {msg.Parameter ?? string.Empty}");
            Trigger();
        }

        private void VolumeGeneratedHandler(TextMessage msg)
        {
            Logger.Trace($"{nameof(TaskProducerWorker)} Received volume generated message for processId {msg.Parameter??string.Empty}");
            Trigger();
        }

    }
}