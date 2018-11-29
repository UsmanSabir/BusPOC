using System.Collections.Generic;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupCommandHandler:IHandler<GroupMessage>
    {
        private readonly List<IGroupSubscriber> _groupSubscribers = new List<IGroupSubscriber>(); //todo

        public void Handle(GroupMessage message)
        {
            if (GroupActions.Start.Id ==message.Action.Id)
            {
                StartGroup(message.Group);
            }
            else if(GroupActions.Stop.Id == message.Action.Id)
            {
                StopGroup(message.Group);
            }
        }

        private void StopGroup(IGroupEntity group)
        {
            //todo mark all pending tasks/processes as stopped

            //publish group stop message
        }

        private void StartGroup(IGroupEntity group)
        {
            var groupLogger = LoggerFactory.GetGroupLogger(group.Id);
            GroupStartContext context = new GroupStartContext(group, groupLogger);

            foreach (var groupSubscriber in _groupSubscribers)
            {
                if (groupSubscriber.GroupKey!=group.GroupKey)
                {
                    continue;
                }
                context.CurrentSubscriber = groupSubscriber;
                Robustness.Instance.SafeCall(() => { groupSubscriber.OnGroupStart(context); }, groupLogger);
                context.CurrentSubscriber = null;
            }
            
            if (context.StopFlag)
            {
                groupLogger.Info("Group stopped by subscriber");
                return;
            }

            //todo get group processes and add to queue

        }
    }
}