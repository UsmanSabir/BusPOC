using BusLib.BatchEngineCore.Groups;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IGroupSubscriber
    {
        int GroupKey { get; }

        void OnGroupStarting(IGroupStartContext context);

        void OnGroupSubmitted(IGroupStartContext context);

        void OnGroupComplete(IGroupCompleteContext context);

        void OnGroupStop(IGroupStoppedContext context);

        void OnGroupResume(IGroupResumeContext context);
    }
}