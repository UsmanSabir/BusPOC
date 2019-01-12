using System.Net;
using System.Net.Sockets;
using System.Text;
using BusImpl.Redis;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Helper;

namespace BusImpl.PubSubImpl.Udp
{
    public class UdpSingletonPublisher: IDistributedMessagePublisher
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly RedisSerializer _serializer;
        private readonly ILogger _logger;
        protected const string PubChannel = "BPEMChannel";
        private readonly string _customChannel;
        private readonly IPEndPoint _ipEndPoint;
        private readonly UdpClient _udpClient;

        public UdpSingletonPublisher(string ip, int port, string channel, ILogger logger) //
        {
            _ip = ip;
            _port = port;
            _logger = logger;

            _serializer = new RedisSerializer();
            _customChannel = channel ?? PubChannel;

            _udpClient = new UdpClient(AddressFamily.InterNetwork);
            _udpClient.AllowNatTraversal(true);
            var address = IPAddress.Parse(_ip);
            _ipEndPoint = new IPEndPoint(address, _port);
            _udpClient.JoinMulticastGroup(address);

        }

        string PackPubMessage(string message, string type)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}::{1}::{2}::0::0::", _customChannel, NodeSettings.Instance.Name,
                    type); //4 and 5 reserved
                sb.Append(message);
                return sb.ToString();
            }

            public void PublishMessage(string message, string type)
            {
                Robustness.Instance.SafeCall(() =>
                {
                    var data = Encoding.UTF8.GetBytes(PackPubMessage(message, type));

                    //using (var udpClient = new UdpClient(AddressFamily.InterNetwork))
                    {
                        //var address = IPAddress.Parse(_ip);
                        //var ipEndPoint = new IPEndPoint(address, _port);
                        //udpClient.JoinMulticastGroup(address);
                        _udpClient.Send(data, data.Length, _ipEndPoint);
                        //udpClient.Close();
                    }

                    _logger.Trace("Udp message published for type {type}", type);
                }, _logger, "UDP Publish failed. {0}");
            }

            public void PublishMessage<T>(T message)
            {
                var json = _serializer.SerializeToString(message);
                var type = message.GetType().Name;
                PublishMessage(json, type);
            }

        public void Dispose()
        {
            _udpClient?.Dispose();
        }
    }
    }