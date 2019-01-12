using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.BatchEngineCore.Groups;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Infrastructure;

namespace BusLib.BatchEngineCore.StatePersistence
{
    public class BusStateManager:IStateManager
    {
        private readonly IStateManager _stateManagerImplementation;
        private readonly ILogger _logger;

        private readonly IResolver _resolver;
        private Bus _bus;

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
                Bus.HandleStateManagerCommand(action);
            }
            catch(TaskCanceledException)
            {
                _logger.Info($"StateManager command cancelled.");
            }
            catch (OperationCanceledException)
            {
                _logger.Info($"StateManager command cancelled.");
            }
            catch (FrameworkException)
            {
                throw;//its ok
            }
            catch (AggregateException aex) when (aex.InnerExceptions.Any(r => r is FrameworkException))
            {
                var fex = aex.InnerExceptions.First(r => r is FrameworkException);
                //if (fex != null)
                {
                    ExceptionDispatchInfo.Capture(fex).Throw();
                }
            }
            catch (Exception e) when(!(e  is FrameworkException))
            {
                _logger.Fatal($"Error executing state command. {e.Message}", e);
                //todo shutdown
            }
        }

        private Bus Bus
        {
            get { return _bus ?? (_bus = _resolver.Resolve<Bus>()); }
        }

        public BusStateManager(IStateManager stateManagerImplementation, ILogger logger, IResolver resolver)
        {
            _stateManagerImplementation = stateManagerImplementation;
            this._logger = logger;
            _resolver = resolver;

            //this._bus = _resolver.Resolve<Bus>();
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

        public long GetIncompleteTasksCountForProcess(long processId)
        {
            return Execute(() => _stateManagerImplementation.GetIncompleteTasksCountForProcess(processId));
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

        public void AddGroupProcess(List<IReadWritableProcessState> groupProcesses, IGroupEntity groupEntity)
        {
            Execute(() =>_stateManagerImplementation.AddGroupProcess(groupProcesses, groupEntity));
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

        public void UpdateTask(ITaskState task, ITransaction runningTransaction, bool commit)
        {
            Execute(() => _stateManagerImplementation.UpdateTask(task, runningTransaction, commit));
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

        public IEnumerable<IReadWritableProcessState> GetIncompleteProcesses()
        {
            return Execute(() => _stateManagerImplementation.GetIncompleteProcesses());
        }
    }
}