using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;

namespace BusImpl.PubSubImpl.Udp
{
    public class UdpPubSubFactory: IPubSubFactory
    {
        private readonly string _ip;
        private readonly int _port;
        readonly object _lock=new object();
        private UdpPortSubscriber _portSubscriber;
        protected const string PubChannel = "BPEMChannel";
        readonly List<UdpVirtualSubscriber> _activeSubscribers = new List<UdpVirtualSubscriber>();
        private readonly IFrameworkLogger _systemLogger;

        public UdpPubSubFactory(string ip, int port, IBatchLoggerFactory logger)
        {
            _ip = ip;
            _port = port;
            _systemLogger = logger.GetSystemLogger();
            _portSubscriber=new UdpPortSubscriber(_ip, port, _systemLogger, OnUdpMessage);
        }

        private void OnUdpMessage(string channel, string type, string message)
        {
            IEnumerable<UdpVirtualSubscriber> subscribers;
            lock (_lock)
            {
                subscribers = _activeSubscribers.Where(ch=>ch.Channel==channel).ToList();
            }
            foreach (var subscriber in subscribers)
            {
                subscriber.OnMessageReceived(type, message);
            }
        }

        public IDistributedMessagePublisher GetPublisher(CancellationToken token, ILogger logger, string channelName = null)
        {
            return new UdpSingletonPublisher(_ip, _port, channelName, logger);
        }

        public IDistributedMessageSubscriber GetSubscriber(CancellationToken token, ILogger logger, string channelName = null)
        {
            var ch = channelName ?? PubChannel;

            
            UdpVirtualSubscriber virtualSubscriber = new UdpVirtualSubscriber((s) =>
            {
                lock (_lock)
                {
                    _activeSubscribers.Remove(s);
                    _systemLogger.Trace("UDP PubSub channel {channel} removed", s.Channel);
                    if (_activeSubscribers.Count == 0)
                    {
                        _portSubscriber?.Dispose();
                        _portSubscriber = null;
                        _systemLogger.Info("UDP PubSub UDP listener closed due to empty subscribers");
                    }
                }
            }, ch);

            lock (_lock)
            {
                _activeSubscribers.Add(virtualSubscriber);

                if (_portSubscriber == null)
                {
                    _portSubscriber = new UdpPortSubscriber(_ip, _port, _systemLogger, OnUdpMessage);
                }
                _portSubscriber.StartIfNot();
            }

            return virtualSubscriber;
        }
    }
}