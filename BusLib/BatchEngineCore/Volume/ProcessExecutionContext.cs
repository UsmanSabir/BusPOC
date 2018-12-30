using System;
using System.Collections.Concurrent;
using BusLib.Core;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore
{
    class ProcessExecutionContext: IProcessExecutionContext
    {
        private readonly IProcessDataStorage _storage;
        readonly ConcurrentDictionary<string,object> _tempData=new ConcurrentDictionary<string, object>();

        public ProcessExecutionContext(ILogger logger, IReadWritableProcessState processState, IProcessConfiguration configuration, IProcessDataStorage storage)
        {
            Logger = logger;
            //ProcessState = processState;
            WritableProcessState = processState;
            Configuration = configuration;
            _storage = storage;
        }

        public IReadWritableProcessState WritableProcessState
        {
            get;
        }

        public ILogger Logger { get; }
        public IDashboardService DashboardService { get; }
        public IProcessState ProcessState => WritableProcessState;

        //private IProcessConfiguration _configuration;
        public IProcessConfiguration Configuration
        {
            get;// { return _configuration ?? (_configuration = GetProcessConfiguration()); }
        }
        
        public bool AddUpdateProcessData<T>(string key, T value)
        {
            _storage.AddOrUpdateProcessData(ProcessState.Id, key, value);
            return true;
        }

        public T GetProcessData<T>(string key)
        {
            return _storage.GetProcessData<T>(ProcessState.Id, key);
        }

        public object GetProcessData(string key)
        {
            return _storage.GetProcessData<object>(ProcessState.Id, key);
        }

        public bool SetTempData(string key, object value)
        {
            _tempData.AddOrUpdate(key, value, (s, o) => value);
            return true;
        }

        public T GetTempData<T>(string key)
        {
            if (_tempData.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return default;
        }

        public object GetTempData(string key)
        {
            return GetTempData<object>(key);
        }
    }
}