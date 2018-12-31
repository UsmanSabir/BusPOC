using System.Threading;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;

namespace BusImpl.Redis
{
    public class RedisPubSubFactory: IPubSubFactory
    {
        private readonly string _connection;

        public RedisPubSubFactory(string connection)
        {
            _connection = connection;
        }
        public IDistributedMessagePublisher GetPublisher(CancellationToken token, ILogger logger,
            string channelName = null)
        {
            return new RedisPublisher(_connection, token, channelName, logger);
        }

        public IDistributedMessageSubscriber GetSubscriber(CancellationToken token, ILogger logger,
            string channelName = null)
        {
            return new RedisSubscriber(_connection, token, channelName, logger);
        }
    }
}