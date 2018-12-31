using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;

namespace BusLib.BatchEngineCore.Groups
{
    public interface IGroupStartContext
    {
        long Id { get; }
        int GroupKey { get; }
        bool IsResubmission { get; }
        void StopGroup(string reason);
        string SubmittedBy { get; }
        ILogger Logger { get; }
    }

    public interface IGroupCompleteContext
    {
        int Id { get; }
        int GroupKey { get; }
        bool IsResubmission { get; }

        bool IsResubmitted { get; }

        void Resubmit(string reason);

        ILogger Logger { get; }
    }

    public interface IGroupStoppedContext
    {
        int Id { get; }
        int GroupKey { get; }
        string StopReason { get; }

        ILogger Logger { get; }
    }

    public interface IGroupResumeContext
    {
        int Id { get; }
        int GroupKey { get; }
        string SubmittedBy { get; }

        ILogger Logger { get; }
    }


    public class GroupStartContext : IGroupStartContext
    {
        private readonly IReadWritableGroupEntity _groupEntity;
        private Bus _bus;
        public List<JobCriteria> MessageCriteria { get; }

        public GroupStartContext(IReadWritableGroupEntity @group, ILogger logger, List<JobCriteria> messageCriteria, Bus bus)
        {
            _groupEntity = @group;
            _bus = bus;
            MessageCriteria = messageCriteria;
            Logger = logger;

            Id = group.Id;
            GroupKey = group.GroupKey;
            IsResubmission = group.IsResubmission;
            SubmittedBy = group.SubmittedBy;
        }

        
        
        public long Id { get; }
        public int GroupKey { get; }

        public bool IsResubmission { get; }
        public string SubmittedBy { get; }
        public ILogger Logger { get; }

        internal IGroupSubscriber CurrentSubscriber { get; set; }

        internal bool StopFlag { get; private set; }
        void IGroupStartContext.StopGroup(string reason)
        {
            StopFlag = true;

            if (CurrentSubscriber != null)
            {
                Logger.Info($"{CurrentSubscriber.GetType()} requested group stop");
            }

            var stopGroupMessage = new GroupMessage(GroupActions.Stop, _groupEntity, null, reason);
            _bus.HandleWatchDogMessage(stopGroupMessage);
        }

    }

    public interface IGroupEntity: ICompletableState
    {
        long Id { get; }
        int GroupKey { get; }
        bool IsManual { get; }
        bool IsResubmission { get; }
        string SubmittedBy { get; }
        string Criteria { get; }
        string State { get; }
        bool IsGenerated { get; }
        string Payload { get; }

    }

    public interface IWritableGroupEntity : IWritableCompletableState
    {
        long Id { set; }
        int GroupKey { set; }
        bool IsManual { set; }
        bool IsResubmission { set; }
        string SubmittedBy { set; }
        string Criteria { set; }
        string State { set; }

        bool IsGenerated { set; }
        string Payload { set; }

    }

    public interface IReadWritableGroupEntity : IGroupEntity,IWritableGroupEntity
    {
        new long Id { get; set; }
        new int GroupKey { get; set; }
        new bool IsManual { get; set; }
        new bool IsResubmission { get; set; }
        new string SubmittedBy { get; set; }
        new string Criteria { get; set; }
        new string State { get; set; }
        new bool IsFinished { get; set; }
        new bool IsStopped { get; set; }

        new bool IsGenerated { get; set; }
        new string Payload { get; set; }

    }


    public interface IProcessEntity : ICompletableState
    {
        int Id { get; }
        int ProcessKey { get; }
        int GroupId { get; }


        bool IsManual { get; }
        bool IsResubmission { get; }
        string SubmittedBy { get; }


    }

    class SubmittedGroup
    {
        object _syncLock=new object();

        public SubmittedGroup(IReadWritableGroupEntity groupEntity, List<IReadWritableProcessState> processEntities)
        {
            GroupEntity = groupEntity;
            ProcessEntities = processEntities;
        }

        public IReadWritableGroupEntity GroupEntity { get; private set; }

        public List<IReadWritableProcessState> ProcessEntities { get; private set; }

        public IEnumerable<IReadWritableProcessState> GetNextProcesses(long? parentProcessId)
        {
            lock (_syncLock)
            {
                List<IReadWritableProcessState> groupProcesses = ProcessEntities;

                var nextProcess = groupProcesses.Where(p => p.ParentId == parentProcessId);
                return nextProcess;
            }
        }

        public void UpdateProcessInstance(IReadWritableProcessState processState)
        {
            lock (_syncLock)
            {
                ProcessEntities.Remove(ProcessEntities.First(p => p.Id == processState.Id));
                ProcessEntities.Add(processState);
            }
        }

        public void Refresh(IStateManager stateManager)
        {
            lock (_syncLock)
            {
                GroupEntity = stateManager.GetGroupEntity(GroupEntity.Id);
                ProcessEntities = stateManager.GetSubmittedGroupProcesses(GroupEntity.Id).ToList();
            }
        }
    }

}