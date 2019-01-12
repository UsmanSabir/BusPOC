using System;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;

namespace BusLib.Core.Events
{
    //https://csharpvault.com/weak-event-pattern/
    public interface IEventAggregator
    {
        void Publish<T>(T parameter) where T : class, ITinyMessage;
        TinyMessageSubscriptionToken Subscribe<T>(Action<T> action) where T : class, ITinyMessage;
        
        void Publish(object sender, string @event, string parameter = null);
        void PublishAsync(object sender, string @event, string parameter = null);

        void Broadcast<T>(T message) where T : class, IBroadcastMessage;
        void BroadcastAsync<T>(T message) where T : class, IBroadcastMessage;

        TinyMessageSubscriptionToken Subscribe<T>(Action<T> action, string message) where T : TextMessage;

        TinyMessageSubscriptionToken Subscribe4Broadcast<T>(Action<T> action) where T : class, IBroadcastMessage;
        void Unsubscribe(TinyMessageSubscriptionToken subscription);
    }

    class TinyEventAggregator:IEventAggregator
    {
        public void Publish<T>(T message) where T : class, ITinyMessage
        {
            TinyMessengerHub.Instance.Publish(message);
        }

        public TinyMessageSubscriptionToken Subscribe<T>(Action<T> action) where T : class, ITinyMessage
        {
            return TinyMessengerHub.Instance.Subscribe<T>(action);
        }

        public void Publish(object sender, string @event, string parameter = null)
        {
            TinyMessengerHub.Instance.Publish(new TextMessage(sender, @event, parameter));
        }

        public void PublishAsync(object sender, string @event, string parameter = null)
        {
            TinyMessengerHub.Instance.PublishAsync(new TextMessage(sender, @event, parameter));
        }

        public TinyMessageSubscriptionToken Subscribe<T>(Action<T> action, string message) where T : TextMessage
        {
            return TinyMessengerHub.Instance.Subscribe<T>(action, m => m.Content == message);
        }

        public void Broadcast<T>(T message) where T : class, IBroadcastMessage
        {
            TinyMessengerHub.Instance.Publish(message);//new GenericTinyMessage<T>(this, message));
        }
        
        public void BroadcastAsync<T>(T message) where T : class, IBroadcastMessage
        {
            TinyMessengerHub.Instance.PublishAsync(message);//new GenericTinyMessage<T>(this, message));
        }


        public TinyMessageSubscriptionToken Subscribe4Broadcast<T>(Action<T> action) where T : class, IBroadcastMessage
        {
            return TinyMessengerHub.Instance.Subscribe<T>(action);
        }

        public void Unsubscribe(TinyMessageSubscriptionToken subscription)
        {
            if (subscription != null) TinyMessengerHub.Instance.Unsubscribe(subscription);
        }

        //public void Unsubscribe<T>(Action<T> action, string message)
        //{
        //    TinyMessengerHub.Instance.Unsubscribe<T>();
        //}
    }

    public class TextMessage:GenericTinyMessage<string>
    {
        public TextMessage(object sender, string content, string parameter) : base(sender, content)
        {
            Parameter = parameter;
        }

        public string Parameter { get; }
    }

    internal class VolumeErrorMessage : GenericTinyMessage<ProcessExecutionContext>
    {
        public VolumeErrorMessage(object sender, ProcessExecutionContext content) : base(sender, content)
        {
            
        }
    }


}