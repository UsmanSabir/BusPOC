using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BusImpl.Redis;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Helper;

namespace BusImpl.PubSubImpl.Udp
{
    #region UdpSubscriber (commented)

    //    public class UdpSubscriber: IDistributedMessageSubscriber
    //    {
    //        private UdpClient _udpClient;
    //        private string _ip;
    //        private int _port;
    //        private Thread _receiveThread;
    //        private bool _stop = false;
    //        readonly ConcurrentBag<KeyValuePair<string, Action<string>>> _subscriptions = new ConcurrentBag<KeyValuePair<string, Action<string>>>();
    //        private readonly ILogger _logger;
    //        private readonly RedisSerializer _serializer;
    //        protected const string PubChannel = "BPEMChannel";
    //        private readonly string _customChannel;

    //        public UdpSubscriber(string ip, int port, string channel, ILogger logger)
    //        {
    //            _port = port;
    //            _logger = logger;
    //            _ip = ip;
    //            _serializer = new RedisSerializer();
    //            _customChannel = channel ?? PubChannel;
    //        }


    //        public void Dispose()
    //        {
    //            _stop = true;
    //            Robustness.Instance.SafeCall(() => { _udpClient?.Dispose(); });
    //            _receiveThread?.Interrupt();
    //            _receiveThread?.Abort();
    //        }

    //        public UdpClient StartIfNot()
    //        {
    //            if (_udpClient == null)
    //            {
    //                _stop = false;
    //                _udpClient = new UdpClient();
    //                _udpClient.ExclusiveAddressUse = false;
    //                IPEndPoint localEp = new IPEndPoint(IPAddress.Any, _port);
    //                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    //                _udpClient.ExclusiveAddressUse = false;

    //                _udpClient.JoinMulticastGroup(IPAddress.Parse(_ip), 50);
    //                _udpClient.Client.Bind(localEp);
    //                _udpClient.MulticastLoopback = true;

    //                _receiveThread = new Thread(Receive);
    //#if !DEBUG
    //                _receiveThread.IsBackground = true;
    //#endif
    //                _receiveThread.Start();
    //            }

    //            return _udpClient;
    //        }

    //        public void Receive()
    //        {
    //            while (!_stop)
    //            {
    //                Robustness.Instance.SafeCall(() =>
    //                {
    //                    var ipEndPoint = new IPEndPoint(IPAddress.Any, _port);

    //                    //var result = _udpClient.BeginReceive()
    //                    var data = _udpClient.Receive(ref ipEndPoint);

    //                    var message = Encoding.UTF8.GetString(data);

    //                    OnMessageReceived(message, ipEndPoint.Address.ToString());
    //                    // Raise the AfterReceive event
    //                    Console.WriteLine(message);
    //                }, _logger);
    //            }
    //        }

    //        private void OnMessageReceived(string message, string endPoint)
    //        {
    //            _logger.Trace("UDP Pubsub message received from endPoint '{endPoint}' and {message}", endPoint, message);
    //            //var channel = ch.ToString();
    //            //string message = val;
    //            var msgParts = message.Split(new[] { "::" }, StringSplitOptions.None);
    //            if (msgParts.Length < 6)
    //            {
    //                _logger.Warn("UDP Invalid pubsub message from endPoint {endPoint}, Dropping. {message}", endPoint, message);
    //                return;
    //            }
    //            var channel = msgParts[0];
    //            if (!channel.Equals(_customChannel))
    //            {
    //                _logger.Info("UDP pubsub channel {channel} received message from channel {messageChannel}, Dropping", _customChannel, channel);
    //                return;
    //            }
    //            var nodeName = msgParts[1];
    //            var type = msgParts[2];
    //            //3 and 4 reserved
    //            var packet = msgParts[5];

    //            _logger.Trace($"UDP PubSub packet received from node {nodeName}, on channel {channel} message, {message}");
    //            //var isWatchDogMsg = type == nameof(IWatchDogMessage);
    //            foreach (var pair in _subscriptions)
    //            {
    //                if (type == pair.Key)
    //                {
    //                    Robustness.Instance.SafeCall(() =>
    //                    {
    //                        pair.Value.BeginInvoke(packet, null, null);
    //                    });
    //                }
    //            }
    //        }

    //        public void Subscribe(string message, Action<string> action)
    //        {
    //            var cl = StartIfNot(); //just to initialize
    //            _subscriptions.Add(new KeyValuePair<string, Action<string>>(message, action)); //todo, do we need to store weak reference
    //        }

    //        public void Subscribe<T>(Action<T> action)
    //        {
    //            if (action == null)
    //                return;

    //            void ActString(string s)
    //            {
    //                var item = _serializer.DeserializeFromString<T>(s);

    //                //WeakAction w=new WeakAction(action);//todo, do we need to store weak reference
    //                action(item);
    //            }

    //            var typeName = typeof(T).Name;

    //            Subscribe(typeName, ActString);

    //            //var cl = GetClient(); //just to initialize
    //            //_subscriptions.Add(new KeyValuePair<string, Action<string>>(typeName, ActString)); //todo, do we need to store weak reference
    //        }
    //    }

    #endregion

