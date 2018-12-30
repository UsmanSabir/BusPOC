using System.Text;
using System.Threading;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Helper;
using BusLib.Serializers;

namespace BusLib.BatchEngineCore.WatchDog
{
    internal class PubSubWatchDog :SafeDisposable,  IHandler<IWatchDogMessage> //IHandler<GroupMessage>, 
    {
        private readonly ILogger _logger;
        private readonly IStateManager _stateManager;
        private readonly IDistributedMessagePublisher _publisher;
        private readonly ISerializer _serializer;
        private readonly IDistributedMessageSubscriber _subscriber;
        private readonly TinyMessageSubscriptionToken _healthSub;

        public PubSubWatchDog(ILogger logger, IStateManager stateManager, IPubSubFactory publisherFactory, CancellationToken cancellationToken)
        {
            _logger = logger;
            _stateManager = stateManager;
            _publisher = publisherFactory.GetPublisher(cancellationToken, logger, nameof(IWatchDogMessage));
            _subscriber = publisherFactory.GetSubscriber(cancellationToken, logger, nameof(IWatchDogMessage));

            _serializer = SerializersFactory.Instance.GetSerializer(typeof(GroupMessage));

            _subscriber.Subscribe(nameof(ProcessRemovedWatchDogMessage), ProcessRemovedFromMaster);
            _subscriber.Subscribe(nameof(TriggerProducerWatchDogMessage), ProcessVolumeChangedFromMaster);
            _healthSub = Bus.Instance.EventAggregator.Subscribe4Broadcast<HealthMessage>(PublishHealth);
        }

        private void PublishHealth(HealthMessage health)
        {
            _logger.Trace($"HealthCheck '{nameof(PubSubWatchDog)}' Start");
            HealthBlock block = new HealthBlock()
            {
                Name = "PubSubWatchDog"
            };

            block.AddMatrix("IsActive", !IsDisposed);

            health.Add(block);
            _logger.Trace($"HealthCheck '{nameof(PubSubWatchDog)}' End");
        }

        private void ProcessRemovedFromMaster(string msg)
        {
            var message = _serializer.DeserializeFromString<ProcessRemovedWatchDogMessage>(msg);
            Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, message.ProcessId.ToString());
        }

        private void ProcessVolumeChangedFromMaster(string msg)
        {
            var message = _serializer.DeserializeFromString<TriggerProducerWatchDogMessage>(msg);
            Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessVolumeGenerated, message.ProcessId);
        }

        string SerializeMessage<T>(T message)
        {
            StringBuilder sb=new StringBuilder();
            //sb.AppendFormat("{0}::{1}::", NodeSettings.Instance.Name, type);
            var msg =_serializer.SerializeToString(message);
            sb.Append(msg);
            return sb.ToString();
        }

        public void Handle(IWatchDogMessage message)
        {
            var msg = SerializeMessage(message);
            if (message is ProcessInputIdleWatchDogMessage processIdleMessage)
            {
                _publisher.PublishMessage(msg, nameof(ProcessInputIdleWatchDogMessage));
            }
            else if (message is GroupMessage groupMessage)
            {
                if (groupMessage.Action == GroupActions.Stop)
                {
                    _stateManager.MarkGroupStopped(groupMessage.Group);
                }

                _publisher.PublishMessage(msg, nameof(GroupMessage));
            }

        }

        protected override void Dispose(bool disposing)
        {
            Bus.Instance.EventAggregator.Unsubscribe(_healthSub);
            Robustness.Instance.SafeCall(()=> _subscriber?.Dispose());
            Robustness.Instance.SafeCall(()=>_publisher?.Dispose());

            base.Dispose(disposing);
        }
    }
}