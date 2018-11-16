using BusLib.Core;
using System;

namespace BusLib.BatchEngineCore
{
    public interface IExecutionContextMessage:IMessage
    {
        Guid CorrelationId { get; }
        ILogger Logger { get; }
    }

    public interface IDashboardContextMessage:IExecutionContextMessage
    {
        IDashboardService DashboardService { get; }
    }

    public interface IProcessExecutionContext: IDashboardContextMessage
    {
        //todo: id, processId, correlationId, nodeId, 

        IProcessState ProcessState { get; }
        
        

        //IFrameworkLogger FrameworkLogger { get; }
    }

    public interface IProcessState
    {
        DateTime UpdatedOn { get; }

        ProcessStatus Status { get; }

        int RetryCount { get; }

        int CompanyId { get; }

        int BranchId { get; }

        int SubTenantId { get; }

        string ProcessKey { get; }

        bool IsVolumeGenerated { get; }
    }
}