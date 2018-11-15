using BusLib.Core;
using System;

namespace BusLib.BatchEngineCore
{
    public interface IProcessExecutionContext:IMessage
    {
        //todo: id, processId, correlationId, nodeId, 

        IProcessState ProcessState { get; }

        ILogger Logger { get; }

        IDashboardService DashboardService { get; }

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