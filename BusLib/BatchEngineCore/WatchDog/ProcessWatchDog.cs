using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.PubSub;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.WatchDog
{
    internal class ProcessWatchDog:RepeatingProcess, IHandler<GroupMessage>
    {
        private readonly IStateManager _stateManager;
        private readonly GroupsHandler _groupsHandler;
        private readonly ConcurrentDictionary<int,SubmittedGroup> _runningGroups=new ConcurrentDictionary<int, SubmittedGroup>();
        private IBatchEngineSubscribers _batchEngineSubscribers;

        public ProcessWatchDog(ILogger logger, IStateManager stateManager,
            IBatchEngineSubscribers batchEngineSubscribers) : base("WatchDog", logger)
        {
            _stateManager = stateManager;
            _batchEngineSubscribers = batchEngineSubscribers;
            _groupsHandler = new GroupsHandler(logger, batchEngineSubscribers, stateManager);
            Interval=TimeSpan.FromSeconds(30);
        }


        internal override void PerformIteration()
        {
            var groups = _stateManager.GetAllIncomplete<IGroupEntity>().ToList();
            foreach (var groupEntity in groups)
            {
                var processes = _stateManager.GetPendingGroupProcess(groupEntity.Id).ToList();

            }


            //todo
            //get all pending groups, processes
            //check their tasks
        }

        //todo: any pub/sub triggering point
        void CheckProcessIdle(int processId, int groupId)
        {
            //todo check if process tasks are finished
            //check deferred tasks
            //check process retry
            //complete process

            //get child processes or other pending group processes
            //send for volume generation
        }

        public void Handle(GroupMessage message)
        {
            if (GroupActions.Start.Id == message.Action.Id)
            {
                var submittedGroup = _groupsHandler.CreateGroup(message.Group);
                if (submittedGroup != null)
                {
                    var add = _runningGroups.TryAdd(submittedGroup.GroupEntity.Id, submittedGroup);
                    if (!add)
                    {
                        Logger.Error($"Failed to add group in process queue. Id {submittedGroup.GroupEntity.Id}");
                    }
                    else
                    {
                        var processes = submittedGroup.GetNextProcesses(null);
                        foreach (var p in processes)
                        {
                            var volumeMessage = new ProcessExecutionContext(LoggerFactory.GetProcessLogger(p.Id, p.ProcessKey), p);
                            Bus.Instance.HandleVolumeRequest(volumeMessage);
                        }
                        
                    }
                }
            }
            else if (GroupActions.Stop.Id == message.Action.Id)
            {
                _groupsHandler.StopGroup(message.Group, message.Message);
                if (!_runningGroups.TryRemove(message.Group.Id, out SubmittedGroup g))
                {
                    Logger.Warn($"Failed to remove group from process queue. Id {message.Group.Id}");
                }

            }
            
        }


        bool CheckProcessHealth(int processId)
        {
            bool handled = false;
            var incompleteTasks = _stateManager.GetIncompleteTasksForProcess<ITaskState>(processId).ToList();
            if (incompleteTasks.Any())
            {
                var defferedTasks = incompleteTasks.Where(d => d.DeferredCount > 0).ToList();

            }
            else
            {
                var erroredTasks = _stateManager.GetFailedTasksForProcess<ITaskState>(processId).ToList();
                if (erroredTasks.Any())
                {
                    IProcessResumeContext context = null;//todo
                    foreach (var processSubscriber in _batchEngineSubscribers.GetProcessSubscribers())
                    {
                        Robustness.Instance.SafeCall(()=> processSubscriber.OnProcessResume(context), Logger);
                    }
                    //todo goto retry
                    handled = true;
                    return handled;
                }
            }

            return handled;
        }

    }
}