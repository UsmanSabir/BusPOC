using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;

namespace BusImpl
{
    public class StatePersistenceService : IStatePersistenceService
    {
        public void AddGroupProcess(List<IReadWritableProcessState> groupProcesses, IGroupEntity groupEntity)
        {
            throw new NotImplementedException();
        }

        public long CountFailedTasksForProcess<T>(long processId)
        {
            throw new NotImplementedException();
        }

        public long CountPendingTasksForProcess(long processId)
        {
            throw new NotImplementedException();
        }

        public IReadWritableGroupEntity CreateGroupEntity(IReadWritableGroupEntity entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReadWritableGroupEntity> GetAllIncomplete()
        {
            throw new NotImplementedException();
        }

        public IReadWritableProcessState GetAndStartProcessById(long processId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> GetConfiguredGroupProcessKeys(long groupEntityId)
        {
            throw new NotImplementedException();
        }

        public IReadWritableGroupEntity GetGroupEntity(long groupId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITaskState> GetIncompleteTasksForProcess(long processId)
        {
            throw new NotImplementedException();
        }

        public IReadWritableProcessState GetProcessById(long processId)
        {
            throw new NotImplementedException();
        }

        public IProcessConfiguration GetProcessConfiguration(int key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReadWritableProcessState> GetSubmittedGroupProcesses(long groupEntityId)
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<string, string>> GetTaskStates(long taskId, long processId)
        {
            throw new NotImplementedException();
        }

        public bool IsHealthy()
        {
            throw new NotImplementedException();
        }

        public void MarkGroupStopped(IGroupEntity group)
        {
            throw new NotImplementedException();
        }

        public int MarkProcessForRetry(IReadWritableProcessState process)
        {
            throw new NotImplementedException();
        }

        public void SaveGroup(IReadWritableGroupEntity group)
        {
            throw new NotImplementedException();
        }

        public void SaveProcess(IProcessState process)
        {
            throw new NotImplementedException();
        }

        public void SaveTaskState(long taskId, long processId, string key, string val)
        {
            throw new NotImplementedException();
        }

        public void SaveTaskStates(long taskId, long processId, List<KeyValuePair<string, string>> states)
        {
            throw new NotImplementedException();
        }

        public void UpdateTask(ITaskState task, ITransaction runningTransaction)
        {
            throw new NotImplementedException();
        }
    }
}