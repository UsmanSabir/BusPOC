using System;
using System.Threading;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore
{
    public interface ITaskContext: IDashboardContextMessage, IDisposable
    {
        //todo: id, processId, correlationId, nodeId, 
        //todo: think about process companyId, branch, subtenant

        ITaskState State { get; }

        string PrevState { get; }

        string NextState { get; }


        IProcessExecutionContext ProcessExecutionContext { get; }

        ITransaction Transaction { get; }
        CancellationToken CancellationToken { get; }
        ResultStatus Result { get; set; }

        //void MarkStatus(TaskStatus status, ResultStatus result, string reason); //todo hide from implementaion, maybe extension method
    }

    //public interface ITaskContext<out T> : ITaskContext
    //{
    //    T Data { get; }
    //}

    internal interface ITaskMessage : IMessage
    {
        ITaskState TaskState { get; }
        ITransaction Transaction { get; }

        SafeDisposableActions OnCompleteActions { get; }

        ILogger Logger { get; }
    }

    class TaskMessage:SafeDisposable, ITaskMessage
    {
        public TaskMessage(ITaskState taskState, ITransaction transaction, SafeDisposableActions onCompleteActions, ILogger logger)
        {
            TaskState = taskState;
            Transaction = transaction;
            OnCompleteActions = onCompleteActions;
            Logger = logger;
        }

        public ITaskState TaskState { get; }
        public ITransaction Transaction { get; }
        public SafeDisposableActions OnCompleteActions { get; internal set; }
        public ILogger Logger { get; }
        public IProcessExecutionContext ProcessContext { get; set; }

        protected override void Dispose(bool disposing)
        {
            OnCompleteActions?.Dispose();
            base.Dispose(disposing);
        }
    }

    //public interface ISagaTaskContext<out T> : ITaskContext<T>
    //{
        
    //    string PreviousState { get; }

        
                
    //}

    public interface ITaskState
    {
        int Id { get; }

        int ProcessId { get; }

        string Payload { get; }

        DateTime UpdatedOn { get; }

        TaskCompletionStatus Status { get; }

        string CurrentState { get; }

        int FailedCount { get; }

        int DeferredCount { get; }

        string NodeKey { get; }

        bool IsFinished { get; }
    }

}
