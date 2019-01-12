using BusLib.BatchEngineCore;

namespace BusImpl
{
    public class Constant
    {
        public static readonly string SQLCountFailedTasks =
            $"SELECT COUNT(1) FROM BatchTaskState WITH(NOLOCK) WHERE CurrentState = '{ResultStatus.Error.Name}' AND ProcessId=@pid";

        public static readonly string SQLRetryFailedTasks =
            $"UPDATE BatchTaskState SET IsFinished = 0, NodeKey=NULL, CurrentState='{ResultStatus.Empty.Name}' WHERE CurrentState = 'Error' AND ProcessId=@0"; //todo what about start/complete date time, DeferredCount, IsStopped
        
        public static readonly string SQLCountPendingTasks =
            $"SELECT COUNT(1) FROM BatchTaskState WITH(NOLOCK) WHERE IsFinished = 0 AND IsStopped = 0 AND NodeKey IS NULL AND ProcessId=@pid"; //(CurrentState='' OR CurrentState IS NULL OR CurrentState='{ResultStatus.Empty.Name}') AND 

        //public const string SQLReadDataQueue =
        //    @"SELECT TOP(1) * FROM BatchTaskState WITH(UPDLOCK, READPAST) WHERE IsFinished=0 AND IsStopped=0  ORDER BY ProcessId, DeferredCount";

        public static readonly string SQLReadDataQueue = string.Format(
            @"UPDATE BatchTaskState SET NodeKey='{0}', StartedOn = @0
OUTPUT Inserted.*
FROM (SELECT TOP 1 id FROM dbo.BatchTaskState WITH (NOLOCK) WHERE IsFinished = 0 AND IsStopped=0 AND NodeKey IS NULL ORDER  BY ProcessId, DeferredCount,Id) t
WHERE BatchTaskState.id = t.Id", NodeSettings.Instance.Name); //todo optimize with CTE


        public const string SQLUpdateTaskState =
            @"UPDATE dbo.BatchTaskValue
	SET StateValue = @2
WHERE TaskId = @0 AND StateKey=@1;";

        public const string SQLStopProcessTaskState =
            @"UPDATE dbo.BatchTaskState
  SET IsStopped=1,
    UpdatedOn=@1,
	CurrentState=@2,
	NodeKey=@3
WHERE ProcessId = @0 AND NodeKey IS NULL AND CurrentState=@4;";

        public const string SQLPing = "SELECT GETUTCDATE()";

        public const string SQLDeleteProcessVolume = @"DELETE FROM dbo.BatchTaskState WHERE ProcessId = @0";


    }
}