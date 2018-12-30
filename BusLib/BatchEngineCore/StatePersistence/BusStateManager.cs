using System;
using System.Collections.Generic;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.StatePersistence
{
    public class BusStateManager:IStateManager
    {
        private readonly IStateManager _stateManagerImplementation;
        private readonly ILogger _logger;

        T Execute<T>(Func<T> func)
        {
            T ret = default;

            void Action() => ret = func();

            Execute(Action);
            
            return ret;
        }

        void Execute(Action act)
        {
            ActionCommand action = new ActionCommand(act);

            try
            {
                Bus.Instance.HandleStateManagerCommand(action);
            }
            catch (Exception e)
            {
                _logger.Fetal($"Error executing state command. {e.Message}", e);
                //todo shutdown
            }
        }

        public BusStateManager(IStateManager stateManagerImplementation, ILogger logger)
        {
            _stateManagerImplementation = stateManagerImplementation;
            this._logger = logger;
        }

        public IEnumerable<IReadWritableGroupEntity> GetAllIncomplete()
        {
            return Execute(_stateManagerImplementation.GetAllIncomplete);
        }

        public List<KeyValuePair<string, string>> GetTaskStates(long taskId, long processId)
        {
            return Execute(()=>_stateManagerImplementation.GetTaskStates(taskId, processId));
        }

        public IEnumerable<int> GetConfiguredGroupProcessKeys(long groupEntityId)
        {
            return Execute(()=>_stateManagerImplementation.GetConfiguredGroupProcessKeys(groupEntityId));
        }

        public IEnumerable<IReadWritableProcessState> GetSubmittedGroupProcesses(long groupEntityId)
        {
            return Execute(()=>_stateManagerImplementation.GetSubmittedGroupProcesses(groupEntityId));
        }

        public IEnumerable<ITaskState> GetIncompleteTasksForProcess(long processId)
        {
            return Execute(()=>_stateManagerImplementation.GetIncompleteTasksForProcess(processId));
        }

        public long CountFailedTasksForProcess<T>(long processId)
        {
            return Execute(() => _stateManagerImplementation.CountFailedTasksForProcess<T>(processId));
        }

        public long CountPendingTasksForProcess(long processId)
        {
            return Execute(() => _stateManagerImplementation.CountPendingTasksForProcess(processId));
        }

        public IReadWritableProcessState GetAndStartProcessById(long processId)
        {
            return Execute(() => _stateManagerImplementation.GetAndStartProcessById(processId));
        }

        public IReadWritableProcessState GetProcessById(long processId)
        {
            return Execute(() => _stateManagerImplementation.GetProcessById(processId));
        }

        public void AddGroupProcess(List<IReadWritableProcessState> groupProcesses)
        {
            Execute(() =>_stateManagerImplementation.AddGroupProcess(groupProcesses));
        }

        public void SaveGroup(IReadWritableGroupEntity @group)
        {
            Execute(() => _stateManagerImplementation.SaveGroup(@group));
        }

        public IReadWritableGroupEntity CreateGroupEntity(IReadWritableGroupEntity entity)
        {
            return Execute(() => _stateManagerImplementation.CreateGroupEntity(entity));
        }

        public IReadWritableGroupEntity GetGroupEntity(long groupId)
        {
            return Execute(() => _stateManagerImplementation.GetGroupEntity(groupId));
        }

        public IProcessConfiguration GetProcessConfiguration(int key)
        {
            return Execute(() => _stateManagerImplementation.GetProcessConfiguration(key));
        }

        public void SaveProcess(IProcessState process)
        {
            Execute(() => _stateManagerImplementation.SaveProcess(process));
        }

        public void SaveTaskStates(long taskId, long processId, List<KeyValuePair<string, string>> states)
        {
            Execute(() => _stateManagerImplementation.SaveTaskStates(taskId, processId, states));
        }

        public void SaveTaskState(long taskId, long processId, string key, string val)
        {
            Execute(() => _stateManagerImplementation.SaveTaskState(taskId, processId, key, val));
        }

        public void UpdateTask(ITaskState task, ITransaction runningTransaction)
        {
            Execute(() => _stateManagerImplementation.UpdateTask(task, runningTransaction));
        }

        public int MarkProcessForRetry(IReadWritableProcessState process)
        {
            return Execute(() => _stateManagerImplementation.MarkProcessForRetry(process));
        }

        public void MarkGroupStopped(IGroupEntity @group)
        {
            Execute(() => _stateManagerImplementation.MarkGroupStopped(@group));
        }

        public bool IsHealthy()
        {
            return _stateManagerImplementation.IsHealthy();
        }
    }
}