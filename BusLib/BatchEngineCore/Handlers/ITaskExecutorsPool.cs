using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Process;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Handlers
{
    interface ITaskExecutorsPool
    {
        ProcessConsumer Get(long processId, int processStateProcessKey, long groupId);
    }

    class TaskExecutorsPool : RepeatingTriggeringProcess, ITaskExecutorsPool
    {
        readonly ConcurrentDictionary<long, ProcessConsumer> _processConsumer = new ConcurrentDictionary<long, ProcessConsumer>();
        private readonly IStateManager _stateManager;
        private readonly CancellationToken _token;
        private readonly ICacheAside _cacheAside;
        private const int TaskTimoutCheckInterval = 2000;
        private readonly IProcessRepository _processRepository;
        private TinyMessageSubscriptionToken _subStop;
        private TinyMessageSubscriptionToken _subRem;
        private TinyMessageSubscriptionToken _healthSub;
        private readonly IEventAggregator _eventAggregator;
        private readonly IResolver _resolver;
        private readonly IFrameworkLogger _frameworkLogger;

        #region commented

        //private Task GetTimeoutObserverTask()
        //{
        //    var task = Task.Factory.StartNew(async () =>
        //    {
        //        while (!_token.IsCancellationRequested)
        //        {

        //            try
        //            {
        //                await Task.Delay(Math.Min(TaskTimoutCheckInterval, _processNotificationThresholdMilliSec), _token);

        //                if (_token.IsCancellationRequested)
        //                    return;

        //                await SweepTimedoutTasks();

        //                CheckLastInputInterval();

        //            }
        //            catch (TaskCanceledException e)
        //            {
        //                var msg =
        //                    $"Timeout observer task canceled Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
        //                Logger.Info(msg);
        //            }
        //            catch (OperationCanceledException e)
        //            {
        //                var msg =
        //                    $"Timeout observer task canceled Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
        //                _logger.Info(msg);
        //            }
        //            catch (Exception e)
        //            {
        //                _logger.Error($"Timeout observer got unexpected error with message {e.Message}", e);
        //            }
        //        }
        //        _logger.Trace($"Timeout observer stopped for Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))}");

        //    }, _processToken);
        //    return task;
        //}

        #endregion

        public TaskExecutorsPool(ILogger logger, ICacheAside cacheAside, CancellationToken token, IStateManager stateManager, IProcessRepository processRepository, IEventAggregator eventAggregator
        ,IResolver resolver, IFrameworkLogger frameworkLogger) : base(nameof(TaskExecutorsPool), logger)
        {
            _cacheAside = cacheAside;
            _token = token;
            _stateManager = stateManager;
            _processRepository = processRepository;
            _eventAggregator = eventAggregator;
            _resolver = resolver;
            _frameworkLogger = frameworkLogger;
            //Start(_token);
            Interval= TimeSpan.FromMilliseconds(1500);
            //TaskTimoutCheckInterval = 1500;
        }

        internal override void OnStart()
        {
            base.OnStart();
            this._subRem = this._eventAggregator.Subscribe<TextMessage>(ProcessRemoved, Constants.EventProcessFinished);
            this._subStop= _eventAggregator.Subscribe<TextMessage>(ProcessStopped, Constants.EventProcessStop);

            this._healthSub = _eventAggregator.Subscribe4Broadcast<HealthMessage>(PublishHealth);
        }

        private void PublishHealth(HealthMessage health)
        {
            Logger.Trace($"HealthCheck '{nameof(PublishHealth)}' Start");

            HealthBlock block =new HealthBlock()
            {
                Name = "TaskExecutorPool"
            };

            block.AddMatrix("ActiveProcessConsumersCount", _processConsumer.Count);
            foreach (var pair in _processConsumer)
            {
                HealthBlock taskExecutorHealth = new HealthBlock();
                taskExecutorHealth.AddMatrix("ProcessId", pair.Key);
                pair.Value.UpdateHealthStatus(taskExecutorHealth);

                block.AddMatrix($"Process_{pair.Key}", taskExecutorHealth);
            }
            health.Add(block);
            Logger.Trace($"HealthCheck '{nameof(PublishHealth)}' End");
        }

        protected override void OnStopping()
        {
            base.OnStopping();

            _eventAggregator.Unsubscribe(_healthSub);
            _eventAggregator.Unsubscribe(_subRem);
            _eventAggregator.Unsubscribe(_subStop);
            _subStop = _subRem = null;
        }

        private void ProcessStopped(TextMessage msg)
        {
            Logger.Trace($"{nameof(TaskExecutorsPool)} received ProcessStopped message for process Id {msg.Parameter??string.Empty}");

            StopProcess(msg.Parameter);
        }

        private void StopProcess(string id)
        {
            if (long.TryParse(id, out long processId))
            {
                if (_processConsumer.TryRemove(processId, out ProcessConsumer consumer))
                {
                    consumer.SweeperAction = null;
                    consumer.Stop();
                }
            }
        }

        private void ProcessRemoved(TextMessage msg)
        {
            Logger.Trace($"{nameof(TaskExecutorsPool)} received ProcessRemoved message for process Id {msg.Parameter ?? string.Empty}");
            StopProcess(msg.Parameter);
        }

        public ProcessConsumer Get(long processId, int processKey, long groupId)
        {
            return GetConsumer(processId, processKey, groupId);
        }

        ProcessConsumer GetConsumer(long processId, int processKey, long groupId)
        {
            var consumer = _processConsumer.GetOrAdd(processId, id=> BuildProcessConsumer(id, processKey, groupId));
            return consumer;
        }
        private Bus _bus;
        private Bus Bus
        {
            get { return _bus ?? (_bus = _resolver.Resolve<Bus>()); }
        }

        private ProcessConsumer BuildProcessConsumer(long processId, int processKey, long groupId)
        {
            Logger.Trace($"Process consumer build start for processId {processId}");
            var executionContext = _cacheAside.GetProcessExecutionContext(processId);

            var taskHandler = _processRepository.GetProcessTaskHandler(executionContext.Configuration.ProcessKey);//processKey
            ProcessConsumer consumer = new ProcessConsumer(_token, processKey, processId, groupId, _stateManager, _frameworkLogger, taskHandler, 
                _processRepository.GetSerializer(taskHandler), Bus, _frameworkLogger, _resolver.Resolve<ITaskListenerHandler>(), executionContext);
            consumer.SweeperAction = Trigger;
            consumer.Start();
            consumer.Completion.ContinueWith(c =>
            {
                Logger.Trace($"Process consumer for processId {processId} stopped and removing from storage. Faulted {c.IsFaulted}, Cancel {c.IsCanceled}, completed {c.IsCompleted}");
                _processConsumer.TryRemove(processId, out ProcessConsumer cns);
                //todo publish consumer complete notification
                consumer.SweeperAction = null;
                Robustness.Instance.SafeCall(() =>
                {
                    consumer.Dispose();
                }, Logger);
            });

            Logger.Trace($"Process consumer created processId {processId}");
            return consumer;
        }

        //private ProcessConfiguration GetProcessConfigurationFromPId(int processId)
        //{
        //    //todo get global configuration if process configuration missing
        //    var context = _cacheAside.GetProcessExecutionContext(processId);
        //    return context.Configuration;            
        //}
        
        internal override void PerformIteration()
        {
            foreach (var pair in _processConsumer)
            {
                if(Interrupter.IsCancellationRequested)
                    return;

                //Robustness.Instance.SafeCall(async () =>
                //{
                //    await pair.Value.SweepItems();
                //});

                Robustness.Instance.SafeCall(() =>
                {
                    pair.Value.SweepItems().Wait(_token);
                });

            }
        }
    }
}
