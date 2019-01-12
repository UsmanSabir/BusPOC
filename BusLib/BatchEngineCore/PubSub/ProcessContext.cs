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
        long Id { get; }
        int ProcessKey { get; }
        bool IsResubmission { get; }

        bool IsResubmitted { get; }

        void Resubmit(string reason);

        ILogger Logger { get; }
    }

    public interface IProcessStoppedContext
    {
        long Id { get; }
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

        IProcessConfiguration Configuration { get; }

        //void CheckErrorsAndStop();
    }

    class ProcessRetryContext: IProcessRetryContext
    {
        public ProcessRetryContext(long id, int processKey, ILogger logger, IProcessExecutionContext executionContext)
        {
            Id = id;
            ProcessKey = processKey;
            Logger = logger;
            ExecutionContext = executionContext;
        }

        public long Id { get; }
        public int ProcessKey { get; }
        public ILogger Logger { get; }

        public IProcessExecutionContext ExecutionContext { get; }
        internal bool StopFlag { get; set; } = false;
        public void Stop()
        {
            StopFlag = true;
        }

        public IProcessConfiguration Configuration
        {
            get { return ExecutionContext?.Configuration; }
        }

        //public void CheckErrorsAndStop()
        //{
        //    var stopNeeded = Configuration.ErrorThreshold.HasValue && Configuration.ErrorThreshold.Value>0;
        //}
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

    class ProcessCompleteContext: IProcessCompleteContext
    {
        public ProcessCompleteContext(long id, int processKey, bool isResubmission, bool isResubmitted, ILogger logger)
        {
            Id = id;
            ProcessKey = processKey;
            IsResubmission = isResubmission;
            IsResubmitted = isResubmitted;
            Logger = logger;
        }

        public long Id { get; }
        public int ProcessKey { get; }
        public bool IsResubmission { get; }
        public bool IsResubmitted { get; }
        public void Resubmit(string reason)
        {
            throw new System.NotImplementedException();
        }

        public ILogger Logger { get; }
    }

    class ProcessStoppedContext:IProcessStoppedContext
    {
        private readonly int _processId;

        public ProcessStoppedContext(long id, int processId, int processKey, string stopReason, ILogger logger)
        {
            _processId = processId;
            Id = id;
            ProcessKey = processKey;
            StopReason = stopReason;
            Logger = logger;
        }

        public long Id { get; }
        public int ProcessKey { get; }
        public string StopReason { get; }
        public ILogger Logger { get; }

        public int ProcessId
        {
            get { return _processId; }
        }
    }

}