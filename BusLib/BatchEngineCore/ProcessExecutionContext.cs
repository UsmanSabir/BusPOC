using System;

namespace BusLib.BatchEngineCore
{
    public interface IProcessExecutionContext:IMessage
    {
        //todo: id, processId, correlationId, nodeId, 
        
        DateTime UpdatedOn { get; }

        ProcessStatus Status { get; }

        int RetryCount { get; }

        int CompanyId { get; }

        int BranchId { get; }

        int SubTenantId { get; }
    }
}