using System;
using System.Collections.Concurrent;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;
using BusLib.Infrastructure;
using BusLib.Serializers;

namespace BusLib.BatchEngineCore
{
    class ProcessExecutionContext: IProcessExecutionContextWithVolume
    {
        private readonly IProcessDataStorage _storage;
        readonly ConcurrentDictionary<string,object> _tempData=new ConcurrentDictionary<string, object>();
        private bool _hasVolume = false;

        public ProcessExecutionContext(ILogger logger, IReadWritableProcessState processState,
            IProcessConfiguration configuration, IProcessDataStorage storage,
            IReadWritableGroupEntity groupDetailsGroupEntity)
        {
            Logger = logger;
            //ProcessState = processState;
            WritableProcessState = processState;
            Configuration = configuration;
            _storage = storage;
            GroupEntity = groupDetailsGroupEntity;
            Criteria =
                SerializersFactory.Instance.DefaultSerializer.DeserializeFromString<JobCriteria>(processState.Criteria);
        }

        public IReadWritableProcessState WritableProcessState
        {
            get;
            private set;
        }

        public ILogger Logger { get; }
        public IDashboardService DashboardService { get; }
        public IProcessState ProcessState => WritableProcessState;
        public DateTime ProcessingDate => ProcessState.ProcessingDate;
        public JobCriteria Criteria { get; }

        //private IProcessConfiguration _configuration;
        public IProcessConfiguration Configuration
        {
            get;// { return _configuration ?? (_configuration = GetProcessConfiguration()); }
        }

        internal bool HasVolume
        {
            get { return _hasVolume || ProcessState.HasVolume; }
        }

        public IReadWritableGroupEntity GroupEntity { get; }

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

        public void SetVolumeGenerated()
        {
            _hasVolume = true;
        }

        internal void UpdateProcessEntity(IReadWritableProcessState state)
        {
            WritableProcessState = state;
        }
    }
}