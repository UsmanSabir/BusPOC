using System;
using System.Diagnostics;
using BusLib.BatchEngineCore;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Infrastructure;
using StackExchange.Redis;

namespace BusImpl.Redis
{
    public class RedisProcessStorage: IProcessDataStorage
    {
        IDatabase _db;
        //readonly ICacheClient _redis;
        private ConnectionMultiplexer _redis;
        private readonly string _name;
        private readonly string _connection;
        private readonly ISerializer _serializer;

        public RedisProcessStorage(string name, string connection, ISerializer serializer)//, int port= 6379)
        {
            _name = name;
            _connection = connection;
            _serializer = serializer;


            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connection);
            //Db = redis.GetDatabase();
            //Db.HashSet("user:user1", new HashEntry[] { new HashEntry("12", "13"), new HashEntry("14", "15") });

            //var config = ConfigurationOptions.Parse(connection);
            //var mux = ConnectionMultiplexer.Connect(config);

            //Db = mux.GetDatabase();


            ////var connectionString = mux.ToString();

            //this._redis =  new StackExchangeRedisCacheClient(serializer, connection); //redis0:6380
            CreateClient();
        }

        string GetHashKey(string processId)
        {
            return $"{_name}_process{processId}";
        }
        
        public void AddOrUpdateProcessData<T>(long processStateId, string key, T value)
        {
            var k = GetHashKey(processStateId.ToString());

            var s = _serializer.SerializeToString(value);

            _db.HashSet(k, new HashEntry[] { new HashEntry(key, s)}); //, new HashEntry("14", "15") })};
            //Db.HashSet()
            //Db.HashSet(k, key, value);
        }

        public T GetProcessData<T>(long processStateId, string key)
        {
            var k = GetHashKey(processStateId.ToString());
            string redisValue = _db.HashGet(k,key);
            if (string.IsNullOrWhiteSpace(redisValue))
                return default(T);

            var item = _serializer.DeserializeFromString<T>(redisValue);
            //var item = _redis.HashGet<T>(k, key);
            return  item;
        }

        public void CleanProcessData(string processId)
        {
            var k = GetHashKey(processId);

            _db.KeyDelete(k);
            //_redis.Remove(k);
            //_redis.HashDelete(k)
        }

        public bool IsHealthy()
        {
            try
            {
                var key = $"{NodeSettings.Instance.Name}_{Process.GetCurrentProcess()?.Id??1}";
                var value = _db.StringGetSet(key, "Ping");
                _db.KeyDelete(key);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void CreateClient()
        {
            _redis = ConnectionMultiplexer.Connect(_connection);
            _db = _redis.GetDatabase();
        }

        
        public void RefreshIfNotHealth()
        {
            if (!IsHealthy())
            {
                CreateClient();
            }
        }
    }
}