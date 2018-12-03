namespace BusLib.Core.Events
{
    public interface IEventAggregator
    {
        void Publish(string @event, string parameter = null);
    }
}