    public class UdpPortSubscriber
    {
        private UdpClient _udpClient;
        private string _ip;
        private int _port;
        private Thread _receiveThread;
        private bool _stop = false;
        //readonly ConcurrentBag<KeyValuePair<string, Action<string>>> _subscriptions = new ConcurrentBag<KeyValuePair<string, Action<string>>>();
        private readonly ILogger _logger;
        private readonly RedisSerializer _serializer;
        //protected const string PubChannel = "BPEMChannel";
        //private readonly string _customChannel;

        internal Action<string, string, string> OnMessage { get; private set; } //channel, type, message

        public UdpPortSubscriber(string ip, int port, ILogger logger, Action<string, string, string> onMessage)
        {
            _port = port;
            _logger = logger;
            OnMessage = onMessage;
            _ip = ip;
            _serializer = new RedisSerializer();
            //_customChannel = channel ?? PubChannel;
        }


        public void Dispose()
        {
            _stop = true;
            Robustness.Instance.SafeCall(() => { _udpClient?.Dispose(); });
            Robustness.Instance.SafeCall(() =>
            {
                _receiveThread?.Interrupt();
                _receiveThread?.Abort();
            });
        }

        public UdpClient StartIfNot()
        {
            if (_udpClient == null)
            {
                _stop = false;
                _udpClient = new UdpClient();
                _udpClient.ExclusiveAddressUse = false;
                IPEndPoint localEp = new IPEndPoint(IPAddress.Any, _port);
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.ExclusiveAddressUse = false;

                _udpClient.JoinMulticastGroup(IPAddress.Parse(_ip), 64);
                _udpClient.AllowNatTraversal(true);
                _udpClient.Client.Bind(localEp);
                _udpClient.MulticastLoopback = true;

                _receiveThread = new Thread(Receive);
#if !DEBUG
                _receiveThread.IsBackground = true;
#endif
                _receiveThread.Start();
            }

            return _udpClient;
        }

        public void Receive()
        {
            while (!_stop)
            {
                Robustness.Instance.SafeCall(() =>
                {
                    var ipEndPoint = new IPEndPoint(IPAddress.Any, _port);

                    //var result = _udpClient.BeginReceive()
                    var data = _udpClient.Receive(ref ipEndPoint);

                    var message = Encoding.UTF8.GetString(data);

                    OnMessageReceived(message, ipEndPoint.Address.ToString());
                    // Raise the AfterReceive event
                    Console.WriteLine(message);
                }, _logger);
            }
        }

        private void OnMessageReceived(string message, string endPoint)
        {
            _logger.Trace("UDP Pubsub message received from endPoint '{endPoint}' and {message}", endPoint, message);
            //var channel = ch.ToString();
            //string message = val;
            var msgParts = message.Split(new[] { "::" }, StringSplitOptions.None);
            if (msgParts.Length < 6)
            {
                _logger.Warn("UDP Invalid pubsub message from endPoint {endPoint}, Dropping. {message}", endPoint, message);
                return;
            }
            var channel = msgParts[0];
            var nodeName = msgParts[1];
            var type = msgParts[2];
            //3 and 4 reserved
            var packet = msgParts[5];

            
            //if (nodeName == NodeSettings.Instance.Name)
            //{
            //    _logger.Info($"UDP dropping Loopback PubSub message received from node {nodeName}, on channel {channel} message, {message}");
            //    return;
            //}
            //else
            {
                _logger.Trace($"UDP PubSub packet received from node {nodeName}, on channel {channel} message, {message}");
            }
            //var isWatchDogMsg = type == nameof(IWatchDogMessage);
            //foreach (var pair in _subscriptions)
            {
                //if (type == pair.Key)
                {
                    Robustness.Instance.SafeCall(() =>
                    {
                        OnMessage?.BeginInvoke(channel, type, packet, null, null);
                        //pair.Value.BeginInvoke(packet, null, null);
                    });
                }
            }
        }
    }

    class UdpVirtualSubscriber : IDistributedMessageSubscriber
    {
        private readonly RedisSerializer _serializer=new RedisSerializer();
        readonly ConcurrentBag<KeyValuePair<string, Action<string>>> _subscriptions = new ConcurrentBag<KeyValuePair<string, Action<string>>>();
        private readonly Action<UdpVirtualSubscriber> _action;
        private readonly string _channel;

        public UdpVirtualSubscriber(Action<UdpVirtualSubscriber> action, string channel)
        {
            this._action = action;
            _channel = channel;
        }

        internal string Channel
        {
            get { return _channel; }
        }

        public void Dispose()
        {
            
        }

        public void Subscribe(string message, Action<string> action)
        {
            _subscriptions.Add(new KeyValuePair<string, Action<string>>(message, action));
        }

        public void Subscribe<T>(Action<T> action)
        {
            if (action == null)
                return;

            void ActString(string s)
            {
                var item = _serializer.DeserializeFromString<T>(s);

                //WeakAction w=new WeakAction(action);//todo, do we need to store weak reference
                action(item);
            }

            var typeName = typeof(T).Name;

            Subscribe(typeName, ActString); 
        }

        internal void OnMessageReceived(string type, string message)
        {
            //var type = msgParts[2];
            //3 and 4 reserved
            var packet = message; // msgParts[5];

            //var isWatchDogMsg = type == nameof(IWatchDogMessage);
            foreach (var pair in _subscriptions)
            {
                if (type == pair.Key)
                {
                    Robustness.Instance.SafeCall(() =>
                    {
                        pair.Value.Invoke(packet); //BeginInvoke(packet, null, null);
                    });
                }
            }
        }
        
    }
}