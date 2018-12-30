using System.Threading;
using BusLib.Core;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IPubSubFactory
    {
        IDistributedMessagePublisher GetPublisher(CancellationToken token, ILogger logger, string channelName = null);

        IDistributedMessageSubscriber GetSubscriber(CancellationToken token, ILogger logger, string channelName = null);
    }
}