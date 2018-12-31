using System;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.StatePersistence
{
    public class DbCommandWrapper
    {
        private readonly ILogger _logger;
        private Bus _bus;
        public DbCommandWrapper(ILogger logger, IResolver resolver)
        {
            _logger = logger;
            _bus = resolver.Resolve<Bus>();
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
                _bus.HandleCacheStorageCommand(action);
            }
            catch (Exception e)
            {
                _logger.Fatal($"Error executing state command. {e.Message}", e);
            }
        }

    }
}