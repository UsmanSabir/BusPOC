namespace BusLib.BatchEngineCore.PubSub
{
    public interface IBroadcastMessage:IMessage
    {
        
    }

    public interface IWatchDogMessage : IBroadcastMessage
    {

    }

    public interface IProcessInputIdleWatchDogMessage : IWatchDogMessage
    {
        int GroupId { get; }
        int ProcessId { get; }

        int ProcessKey { get; }
    }
}