using BusLib.BatchEngineCore.Groups;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IBroadcastMessage:IMessage
    {
        
    }

    internal interface IProcessGroupRemovedMessage : IBroadcastMessage
    {
        SubmittedGroup Group { get; }
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

    class ProcessGroupRemovedMessage: IProcessGroupRemovedMessage
    {
        public ProcessGroupRemovedMessage(SubmittedGroup @group)
        {
            Group = @group;
        }

        public SubmittedGroup Group { get; }
    }
}