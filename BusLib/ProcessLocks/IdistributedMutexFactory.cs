using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Core;

namespace BusLib.ProcessLocks
{
    public interface IDistributedMutexFactory
    {
        DistributedMutex CreateDistributedMutex(string key, Func<CancellationToken, Task> taskToRunWhenLockAcquired,
            Action secondaryAction, IFrameworkLogger logger);
    }
}