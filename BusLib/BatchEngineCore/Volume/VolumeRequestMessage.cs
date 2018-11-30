using System;
using BusLib.Core;

namespace BusLib.BatchEngineCore
{
    class ProcessExecutionContext: IProcessExecutionContext
    {
        public ProcessExecutionContext(ILogger logger, IProcessState processState)
        {
            Logger = logger;
            ProcessState = processState;
        }

        public ILogger Logger { get; }
        public IDashboardService DashboardService { get; }
        public IProcessState ProcessState { get; }

        private ProcessConfiguration _configuration;
        public ProcessConfiguration Configuration
        {
            get { return _configuration ?? (_configuration = GetProcessConfiguration()); }
        }

        private ProcessConfiguration GetProcessConfiguration()
        {
            throw new NotImplementedException();
        }

        public bool AddUpdateProcessData(string key, object value)
        {
            throw new System.NotImplementedException();
        }

        public T GetProcessData<T>(string key)
        {
            throw new System.NotImplementedException();
        }

        public object GetProcessData(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool SetTempData(string key, object value)
        {
            throw new System.NotImplementedException();
        }

        public T GetTempData<T>(string key)
        {
            throw new System.NotImplementedException();
        }

        public object GetTempData(string key)
        {
            throw new System.NotImplementedException();
        }
    }
}