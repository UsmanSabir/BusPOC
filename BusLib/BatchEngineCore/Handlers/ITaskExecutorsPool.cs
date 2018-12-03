using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Handlers
{
    interface ITaskExecutorsPool
    {
        ProcessConsumer Get(int processId);
    }

    class TaskExecutorsPool : RepeatingProcess, ITaskExecutorsPool
    {
        readonly ConcurrentDictionary<int, ProcessConsumer> _processConsumer = new ConcurrentDictionary<int, ProcessConsumer>();
        readonly IStateManager _stateManager;
        private CancellationToken _token;
        ICacheAside _cacheAside;
        private const int TaskTimoutCheckInterval = 2000;
        
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

        public TaskExecutorsPool(ILogger logger) : base(nameof(TaskExecutorsPool), logger)
        {
            Start(_token);
        }


        public ProcessConsumer Get(int processId)
        {
            return GetConsumer(processId);
        }

        ProcessConsumer GetConsumer(int processId)
        {
            var consumer = _processConsumer.GetOrAdd(processId, BuildProcessConsumer);
            return consumer;
        }

        private ProcessConsumer BuildProcessConsumer(int processId)
        {
            Logger.Trace($"Process consumer build start for processId {processId}");
            ProcessConsumer consumer = new ProcessConsumer(_token, processId, _stateManager, LoggerFactory.GetSystemLogger(), _cacheAside);
            consumer.Start();
            consumer.Completion.ContinueWith(c =>
            {
                Logger.Trace($"Process consumer for processId {processId} stopped and removing from storage");
                //todo publish consumer complete notification
                _processConsumer.TryRemove(processId, out ProcessConsumer cns);
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

                Robustness.Instance.SafeCall(async () =>
                {
                    await pair.Value.SweepItems();
                });
                
            }
        }
    }
}
