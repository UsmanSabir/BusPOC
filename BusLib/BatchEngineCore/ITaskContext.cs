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

        bool Defer();

        int DeferredCount { get; }

        //void MarkStatus(TaskStatus status, ResultStatus result, string reason); //todo hide from implementation, maybe extension method
        bool SetNextState(string next);
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
        public TaskMessage(IReadWritableTaskState taskState, ITransaction transaction, SafeDisposableActions onCompleteActions, ILogger logger)
        {
            TaskStateWritable = taskState;
            Transaction = transaction;
            OnCompleteActions = onCompleteActions;
            Logger = logger;
        }

        public ITaskState TaskState => TaskStateWritable;
        internal IReadWritableTaskState TaskStateWritable { get; }
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

    public interface ITaskState: ICompletableState
    {
        long Id { get; }

        long ProcessId { get; }

        string Payload { get; }

        DateTime? UpdatedOn { get; }

        ResultStatus Status { get; }

        string CurrentState { get; }

        int FailedCount { get; }

        int DeferredCount { get; }

        string NodeKey { get; }

        DateTime? StartedOn { get; }

        DateTime? CompletedOn { get; }
    }

    public interface IWritableTaskState : IWritableCompletableState
    {
        long Id { set; }

        long ProcessId { set; }

        string Payload { set; }

        DateTime? UpdatedOn { set; }

        ResultStatus Status { set; }

        string CurrentState { set; }

        int FailedCount { set; }

        int DeferredCount { set; }

        string NodeKey { set; }

        DateTime? StartedOn { set; }

        DateTime? CompletedOn { set; }
    }

    public interface IReadWritableTaskState : IWritableTaskState, ITaskState
    {
        new long Id { get; set; }

        new long ProcessId { get; set; }

        new string Payload { get; set; }

        new DateTime? UpdatedOn { get; set; }

        new ResultStatus Status { get; set; }

        new string CurrentState { get; set; }

        new int FailedCount { get; set; }

        new int DeferredCount { get; set; }

        new string NodeKey { get; set; }

        new bool IsFinished { get; set; }

        new bool IsStopped { get; set; }

        new DateTime? StartedOn { get; set; }

        new DateTime? CompletedOn { get; set; }
    }
}
