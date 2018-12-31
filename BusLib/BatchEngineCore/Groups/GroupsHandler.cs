using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Serializers;

namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupsHandler
    {
        private readonly IBatchEngineSubscribers _batchEngineSubscribers;
        private readonly ILogger _logger;
        private readonly IStateManager _stateManager;
        private readonly ISerializersFactory _serializersFactory;
        private readonly IEntityFactory _entityFactory;
        private readonly IResolver _resolver;
        private readonly IBatchLoggerFactory _loggerFactory;

        public GroupsHandler(ILogger logger, IBatchEngineSubscribers batchEngineSubscribers, IStateManager stateManager, 
            ISerializersFactory serializersFactory, IEntityFactory entityFactory, IResolver resolver, IBatchLoggerFactory loggerFactory)
        {
            _logger = logger;
            _batchEngineSubscribers = batchEngineSubscribers;
            _stateManager = stateManager;
            _serializersFactory = serializersFactory;
            _entityFactory = entityFactory;
            _resolver = resolver;
            _loggerFactory = loggerFactory;
        }

        private Bus _bus;
        private Bus Bus
        {
            get { return _bus ?? (_bus = _resolver.Resolve<Bus>()); }
        }
        
        //public void Handle(GroupMessage message)
        //{
        //    if (GroupActions.Start.Id ==message.Action.Id)
        //    {
        //        if (message.IsProcessSubmission)
        //        {
        //            CreateProcesses(message.Group, message.ProcessKeys, message.Criteria);
        //        }
        //        else
        //        {
        //            CreateGroup(message.Group, message.Criteria);
        //        }

        //    }
        //    else if(GroupActions.Stop.Id == message.Action.Id)
        //    {
        //        StopGroup(message.Group, message.Message);
        //    }
        //}

        public SubmittedGroup HandleStartMessage(GroupMessage message)
        {
            if (GroupActions.Start == message.Action)
            {
                List<int> groupProcesses;

                if (!message.IsProcessSubmission)
                {
                    if (message.Group.GroupKey == 0)
                    {
                        _logger.Error("Group {groupId} submitted with no group key", message.GroupId);
                        return null;
                    }
                    groupProcesses = GetGroupProcesses(message.Group.GroupKey);


                    //}
                    //else
                    //{
                    //    return CreateGroup(message.Group, message.Criteria);
                    //}
                }
                else
                {
                    groupProcesses = message.ProcessKeys;
                }

                return CreateProcesses(message.Group, groupProcesses, message.Criteria);
            }
            return null; //todo not a group start message
        }

        public void StopGroup(IReadWritableGroupEntity group, string message)
        {
            _logger.Info($"Stopping group with message {message}");

            _stateManager.MarkGroupStopped(group);
            ////todo mark all pending tasks/processes as stopped
            //group.MarkGroupStatus(CompletionStatus.Stopped, ResultStatus.Empty, message);
            ////publish group stop message
        }

        internal SubmittedGroup CreateProcesses(IReadWritableGroupEntity @group, List<int> processKeys,
            List<JobCriteria> messageCriteria)
        {
            var groupLogger = _loggerFactory.GetGroupLogger(group.Id, group.GroupKey);
            groupLogger.Trace("Starting group");


            if (processKeys ==null || processKeys.Count == 0)
            {
                _logger.Error("No process found for group");
                StopGroup(@group, "No process found for group");
                return null;
            }

            var serializer = _serializersFactory.GetSerializer<JobCriteria>();

            GroupStartContext context = new GroupStartContext(group, groupLogger, messageCriteria, Bus);

            var groupSubscribers = _batchEngineSubscribers.GetGroupSubscribers().ToList();

            foreach (var groupSubscriber in groupSubscribers)
            {
                if (groupSubscriber.GroupKey != group.GroupKey)
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
            List<(JobCriteria Criteria, IReadWritableProcessState ProcessState)> process2Submit = new List<(JobCriteria Criteria, IReadWritableProcessState ProcessState)>(); // List<IReadWritableProcessState>();

            List<IReadWritableProcessState> processList = new List<IReadWritableProcessState>();

            foreach (var processKey in processKeys)
            {
                var process = _entityFactory.CreateProcessEntity();
                process.ProcessKey = processKey;
                //IReadWritableProcessState process = erf wer GetKeyProcesses(processKey);
                //IWritableProcessState writableProcess = process;
                process.GroupId = group.Id;
                
                //process.CorrelationId=Guid.NewGuid();
                processList.Add(process);
            }

            
            if (processList.Count == 0)
            {
                _logger.Error("No process found for group");
                StopGroup(group, "No process found for group");
                return null;
            }

            //if (messageCriteria.Count == 1)
            //{
            //    var cta = messageCriteria[0];
            //    process2submit.AddRange(processList.Select(s =>
            //    {
            //        ((IWritableProcessState)s).Criteria = serializer.SerializeToString(cta);
            //        return s;
            //    }));
            //}
            //else
            {
                foreach (var criteria in messageCriteria)
                {
                    foreach (var process in processList)
                    {
                        var p = process.Clone(_entityFactory);
                        p.Criteria = serializer.SerializeToString(criteria);
                        p.CorrelationId=Guid.NewGuid();
                        process.CompanyId = criteria.CompanyId;
                        process.BranchId = criteria.BranchId;
                        process.SubTenantId = criteria.SubTenantId;
                        p.ProcessingDate = criteria.ProcessingDate;
                        p.Status= CompletionStatus.Pending;
                        p.Result = ResultStatus.Empty;
                        //p.Tag = criteria.Tag; //todo
                        process2Submit.Add((criteria, p));
                    }
                }
            }


            _logger.Trace($"Submitting processes {process2Submit.Count}");
            SubmitProcesses(process2Submit, group);
            _logger.Trace($"Submission complete of {process2Submit.Count} processes");

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

            //var nextProcesses = GetNextProcesses(null);

            //nextProcesses.ForEach(p =>
            //{
            //    var volumeMessage = new ProcessExecutionContext(BatchLoggerFactory.GetProcessLogger(p.Id, p.ProcessKey), p);
            //    Bus.Instance.HandleVolumeRequest(volumeMessage);
            //});

            SubmittedGroup gGroup = new SubmittedGroup(group, process2Submit.Select(s=>s.ProcessState).ToList());
            return gGroup;
        }

        //private IReadWritableProcessState GetKeyProcesses(int processKey)
        //{
        //    var processState = _stateManager.GetProcessByKey(processKey);
        //    return processState;
        //}

        #region GroupSubmit(Commented)

        //internal SubmittedGroup CreateGroup(IGroupEntity @group, List<JobCriteria> messageCriteria)
        //{
        //    var groupLogger = BatchLoggerFactory.GetGroupLogger(group.Id, group.GroupKey);
        //    groupLogger.Trace("Starting group");
        //    var serializer = _serializersFactory.GetSerializer<JobCriteria>();

        //    GroupStartContext context = new GroupStartContext(group, groupLogger, messageCriteria);

        //    var groupSubscribers = _batchEngineSubscribers.GetGroupSubscribers().ToList();

        //    foreach (var groupSubscriber in groupSubscribers)
        //    {
        //        if (groupSubscriber.GroupKey!=group.GroupKey)
        //        {
        //            continue;
        //        }
        //        context.CurrentSubscriber = groupSubscriber;
        //        Robustness.Instance.SafeCall(() => { groupSubscriber.OnGroupStarting(context); }, groupLogger);
        //        context.CurrentSubscriber = null;
        //    }

        //    if (context.StopFlag)
        //    {
        //        groupLogger.Info("Group stopped by subscriber");
        //        StopGroup(group, "Group stopped by subscriber");
        //        return null;
        //    }

        //    //todo get group processes and add to queue
        //    List<(JobCriteria Criteria, IReadWritableProcessState ProcessState)> process2submit = new List<(JobCriteria Criteria, IReadWritableProcessState ProcessState)>(); // List<IReadWritableProcessState>();
        //    //List<IProcessState> process2submit=new List<IProcessState>();
        //    var groupProcesses = GetGroupProcesses(@group.GroupKey);

        //    if (groupProcesses.Count == 0)
        //    { 
        //        _logger.Error("No process found for group");
        //        StopGroup(group, "No process found for group");
        //        return null;
        //    }

        //    //if (messageCriteria.Count == 1)
        //    //{
        //    //    var cta = messageCriteria[0];
        //    //    process2submit.AddRange(groupProcesses.Select(s =>
        //    //    {
        //    //        s.Criteria = cta;
        //    //        return s;
        //    //    }));
        //    //}
        //    //else
        //    {
        //        foreach (var criteria in messageCriteria)
        //        {
        //            foreach (var process in groupProcesses)
        //            {
        //                var p = process.Clone(_entityFactory);
        //                p.Criteria = serializer.SerializeToString(criteria);
        //                p.CorrelationId=Guid.NewGuid();
        //                process2submit.Add((criteria, p));
        //            }
        //        }
        //    }


        //    _logger.Trace($"Submitting processes {process2submit.Count}");
        //    SubmitProcesses(process2submit, group);
        //    _logger.Trace($"Submission complete of {process2submit.Count} processes");

        //    foreach (var groupSubscriber in groupSubscribers)
        //    {
        //        if (groupSubscriber.GroupKey != group.GroupKey)
        //        {
        //            continue;
        //        }
        //        context.CurrentSubscriber = groupSubscriber;
        //        Robustness.Instance.SafeCall(() => { groupSubscriber.OnGroupSubmitted(context); }, groupLogger);
        //        context.CurrentSubscriber = null;
        //    }

        //    if (context.StopFlag)
        //    {
        //        groupLogger.Info("Group stopped by subscriber");
        //        StopGroup(group, "Group stopped by subscriber");
        //        return null;
        //    }

        //    //var nextProcesses = GetNextProcesses(null);

        //    //nextProcesses.ForEach(p =>
        //    //{
        //    //    var volumeMessage = new ProcessExecutionContext(BatchLoggerFactory.GetProcessLogger(p.Id, p.ProcessKey), p);
        //    //    Bus.Instance.HandleVolumeRequest(volumeMessage);
        //    //});

        //    SubmittedGroup gGroup=new SubmittedGroup(group, process2submit.Select(s=>s.ProcessState).ToList());
        //    return gGroup;
        //}

        #endregion

        private void SubmitProcesses(List<(JobCriteria Criteria, IReadWritableProcessState ProcessState)> groupProcesses, IGroupEntity groupEntity)
        {
            //groupProcesses.ForEach(p =>
            //{
            //    p.GroupId = groupEntity.Id; // todo
            //    p.Criteria = groupEntity.Criteria;
            //});


            var processStates = groupProcesses.Select(s=>s.ProcessState).ToList();
            _stateManager.AddGroupProcess(processStates, groupEntity);

            //todo
            //using (var trans = _stateManager.BeginTransaction())
            //{
            //    foreach (var process in groupProcesses)
            //    {
            //        _stateManager.Insert(process);
            //    }

            //    trans.Commit();
            //}


            var subscribers = _batchEngineSubscribers.GetProcessSubscribers().ToList();
            if (subscribers.Count > 0)
            {
                foreach (var p in groupProcesses)
                {
                    var processState = p.ProcessState;
                    var processSubscribers = subscribers.Where(s=>s.ProcessKey== processState.ProcessKey).ToList();
                    if (processSubscribers.Count > 0)
                    {
                        ProcessSubmittedContext pContext = new ProcessSubmittedContext(processState.Id, processState.ProcessKey, groupEntity.IsResubmission, groupEntity.SubmittedBy, p.Criteria, _loggerFactory.GetProcessLogger(processState.Id, processState.ProcessKey, processState.CorrelationId));
                        foreach (var subscriber in subscribers)
                        {
                            Robustness.Instance.SafeCall(() => subscriber.OnProcessSubmitted(pContext));
                        }
                    }
                }
            }

            
        }


        List<int> GetGroupProcesses(int groupId)
        {
            var processes=_stateManager.GetConfiguredGroupProcessKeys(groupId).ToList(); //todo
            return processes;
        }

        //List<IProcessState> GetNextProcesses(int? parentProcessId)
        //{
        //    _logger.Trace($"");
        //    List<IProcessState> groupProcesses=null;

        //    var nextProcess = groupProcesses.Where(p=>p.ParentId==parentProcessId).ToList();
        //    return nextProcess;
        //}

        public void Dispose()
        {
        }
    }
}