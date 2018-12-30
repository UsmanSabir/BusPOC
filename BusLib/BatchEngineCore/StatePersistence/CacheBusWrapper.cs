using System;
using BusLib.Core;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.StatePersistence
{
    public class CacheBusWrapper:IProcessDataStorage
    {
        private readonly IProcessDataStorage _processDataStorageImplementation;
        private readonly ILogger _logger;

        public CacheBusWrapper(ILogger logger, IProcessDataStorage processDataStorageImplementation)
        {
            _logger = logger;
            _processDataStorageImplementation = processDataStorageImplementation;
        }

        T Execute<T>(Func<T> func)
        {
            T ret = default;

            void Action() => ret = func();

            Execute(Action);

            return ret;
        }

        void Execute(Action act)
        {
            ActionCommand action = new ActionCommand(act);

            try
            {
                Bus.Instance.HandleCacheStorageCommand(action);
            }
            catch (Exception e)
            {
                _logger.Fetal($"Error executing state command. {e.Message}", e);
            }
        }

        public void AddOrUpdateProcessData<T>(long processStateId, string key, T value)
        {
            Execute(()=>_processDataStorageImplementation.AddOrUpdateProcessData(processStateId, key, value));
        }

        public T GetProcessData<T>(long processStateId, string key)
        {
            return Execute(() => _processDataStorageImplementation.GetProcessData<T>(processStateId, key));
        }

        public void CleanProcessData(string processId)
        {
            Execute(() => _processDataStorageImplementation.CleanProcessData(processId));
        }

        public bool IsHealthy()
        {
            return _processDataStorageImplementation.IsHealthy();
        }

        public void RefreshIfNotHealth()
        {
            _processDataStorageImplementation.RefreshIfNotHealth();
        }
    }
}