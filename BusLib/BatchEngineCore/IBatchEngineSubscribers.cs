using System.Collections.Generic;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.PubSub;

namespace BusLib.BatchEngineCore
{
    public interface IBatchEngineSubscribers
    {
        IEnumerable<IGroupSubscriber> GetGroupSubscribers();
        IEnumerable<IProcessSubscriber> GetProcessSubscribers();
    }

    public class BatchEngineSubscribers: IBatchEngineSubscribers
    {
        private readonly IReadOnlyList<IGroupSubscriber> _groupSubscribers = new List<IGroupSubscriber>(); //todo
        private readonly IReadOnlyList<IProcessSubscriber> _processSubscribers = new List<IProcessSubscriber>(); //todo
        

        public IEnumerable<IGroupSubscriber> GetGroupSubscribers()
        {
            return _groupSubscribers;//.AsReadOnly();
        }

        public IEnumerable<IProcessSubscriber> GetProcessSubscribers()
        {
            return _processSubscribers;
        }
    }



}