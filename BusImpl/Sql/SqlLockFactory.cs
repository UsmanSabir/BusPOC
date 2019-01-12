using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Core;
using BusLib.ProcessLocks;

namespace BusImpl.Sql
{
    public class SqlLockFactory: IDistributedMutexFactory
    {
        public DistributedMutex CreateDistributedMutex(string key, Func<CancellationToken, Task> taskToRunWhenLockAcquired, Action secondaryAction,
            IFrameworkLogger logger)
        {
            return new SqlDistributedLock(key, taskToRunWhenLockAcquired, secondaryAction, logger);
        }
    }
}