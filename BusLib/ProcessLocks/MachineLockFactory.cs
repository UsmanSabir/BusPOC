using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using BusLib.Core;

namespace BusLib.ProcessLocks
{
    class MachineLockFactory:ILockFactory
    {
        //https://stackoverflow.com/a/2590446
        private const string GlobalPrefix = "Global\\";

        public ILock AcquireLock(string key, int lockWaitMillis)
        {
            var security = new EventWaitHandleSecurity();
            // allow anyone to wait on and signal this lock
            security.AddAccessRule(
                new EventWaitHandleAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, domainSid: null),
                    EventWaitHandleRights.FullControl, // doesn't seem to work without this :-/
                    AccessControlType.Allow
                )
            );

            bool ignored;
            var @event = new EventWaitHandle(
                // if we create, start as unlocked
                initialState: true,
                // allow only one thread to hold the lock
                mode: EventResetMode.AutoReset,
                name: GlobalPrefix + key,
                createdNew: out ignored
            );

            try
            {
                @event.SetAccessControl(security);

                if (@event.WaitOne(TimeSpan.FromMilliseconds(lockWaitMillis)))
                {
                    var locker = new SystemLock(@event);
                    return locker;
                }

                throw new OperationCanceledException("Unable to acquire lock");
            }
            catch
            {
                @event.Dispose();
                throw;
            }
        }

        public bool TryAcquireLock(string key, out ILock locker, int timeoutMillis)
        {
            locker = null;
            try
            {
                locker = AcquireLock(key, timeoutMillis);
            }
            catch (Exception e)
            {
                //todo log
            }
            return false;
        }
    }
}