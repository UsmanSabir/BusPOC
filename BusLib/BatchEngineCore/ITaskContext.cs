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
    }

    public interface ITaskContext<out T> : ITaskContext
    {
        T Data { get; }
    }

    internal interface ITaskMessage : IMessage
    {
        ITaskState TaskState { get; }
        ITransaction Transaction { get; }

        SafeDisposableActions OnCompleteActions { get; }

        ILogger Logger { get; }
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

        TaskStatus Status { get; }

        string CurrentState { get; }

        int FailedCount { get; }

        int DeferredCount { get; }

        string NodeKey { get; }
    }

}
