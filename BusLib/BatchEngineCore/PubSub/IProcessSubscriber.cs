using BusLib.BatchEngineCore.Groups;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IProcessSubscriber
    {
        int ProcessKey { get; }

        //void OnProcessStarting(IProcessStartContext context);

        void OnProcessSubmitted(IProcessSubmittedContext context);

        void OnProcessComplete(IProcessCompleteContext context);

        void OnProcessStop(IProcessStoppedContext context);

        void OnProcessResume(IProcessResumeContext context);

        void OnProcessRetry(IProcessRetryContext context);
    }
}