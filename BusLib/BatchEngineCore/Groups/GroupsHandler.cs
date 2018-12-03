using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupsHandler
    {
        private readonly IBatchEngineSubscribers _batchEngineSubscribers;
        private readonly ILogger _logger;
        private readonly IStateManager _stateManager;
        public GroupsHandler(ILogger logger, IBatchEngineSubscribers batchEngineSubscribers, IStateManager stateManager)
        {
            _logger = logger;
            this._batchEngineSubscribers = batchEngineSubscribers;
            _stateManager = stateManager;
        }

        public void Handle(GroupMessage message)
        {
            if (GroupActions.Start.Id ==message.Action.Id)
            {
                CreateGroup(message.Group);
            }
            else if(GroupActions.Stop.Id == message.Action.Id)
            {
                StopGroup(message.Group, message.Message);
            }
        }

        public void StopGroup(IGroupEntity group, string message)
        {
            //todo mark all pending tasks/processes as stopped
            group.MarkGroupStatus(TaskCompletionStatus.Stopped, ResultStatus.Empty, message);
            //publish group stop message
        }

        internal SubmittedGroup CreateGroup(IGroupEntity group)
        {
            var groupLogger = LoggerFactory.GetGroupLogger(group.Id, group.GroupKey);
            groupLogger.Trace("Starting group");

            GroupStartContext context = new GroupStartContext(group, groupLogger);

            var groupSubscribers = _batchEngineSubscribers.GetGroupSubscribers().ToList();

            foreach (var groupSubscriber in groupSubscribers)
            {
                if (groupSubscriber.GroupKey!=group.GroupKey)
                {
                    continue;
                }
                context.CurrentSubscriber = groupSubscriber;
                Robustness.Instance.SafeCall(() => { groupSubscriber.OnGroupStarting(context); }, groupLogger);
                context.CurrentSubscriber = null;
            }
            
            if (context.StopFlag)
            {
                groupLogger.Info("Group stopped by subscriber");
                StopGroup(group, "Group stopped by subscriber");
                return null;
            }

            //todo get group processes and add to queue
            var groupProcesses = GetGroupProcesses(group.GroupKey);

            if (groupProcesses.Count == 0)
            { 
                _logger.Error("No process found for group");
                StopGroup(group, "No process found for group");
                return null;
            }

            _logger.Trace($"Submitting processes {groupProcesses.Count}");
            SubmitProcesses(groupProcesses, group);
            _logger.Trace($"Submission complete of {groupProcesses.Count} processes");

            foreach (var groupSubscriber in groupSubscribers)
            {
                if (groupSubscriber.GroupKey != group.GroupKey)
                {
                    continue;
                }
                context.CurrentSubscriber = groupSubscriber;
                Robustness.Instance.SafeCall(() => { groupSubscriber.OnGroupSubmitted(context); }, groupLogger);
                context.CurrentSubscriber = null;
            }

            if (context.StopFlag)
            {
                groupLogger.Info("Group stopped by subscriber");
                StopGroup(group, "Group stopped by subscriber");
                return null;
            }

            var nextProcesses = GetNextProcesses(null);

            nextProcesses.ForEach(p =>
            {
                var volumeMessage = new ProcessExecutionContext(LoggerFactory.GetProcessLogger(p.Id, p.ProcessKey), p);
                Bus.Instance.HandleVolumeRequest(volumeMessage);
            });

            SubmittedGroup gGroup=new SubmittedGroup(group, groupProcesses);
            return gGroup;
        }

        private void SubmitProcesses(List<IProcessState> groupProcesses, IGroupEntity groupEntity)
        {
            //groupProcesses.ForEach(p =>
            //{
            //    p.GroupId = groupEntity.Id; // todo
            //    p.Criteria = groupEntity.Criteria;
            //});


            //todo
            using (var trans = _stateManager.BeginTransaction())
            {
                foreach (var process in groupProcesses)
                {
                    _stateManager.Insert(process);
                }

                trans.Commit();
            }


            var subscribers = _batchEngineSubscribers.GetProcessSubscribers().ToList();
            if (subscribers.Count > 0)
            {
                groupProcesses.ForEach(p =>
                {
                    var processSubscribers = subscribers.Where(s=>s.ProcessKey== p.ProcessKey).ToList();
                    if (processSubscribers.Count > 0)
                    {
                        ProcessSubmittedContext pContext = new ProcessSubmittedContext(p.Id, p.ProcessKey, groupEntity.IsResubmission, groupEntity.SubmittedBy, LoggerFactory.GetProcessLogger(p.Id, p.ProcessKey));
                        foreach (var subscriber in subscribers)
                        {
                            Robustness.Instance.SafeCall(() => subscriber.OnProcessSubmitted(pContext));
                        }
                    }
                });
            }

            
        }


        List<IProcessState> GetGroupProcesses(int groupId)
        {
            List<IProcessState> processes=null; //todo
            return processes;
        }

        List<IProcessState> GetNextProcesses(int? parentProcessId)
        {
            _logger.Trace($"");
            List<IProcessState> groupProcesses=null;

            var nextProcess = groupProcesses.Where(p=>p.ParentId==parentProcessId).ToList();
            return nextProcess;
        }

        public void Dispose()
        {
        }
    }
}