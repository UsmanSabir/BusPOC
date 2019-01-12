using System;
using System.Net;
using System.Threading;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Helper.SystemHealth;
using BusLib.Infrastructure;
using BusLib.Serializers;

namespace BusLib.BatchEngineCore.PubSub
{
    internal class DashboardService:RepeatingProcess
    {
        private readonly IPubSubFactory _pubSubFactory;
        //private readonly CancellationToken _cancellationToken;
        private IDistributedMessageSubscriber _subscriber;
        private IDistributedMessagePublisher _publisher;
        //private ISerializer _serializer;

        private const string dashboardChannel = "Dashboard";
        private const string statusRequest = "status";
        private readonly IProcessDataStorage _cacheStorage;
        private readonly IStateManager _stateManager;
        private readonly Bus _bus;

        public DashboardService(IPubSubFactory pubSubFactory, ILogger logger, IProcessDataStorage cacheStorage, IStateManager stateManager, Bus bus):base(nameof(DashboardService), logger)
        {
            _pubSubFactory = pubSubFactory;
            _cacheStorage = cacheStorage;
            _stateManager = stateManager;
            _bus = bus;
        }

        internal override void OnStart()
        {
            base.OnStart();

            _publisher = _pubSubFactory.GetPublisher(Interrupter.Token, Logger, dashboardChannel);

            _subscriber = _pubSubFactory.GetSubscriber(Interrupter.Token, Logger, dashboardChannel);
            

            _subscriber.Subscribe(statusRequest, OnStatusRequest);
        }

        private void OnStatusRequest(string req)
        {
            //todo compile status
            //todo memory/ram/db/disk heal check

            HealthMessage healthMessage=new HealthMessage()
            {
                Id = Guid.NewGuid(),
                Time = DateTime.UtcNow,
                NodeKey = NodeSettings.Instance.Name,
                Sender = Dns.GetHostName(),
                NodeThrottling = NodeSettings.Instance.Throttling
            };

            healthMessage.Add(GetMemoryHealth());
            healthMessage.Add(ProcessorHealthProvider.GetHealth());
            healthMessage.Add(DiskInfoHealthProvider.GetHealth());
            healthMessage.Add(GetCacheHealth());
            healthMessage.Add(GetDatabaseHealth());

            _bus.EventAggregator.Broadcast(healthMessage);

            PublishMessage(healthMessage);
        }

        HealthBlock GetMemoryHealth()
        {
            var health = MemoryHealthProvider.GetHealth();
            return health;
        }

        HealthBlock GetCacheHealth()
        {
            var isHealthy = _cacheStorage.IsHealthy();
            HealthBlock block = new HealthBlock()
            {
                Name = "Cache"
            };
            block.AddMatrix("IsHealthy", isHealthy);
            return block;
        }

        HealthBlock GetDatabaseHealth()
        {
            HealthBlock block = new HealthBlock()
            {
                Name = "Database"
            };
            var isHealthy = _stateManager.IsHealthy();
            block.AddMatrix("IsHealthy", isHealthy);
            return block;
        }

        void PublishMessage<T>(T message) where T: IBroadcastMessage
        {
            Robustness.Instance.SafeCall(() => { _publisher.PublishMessage(message); }, Logger, "Failed to publish dashboard message with error {0}");
        }

        internal override void PerformIteration()
        {
            PublishMessage(new PingHealthMessage
            {
                IsWorking = true,
                NodeKey = NodeSettings.Instance.Name,
                Sender = Dns.GetHostName()
            });
        }
    }
}