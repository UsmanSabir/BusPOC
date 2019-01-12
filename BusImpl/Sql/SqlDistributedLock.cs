using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore;
using BusLib.Core;
using BusLib.Helper;
using BusLib.ProcessLocks;

namespace BusImpl.Sql
{
    public class SqlDistributedLock: DistributedMutex
    {
        //based on https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-2017

        private const string _sqlLockCommand = "sys.sp_GetAppLock";

        System.Data.IDbTransaction _uow;

        public SqlDistributedLock(string key, Func<CancellationToken, Task> taskToRunWhenLockAcquired, Action secondaryAction, IFrameworkLogger logger) : base(key, taskToRunWhenLockAcquired, secondaryAction, logger)
        {
        }

        protected override Task<string> AcquireLockAsync(string key, CancellationToken token)
        {
            string lockKey = NodeSettings.Instance.LockKey;

            ReleaseLock(lockKey);
            
            //_uow = Create();
            
            try
            {
                var res = 0;// execute below command
                    //connection.ScalarFunctionCall<int>( 
                    //_sqlLockCommand, new Dictionary<string, object>()
                    //{
                    //    {"Resource", lockKey },
                    //    {"LockMode","Exclusive" }, //Shared,Update,Exclusive
                    //    {"LockOwner","Transaction" },
                    //    {"LockTimeout", "100" }, // don't wait

                    //}, false);

                Logger.Trace("SQLLock response received with value {lockId}", res);

                if (res==0 || res==1)
                {
                    //lock taken
                    //Value Result
                    //0   The lock was successfully granted synchronously.
                    //1   The lock was granted successfully after waiting for other incompatible locks to be released.
                    //- 1  The lock request timed out.
                    //-2  The lock request was canceled.
                    //-3  The lock request was chosen as a deadlock victim.
                    //-999    Indicates a parameter validation or other call error.
                    Logger.Info("SQLLock master lock taken with result value {lockId}", res);
                    return Task.FromResult(lockKey);
                }

                //Console.WriteLine(res);
            }
            catch (Exception e)
            {
                Logger.Error("Error while taking lock {error}", e);
            }

            ReleaseLock(lockKey);//close transaction / connection
            return Task.FromResult(string.Empty);
        }

        protected override void ReleaseLock(string lockId)
        {
            Robustness.Instance.SafeCall(() =>
            {
                _uow?.Dispose();
                _uow = null;
            });
        }

        protected override Task<bool> RenewLockAsync(string lockId, CancellationToken token)
        {
            try
            {
                //ExecuteSql("SELECT 'A'", null); //just ping query
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Logger.Warn("Error while Renewing lock {error}", e);

                ReleaseLock(lockId);

                var locker = AcquireLockAsync(NodeSettings.Instance.LockKey, token).Result;

                return Task.FromResult(!string.IsNullOrWhiteSpace(locker));
            }
        }
    }
}