using System.Xml.Serialization;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core.Events;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IBroadcastMessage:IMessage, ITinyMessage
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
        long GroupId { get; }
        long ProcessId { get; }

        int ProcessKey { get; }
    }

    public interface IProcessVolumeGeneratedWatchDogMessage : IWatchDogMessage
    {
        string ProcessId { get; }
    }

    public interface IProcessRemovedWatchDogMessage : IWatchDogMessage
    {
        long GroupId { get; }
        long ProcessId { get; }
    }

    class ProcessGroupRemovedMessage: IProcessGroupRemovedMessage
    {
        public ProcessGroupRemovedMessage(SubmittedGroup @group, object sender)
        {
            Group = @group;
            Sender = sender;
        }

        public SubmittedGroup Group { get; }

        [XmlIgnore]
        public object Sender { get; }
    }

    internal class ProcessInputIdleWatchDogMessage : IProcessInputIdleWatchDogMessage
    {
        public ProcessInputIdleWatchDogMessage()
        {
            
        }

        public ProcessInputIdleWatchDogMessage(long groupId, long processId, int processKey, object sender)
        {
            GroupId = groupId;
            ProcessId = processId;
            ProcessKey = processKey;
            Sender = sender;
        }

        public long GroupId { get; set; }
        public long ProcessId { get; set; }
        public int ProcessKey { get; set; }

        [XmlIgnore]
        public object Sender { get; set; }
    }

    class ProcessRemovedWatchDogMessage: IProcessRemovedWatchDogMessage
    {
        //public object Sender { get; }
        public long GroupId { get; set; }
        public long ProcessId { get; set; }

        [XmlIgnore]
        public object Sender { get; }
    }

    internal class TriggerProducerWatchDogMessage : IProcessVolumeGeneratedWatchDogMessage
    {
        [XmlIgnore]
        public object Sender { get; }

        public string ProcessId { get; set; }
    }

    internal class ProcessGroupAddedMessage : IBroadcastMessage
    {
        public long GroupId { get; set; }
        [XmlIgnore]
        public object Sender { get; }
    }


}