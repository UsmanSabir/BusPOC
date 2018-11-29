namespace BusLib.BatchEngineCore.Groups
{
    public interface IGroupSubscriber
    {
        int GroupKey { get; }

        void OnGroupStart(IGroupStartContext context);

        void OnGroupComplete(IGroupCompleteContext context);

        void OnGroupStop(IGroupStoppedContext context);

        void OnGroupResume(IGroupResumeContext context);
    }
}