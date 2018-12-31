using BusLib.Core;

namespace BusLib.BatchEngineCore.PubSub
{
    public interface IProcessSubmittedContext
    {
        long Id { get; }
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

    public interface IProcessRetryContext
    {
        long Id { get; }
        int ProcessKey { get; }
        ILogger Logger { get; }

        void Stop();

    }

    class ProcessRetryContext: IProcessRetryContext
    {
        public ProcessRetryContext(long id, int processKey, ILogger logger)
        {
            Id = id;
            ProcessKey = processKey;
            Logger = logger;
        }

        public long Id { get; }
        public int ProcessKey { get; }
        public ILogger Logger { get; }

        internal bool StopFlag { get; set; } = false;
        public void Stop()
        {
            StopFlag = true;
        }
    }

    internal class ProcessSubmittedContext: IProcessSubmittedContext
    {
        public JobCriteria Criteria { get; }

        public ProcessSubmittedContext(long id, int processKey, bool isResubmission, string submittedBy,
            JobCriteria criteria, ILogger logger)
        {
            Criteria = criteria;
            Id = id;
            ProcessKey = processKey;
            IsResubmission = isResubmission;
            SubmittedBy = submittedBy;
            Logger = logger;
        }



        public long Id { get; }
        public int ProcessKey { get; }
        public bool IsResubmission { get; }
        public string SubmittedBy { get; }
        public ILogger Logger { get; }
    }


}