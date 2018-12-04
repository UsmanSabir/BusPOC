using System;

namespace BusLib.Core.Events
{
    public interface IEventAggregator
    {
        void Publish(string @event, string parameter = null);

        void Broadcast<T>(T message);

        void Subscribe<T>(Action<T> action); //todo
    }
}