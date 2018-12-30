using System;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IDistributedMessagePublisher:IDisposable
    {
        void PublishMessage(string message, string type);
        void PublishMessage<T>(T message);
    }

    public interface IDistributedMessageSubscriber:IDisposable
    {
        void Subscribe(string message, Action<string> action);

        void Subscribe<T>(Action<T> action);
        //void Shutdown();
    }

}