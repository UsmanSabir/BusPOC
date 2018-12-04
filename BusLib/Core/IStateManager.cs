using BusLib.BatchEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface IStateManager
    {
        IEnumerable<T> GetAllIncomplete<T>() where T : ICompletableState;

        void Save<T>(T item) where T: ICompletableState;

        T GetById<T>(object id);

        IEnumerable<T> Get<T>(Predicate<T> predicate);

        void AddTaskState(ITaskContext taskContext, string key, string value); // Add state command

        List<KeyValuePair<string, string>> GetTaskStates(int taskId, int processId);
        IEnumerable<IProcessState> GetPendingGroupProcess(int groupEntityId);
        IEnumerable<IProcessState> GetGroupProcess(int groupEntityId);
        ITransaction BeginTransaction();
        T Insert<T>(T process);
        IEnumerable<T> GetIncompleteTasksForProcess<T>(int processId);
        IEnumerable<T> GetFailedTasksForProcess<T>(int processId);
        long CountFailedTasksForProcess<T>(int processId);
        long CountPendingTasksForProcess(int processId);
        IProcessState GetProcessById(int processId);
    }
}
