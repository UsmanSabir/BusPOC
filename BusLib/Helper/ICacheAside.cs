using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Infrastructure;

namespace BusLib.Helper
{
    /// <summary>
    /// DataStore to avoid repeated access data source
    /// </summary>
    public interface ICacheAside
    {
        //MemoryCache.Default;  //20 mins expiry of pool
        IProcessExecutionContext GetProcessExecutionContext(long processId);

        IProcessConfiguration GetProcessConfiguration(int processKey);

        object GetProcessData(int processId, string dataKey); // might be from central cache, a dictionary of string,object type to store data

        T GetProcessData<T>(int processId, string dataKey); // might be from central cache

        bool IsHealthy();
    }

    //todo
    class CacheAside:SafeDisposable, ICacheAside
    {
        private readonly IStateManager _stateManager;

        readonly ConcurrentDictionary<int, IProcessConfiguration> _processConfigurations=new ConcurrentDictionary<int, IProcessConfiguration>();
        readonly ConcurrentDictionary<long, IProcessExecutionContext> _processExecutionContexts=new ConcurrentDictionary<long, IProcessExecutionContext>();
        private readonly IProcessDataStorage _storage;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly IBatchLoggerFactory _batchLoggerFactory;
        private TinyMessageSubscriptionToken _subRem;

        public CacheAside(IStateManager stateManager, IProcessDataStorage storage, IEventAggregator eventAggregator, ILogger logger, IBatchLoggerFactory batchLoggerFactory)
        {
            _stateManager = stateManager;
            _storage = storage;
            _eventAggregator = eventAggregator;
            _logger = logger;
            _batchLoggerFactory = batchLoggerFactory;
            _subRem =  eventAggregator.Subscribe<TextMessage>(ProcessRemoved, Constants.EventProcessFinished);

        }

        private void ProcessRemoved(TextMessage msg)
        {
            if (long.TryParse(msg.Parameter, out var processId))
            {
                Robustness.Instance.SafeCall(() =>
                {
                    if(_processExecutionContexts.TryRemove(processId, out IProcessExecutionContext pe))
                    {
                        _processConfigurations.TryRemove(pe.ProcessState.ProcessKey, out IProcessConfiguration cnf);
                    }

                    _storage.CleanProcessData(msg.Parameter);
                }, _logger);
            }
        }


        public IProcessExecutionContext GetProcessExecutionContext(long processId)
        {
            var context = _processExecutionContexts.GetOrAdd(processId, id =>
            {
                var process = _stateManager.GetProcessById(processId);
                if (process == null)
                {
                    throw new FrameworkException($"BatchProcess not found for process id {processId}");
                }

                var config = GetProcessConfiguration(process.ProcessKey);
                var executionContext = new ProcessExecutionContext(_batchLoggerFactory.GetProcessLogger(processId, process.ProcessKey, process.CorrelationId), process, config, _storage);
                return executionContext;
            });

            return context;
        }

        public IProcessConfiguration GetProcessConfiguration(int processKey)
        {
            var cfg = _processConfigurations.GetOrAdd(processKey, key =>
            {
                var config = _stateManager.GetProcessConfiguration(key);
                return config;
            });
            if (cfg== null)
            {
                _processConfigurations.TryRemove(processKey, out cfg);
                throw new FrameworkException($"Process configuration not found for process key {processKey}");
            }

            return cfg;
        }

        public object GetProcessData(int processId, string dataKey)
        {
            return null; throw new System.NotImplementedException();
        }

        public T GetProcessData<T>(int processId, string dataKey)
        {
            return default; //throw new System.NotImplementedException();
        }

        public bool IsHealthy()
        {
            return _storage.IsHealthy();
        }

        protected override void Dispose(bool disposing)
        {
            _eventAggregator.Unsubscribe(_subRem);

            base.Dispose(disposing);
        }
    }
}