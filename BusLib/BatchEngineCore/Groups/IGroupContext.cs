using BusLib.Core;

namespace BusLib.BatchEngineCore.Groups
{
    public interface IGroupStartContext
    {
        int Id { get; }
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
        private readonly IGroupEntity _groupEntity;

        public GroupStartContext(IGroupEntity @group, ILogger logger)
        {
            _groupEntity = @group;
            Logger = logger;

            Id = group.Id;
            GroupKey = group.GroupKey;
            IsResubmission = group.IsResubmission;
            SubmittedBy = group.SubmittedBy;
        }

        public int Id { get; }
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

            var stopGroupMessage = new GroupMessage(GroupActions.Stop, _groupEntity, reason);
            Bus.Instance.HandleGroupMessage(stopGroupMessage);
        }

    }

    public interface IGroupEntity: ICompletableState
    {
        int Id { get; }
        int GroupKey { get; }
        bool IsManual { get; }
        bool IsResubmission { get; }
        string SubmittedBy { get; }


    }
}