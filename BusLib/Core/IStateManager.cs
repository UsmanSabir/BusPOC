using BusLib.BatchEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Groups;

namespace BusLib.Core
{
    public interface IStateManager
    {
        IEnumerable<IReadWritableGroupEntity> GetAllIncomplete();

        //void Save<T>(T item) where T: ICompletableState;

        //T GetById<T>(object id);

        //IEnumerable<T> Get<T>(Predicate<T> predicate);

        //void AddTaskState(ITaskContext taskContext, string key, string value); // Add state command

        List<KeyValuePair<string, string>> GetTaskStates(long taskId, long processId);
        //IEnumerable<IProcessState> GetPendingGroupProcess(int groupEntityId);
        IEnumerable<int> GetConfiguredGroupProcessKeys(long groupEntityId);

        IEnumerable<IReadWritableProcessState> GetSubmittedGroupProcesses(long groupEntityId);
        //ITransaction BeginTransaction();
        //T Insert<T>(T process);
        IEnumerable<ITaskState> GetIncompleteTasksForProcess(long processId);
        //IEnumerable<T> GetFailedTasksForProcess<T>(int processId);
        long CountFailedTasksForProcess<T>(long processId);
        long CountPendingTasksForProcess(long processId);
        IReadWritableProcessState GetAndStartProcessById(long processId);
        IReadWritableProcessState GetProcessById(long processId);
        //IReadWritableProcessState GetProcessByKey(int processKey);
        void AddGroupProcess(List<IReadWritableProcessState> groupProcesses, IGroupEntity groupEntity);
        void SaveGroup(IReadWritableGroupEntity group);
        IReadWritableGroupEntity CreateGroupEntity(IReadWritableGroupEntity entity);
        IReadWritableGroupEntity GetGroupEntity(long groupId);
        IProcessConfiguration GetProcessConfiguration(int key);
        void SaveProcess(IProcessState process);
        void SaveTaskStates(long taskId, long processId, List<KeyValuePair<string, string>> states);
        void SaveTaskState(long taskId, long processId, string key, string val);
        void UpdateTask(ITaskState task, ITransaction runningTransaction);
        int MarkProcessForRetry(IReadWritableProcessState process);
        void MarkGroupStopped(IGroupEntity @group);

        bool IsHealthy();
    }

    //to have wrapper
    public interface IStatePersistenceService : IStateManager
    {
        
    }
}
