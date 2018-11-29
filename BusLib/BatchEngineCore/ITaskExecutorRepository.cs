using BusLib.BatchEngineCore.Handlers;
using BusLib.Core;
using BusLib.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    interface ITaskExecutorRepository
    {
        ProcessConsumer Get(int processId);



    }

    class TaskExecutorRepository : ITaskExecutorRepository
    {
        readonly ConcurrentDictionary<int, ProcessConsumer> _processConsumer = new ConcurrentDictionary<int, ProcessConsumer>();
        readonly IStateManager _stateManager;
        private CancellationToken _token;
        readonly ILogger _logger;
        ICacheAside _cacheAside;

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
            _logger.Trace($"Process consumer build start for processId {processId}");
            ProcessConsumer consumer = new ProcessConsumer(_token, processId, _stateManager, LoggerFactory.GetSystemLogger(), _cacheAside);
            consumer.Start();
            consumer.Completion.ContinueWith(c =>
            {
                _logger.Trace($"Process consumer for processId {processId} stopped and removing from storage");
                _processConsumer.TryRemove(processId, out ProcessConsumer cns);
            });

            _logger.Trace($"Process consumer created processId {processId}");
            return consumer;
        }

        //private ProcessConfiguration GetProcessConfigurationFromPId(int processId)
        //{
        //    //todo get global configuration if process configuration missing
        //    var context = _cacheAside.GetProcessExecutionContext(processId);
        //    return context.Configuration;            
        //}
    }
}
