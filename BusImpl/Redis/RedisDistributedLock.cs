using System;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore;
using BusLib.Core;
using BusLib.Helper;
using BusLib.ProcessLocks;
using StackExchange.Redis;


namespace BusImpl.Redis
{
    public class RedisDistributedLock: DistributedMutex
    {
        private readonly string _connection;
        //private StackExchangeRedisCacheClient _redis;
        private ConnectionMultiplexer _redis;
        private readonly RedisSerializer _serializer;
        private IDatabase _lockedDb;
        private string _lockId;
        private const int lockUpdateMargin = 1;

        public RedisDistributedLock(string connection, string key,
            Func<CancellationToken, Task> taskToRunWhenLockAcquired, Action secondaryAction, IFrameworkLogger logger) : base(key, taskToRunWhenLockAcquired, secondaryAction, logger)
        {
            _connection = connection;
            _serializer = new RedisSerializer();
        }


        ConnectionMultiplexer SafeGetClient(CancellationToken token)
        {
            if (_redis != null)
                return _redis;

            Robustness.Instance.ExecuteUntilTrue(CreateClient, token);
            return _redis;
        }

        private void CreateClient()
        {
            _redis = ConnectionMultiplexer.Connect(_connection);
            //_redis = new StackExchangeRedisCacheClient(_serializer, _connection);
        }


        protected override async Task<string> AcquireLockAsync(string key, CancellationToken token)
        {
            var client = SafeGetClient(token);
            if (token.IsCancellationRequested)
                return string.Empty;

            var nodeName = NodeSettings.Instance.Name;
            var db = client.GetDatabase(); //.SetEntryIfNotExists(_lockKey, _lockValue);

            bool success = await db.LockTakeAsync(key, nodeName, TimeSpan.FromSeconds(RenewIntervalSecs + lockUpdateMargin));

            if (success)
            {
                _lockedDb = db;
                _lockId = nodeName;
                return nodeName;
            }

            return string.Empty;
        }

        protected override void ReleaseLock(string lockId)
        {
            Robustness.Instance.SafeCall(()=>
            {
                _lockedDb?.LockRelease(Key, lockId);
            });
            _lockId = string.Empty;
        }

        protected override async Task<bool> RenewLockAsync(string lockId, CancellationToken token)
        {
            //var client = SafeGetClient(token);
            //if (client==null)
            //    return false;

            var res = _lockedDb != null && await _lockedDb.LockExtendAsync(Key, lockId, TimeSpan.FromSeconds(RenewIntervalSecs + lockUpdateMargin));
            if (!res && _lockedDb!=null)
            {
                _lockId = await AcquireLockAsync(Key, token); //try again
                if (!string.IsNullOrWhiteSpace(_lockId))
                {
                    res = true;
                    return true;
                }
                var nodeName = NodeSettings.Instance.Name;
                ReleaseLock(nodeName);
                _lockedDb = null;
            }
            return res;
        }
    }
}