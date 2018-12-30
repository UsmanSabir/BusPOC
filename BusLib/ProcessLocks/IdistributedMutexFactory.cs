using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLib.ProcessLocks
{
    public interface IDistributedMutexFactory
    {
        DistributedMutex CreateDistributedMutex(string key, Func<CancellationToken, Task> taskToRunWhenLockAcquired,
            Action secondaryAction);
    }
}