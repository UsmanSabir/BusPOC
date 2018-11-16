using System;

namespace BusLib.Core
{
    //available distributed lock providers are redis cache, mongodb, sql server, azure blob, app fabric
    public interface ILockFactory
    {
        /// <summary>
        /// Acquire a lock 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="lockWaitMillis"></param>
        /// <returns></returns>
        ILock AcquireLock(string key, int lockWaitMillis);

        /// <summary>
        /// Try to acquire a lock or fail if lock already taken
        /// </summary>
        /// <param name="key"></param>
        /// <param name="locker"></param>
        /// <param name="timeoutMillis"></param>
        /// <returns></returns>
        bool TryAcquireLock(string key, out ILock locker, int timeoutMillis=-1);
    }

    public interface ILock : IDisposable
    {

    }
}