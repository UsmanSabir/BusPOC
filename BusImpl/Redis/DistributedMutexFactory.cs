using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Core;
using BusLib.ProcessLocks;

namespace BusImpl.Redis
{
    public class DistributedMutexFactory: IDistributedMutexFactory
    {
        private readonly string _connection;

        public DistributedMutexFactory(string connection)
        {
            _connection = connection;
        }

        public DistributedMutex CreateDistributedMutex(string key,
            Func<CancellationToken, Task> taskToRunWhenLockAcquired, Action secondaryAction, IFrameworkLogger logger)
        {
            return new RedisDistributedLock(_connection, key, taskToRunWhenLockAcquired, secondaryAction, logger);
        }
    }
}