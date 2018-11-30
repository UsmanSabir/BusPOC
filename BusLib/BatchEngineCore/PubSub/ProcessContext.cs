using BusLib.Core;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IProcessSubmittedContext
    {
        int Id { get; }
        int ProcessKey { get; }
        bool IsResubmission { get; }
        string SubmittedBy { get; }
        ILogger Logger { get; }
    }

    public interface IProcessCompleteContext
    {
        int Id { get; }
        int ProcessKey { get; }
        bool IsResubmission { get; }

        bool IsResubmitted { get; }

        void Resubmit(string reason);

        ILogger Logger { get; }
    }

    public interface IProcessStoppedContext
    {
        int Id { get; }
        int ProcessKey { get; }
        string StopReason { get; }

        ILogger Logger { get; }
    }

    public interface IProcessResumeContext
    {
        int Id { get; }
        int ProcessKey { get; }
        string SubmittedBy { get; }

        ILogger Logger { get; }
    }

    internal class ProcessSubmittedContext: IProcessSubmittedContext
    {
        public ProcessSubmittedContext(int id, int processKey, bool isResubmission, string submittedBy, ILogger logger)
        {
            Id = id;
            ProcessKey = processKey;
            IsResubmission = isResubmission;
            SubmittedBy = submittedBy;
            Logger = logger;
        }

        public int Id { get; }
        public int ProcessKey { get; }
        public bool IsResubmission { get; }
        public string SubmittedBy { get; }
        public ILogger Logger { get; }
    }


}