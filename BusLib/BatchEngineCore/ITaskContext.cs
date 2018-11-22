using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public interface ITaskContext: IDashboardContextMessage
    {
        //todo: id, processId, correlationId, nodeId, 
        //todo: think about process companyId, branch, subtenant

        ITaskState State { get; }

        IProcessExecutionContext ProcessExecutionContext { get; }
    }

    public interface ITaskContext<out T> : ITaskContext
    {
        
        T Data { get; }
    }

    public interface ISagaTaskContext<out T> : ITaskContext<T>
    {
        
        string PreviousState { get; }

        string NextState { get; }
                
    }

    public interface ITaskState
    {
        int Id { get; }

        int ProcessId { get; }

        DateTime UpdatedOn { get; }

        TaskStatus Status { get; }

        string CurrentState { get; }

        int RetryCount { get; }
    }
}
