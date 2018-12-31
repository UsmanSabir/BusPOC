using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Helper;
using StackExchange.Redis;

namespace BusImpl.Redis
{
    abstract class RedisPubSubClient: SafeDisposable
    {
        private readonly string _connection;
        private readonly CancellationToken _token;

        protected readonly ILogger Logger;

        //private StackExchangeRedisCacheClient _redis;
        private ConnectionMultiplexer _redis;
        protected const string PubChannel = "BPEMChannel";

        private readonly string _customChannel;

        //private static Lazy<ConfigurationOptions> configOptions = new Lazy<ConfigurationOptions>(() =>
        //{
        //    var configOptions = new ConfigurationOptions();
        //    configOptions.EndPoints.Add("10.26.130.170:6379");
        //    configOptions.ClientName = "MyAppRedisConn";
        //    configOptions.ConnectTimeout = 100000;
        //    configOptions.SyncTimeout = 100000;
        //    configOptions.AbortOnConnectFail = true;
        //    return configOptions;
        //});

        //private static Lazy<ConnectionMultiplexer> LazyRedisconn = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configOptions.Value));



        protected RedisPubSubClient(string connection, CancellationToken token, ILogger logger, string customChannel)
        {
            _connection = connection;
            _token = token;
            Logger = logger;
            _customChannel = customChannel??PubChannel;
            this.Serializer = new RedisSerializer();
            
            CreateClient();
        }

        protected string CustomChannel
        {
            get { return _customChannel; }
        }

        protected RedisSerializer Serializer { get; }

        private void CreateClient()
        {
            //https://stackexchange.github.io/StackExchange.Redis/Configuration.html
            ConfigurationOptions options = ConfigurationOptions.Parse(_connection);//$"{_connection};keepAlive=60;"
            //options.ReconnectRetryPolicy = new LinearRetry();

            _redis = ConnectionMultiplexer.Connect(options);//todo configure retry timeout etc
            
            
            //StackExchangeRedisCacheClient(_serializer, _connection);
            OnClientCreated(_redis);
        }

        protected abstract void OnClientCreated(ConnectionMultiplexer redis);

        protected IDatabase GetClient()
        {
            try
            {
                _redis.GetDatabase().StringGet("Ping");
            }
            catch (Exception e)
            {
                _redis?.Dispose();
                _redis = null;

                Robustness.Instance.ExecuteUntilTrue(() =>
                {
                    CreateClient();
                }, _token);
                
            }
            return _redis.GetDatabase();
        }

        protected override void Dispose(bool disposing)
        {
            _redis?.Dispose();
            base.Dispose(disposing);
        }
    }

    internal class RedisPublisher:RedisPubSubClient, IDistributedMessagePublisher
    {
        public RedisPublisher(string connection, CancellationToken token, string customChannel, ILogger logger) : base(connection, token, logger, customChannel)
        {

        }

        string PackPubMessage(string message, string type)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}::{1}::0::0::", NodeSettings.Instance.Name, type); //3 and 4 reserved
            sb.Append(message);
            return sb.ToString();
        }

        protected override void OnClientCreated(ConnectionMultiplexer redis)
        {
            
        }

        public void PublishMessage(string message, string type)
        {
            var packPubMessage = PackPubMessage(message, type);
            var count = GetClient().Publish(CustomChannel, packPubMessage);
            Logger.Trace($"PubSub message published to {count} subscribers with body '{packPubMessage}'");
        }

        public void PublishMessage<T>(T message)
        {
            var json = Serializer.SerializeToString(message);
            var type = message.GetType().Name;
            var packPubMessage = PackPubMessage(json, type);
            var count = GetClient().Publish(CustomChannel, packPubMessage);
            Logger.Trace($"PubSub message published to {count} subscribers with body '{packPubMessage}'");
        }

    }

    class RedisSubscriber : RedisPubSubClient, IDistributedMessageSubscriber
    {
        ConcurrentBag<KeyValuePair<string, Action<string>>> _subscriptions = new ConcurrentBag<KeyValuePair<string, Action<string>>>();
        private ISubscriber _subscriber;

        public RedisSubscriber(string connection, CancellationToken token, string customChannel, ILogger logger):base(connection, token, logger, customChannel)
        {
            
        }

        protected override void OnClientCreated(ConnectionMultiplexer redis)
        {
            Robustness.Instance.SafeCall(()=>_subscriber?.UnsubscribeAll());
            _subscriber = redis.GetSubscriber();
            _subscriber.Subscribe(CustomChannel, OnMessageReceived);
            
            //foreach (var pair in _subscriptions)
            //{
            //    sub.Subscribe(pair.Key, pair.Value);
            //}
        }

        private void OnMessageReceived(RedisChannel ch, RedisValue val)
        {
            Logger.Trace($"Pubsub message received.{Environment.NewLine} {val}");
            var channel = ch.ToString();
            string message = val;
            var msgParts = message.Split(new[] { "::" }, StringSplitOptions.None);
            if (msgParts.Length < 5)
            {
                Logger.Warn($"Invalid pubsub message, Dropping.{Environment.NewLine} {message}");
                return;
            }
            var nodeName = msgParts[0];
            var type = msgParts[1];
            //3 and 4 reserved
            var packet = msgParts[4];

            Logger.Trace($"PubSub packet received from node {nodeName}, on channel {channel} message, {message}");
            //var isWatchDogMsg = type == nameof(IWatchDogMessage);
            foreach (var pair in _subscriptions)
            {
                if (type == pair.Key)
                {
                    Robustness.Instance.SafeCall(()=>
                    {
                        pair.Value.BeginInvoke(packet, null, null);
                    });
                }
            }
        }

        public void Subscribe(string message, Action<string> action)
        {
            var cl = GetClient(); //just to initialize
            _subscriptions.Add(new KeyValuePair<string, Action<string>>(message, action)); //todo, do we need to store weak reference
        }

        public void Subscribe<T>(Action<T> action)
        {
            if (action == null)
                return;

            void ActString(string s)
            {
                var item = Serializer.DeserializeFromString<T>(s);

                //WeakAction w=new WeakAction(action);//todo, do we need to store weak reference
                action(item);
            }

            var typeName = typeof(T).Name;

            var cl = GetClient(); //just to initialize
            _subscriptions.Add(new KeyValuePair<string, Action<string>>(typeName, ActString)); //todo, do we need to store weak reference
        }


        
        protected override void Dispose(bool disposing)
        {
            Robustness.Instance.SafeCall(() =>
            {
                _subscriber?.UnsubscribeAll();
                _subscriber = null;
            });
            _subscriptions = null;
            base.Dispose(disposing);
        }
    }
}