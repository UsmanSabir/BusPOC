using System;
using BusLib.Core;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.StatePersistence
{
    public class DbCommandWrapper
    {
        private readonly ILogger _logger;

        public DbCommandWrapper(ILogger logger)
        {
            _logger = logger;

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

    }
}