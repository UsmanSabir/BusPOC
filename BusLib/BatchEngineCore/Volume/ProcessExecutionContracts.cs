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

        IProcessConfiguration Configuration { get; }

        bool AddUpdateProcessData<T>(string key, T value);

        T GetProcessData<T>(string key);

        object GetProcessData(string key);


        bool SetTempData(string key, object value);
        T GetTempData<T>(string key);

        object GetTempData(string key);

        //IFrameworkLogger FrameworkLogger { get; }
    }

    public interface IProcessState
    {
        long Id { get; }

        Guid CorrelationId { get; }

        DateTime? UpdatedOn { get; }

        CompletionStatus Status { get; }

        int RetryCount { get; }

        int CompanyId { get; }

        int BranchId { get; }

        int SubTenantId { get; }

        DateTime ProcessingDate { get; }

        int ProcessKey { get; }

        bool IsVolumeGenerated { get; }

        long? ParentId { get; }

        long GroupId { get; } 

        bool IsFinished { get; }
        bool IsStopped { get; }
        string Criteria { get; } //todo
        DateTime? StartTime { get; }
        DateTime? CompleteTime { get; }
        DateTime? GenerationCompleteTime { get; }

        ResultStatus Result { get; }
    }

    public interface IWritableProcessState
    {
        long Id { set; }

        Guid CorrelationId { set; }

        DateTime? UpdatedOn { set; }

        CompletionStatus Status { set; }

        int RetryCount { set; }

        int CompanyId { set; }

        int BranchId { set; }

        int SubTenantId { set; }

        DateTime ProcessingDate { set; }

        int ProcessKey { set; }

        bool IsVolumeGenerated { set; }

        long? ParentId { set; }

        long GroupId { set; }

        bool IsFinished { set; }
        bool IsStopped { set; }
        string Criteria { set; } //todo
        DateTime? StartTime { set; }
        DateTime? CompleteTime { set; }
        DateTime? GenerationCompleteTime { set; }
        ResultStatus Result { set; }
    }

    public interface IReadWritableProcessState: IWritableProcessState, IProcessState
    {
        new long Id { get; set; }

        new Guid CorrelationId { get; set; }
        new DateTime? UpdatedOn { get; set; }
        new CompletionStatus Status { get; set; }
        new int RetryCount { get; set; }
        new int CompanyId { get; set; }
        new int BranchId { get; set; }
        new int SubTenantId { get; set; }
        new DateTime ProcessingDate { get; set; }
        new int ProcessKey { get; set; }
        new bool IsVolumeGenerated { get; set; }
        new long? ParentId { get; set; }
        new long GroupId { get; set; }
        new bool IsFinished { get; set; }
        new bool IsStopped { get; set; }
        new string Criteria { get; set; } 
        new DateTime? StartTime { get; set; }
        new DateTime? CompleteTime { get; set; }
        new DateTime? GenerationCompleteTime { get; set; }
        new ResultStatus Result { get; set; }
    } 
}