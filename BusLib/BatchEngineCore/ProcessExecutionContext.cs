using BusLib.Core;
using System;

namespace BusLib.BatchEngineCore
{
    public interface IExecutionContextMessage:IMessage
    {
        //Guid CorrelationId { get; }
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

        ProcessConfiguration Configuration { get; }

        bool AddUpdateProcessData(string key, object value);

        T GetProcessData<T>(string key);

        object GetProcessData(string key);


        bool SetTempData(string key, object value);
        T GetTempData<T>(string key);

        object GetTempData(string key);

        //IFrameworkLogger FrameworkLogger { get; }
    }

    public interface IProcessState
    {
        int Id { get; }

        Guid CorrelationId { get; }

        DateTime UpdatedOn { get; }

        ProcessStatus Status { get; }

        int RetryCount { get; }

        int CompanyId { get; }

        int BranchId { get; }

        int SubTenantId { get; }

        DateTime ProcessingDate { get; }

        int ProcessKey { get; }

        bool IsVolumeGenerated { get; }

        int? ParentId { get; }

        int GroupId { get; }

        bool IsFinished { get; }
    }
}