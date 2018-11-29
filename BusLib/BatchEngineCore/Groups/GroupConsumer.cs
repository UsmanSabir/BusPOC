using System.Collections.Generic;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupConsumer : WorkerProcess
    {
        //private readonly List<IGroupSubscriber> _groupSubscribers = new List<IGroupSubscriber>(); //todo

        public GroupConsumer(ILogger logger) : base("GroupConsumer", logger)
        {
        }


        internal override void OnStart()
        {
            //subscripe to groups


        }

        void OnGroupSubscriptionReceived(IGroupEntity group)
        {
            //var groupLogger = LoggerFactory.GetGroupLogger(group.Id);
            //IGroupStartContext context=new GroupStartContext(group, groupLogger);

            //foreach (var groupSubscriber in _groupSubscribers)
            //{
            //    Robustness.Instance.SafeCall(() => { groupSubscriber.OnGroupStart(context); });
            //}
        }

        internal override void OnStop()
        {
            //unsbscribe from groups
        }
    }
}