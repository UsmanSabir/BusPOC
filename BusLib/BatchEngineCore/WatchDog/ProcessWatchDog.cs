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
        private readonly ICacheAside _cacheAside;
        private readonly ConcurrentDictionary<int,SubmittedGroup> _runningGroups=new ConcurrentDictionary<int, SubmittedGroup>();
        private readonly IBatchEngineSubscribers _batchEngineSubscribers;

        public ProcessWatchDog(ILogger logger, IStateManager stateManager,
            IBatchEngineSubscribers batchEngineSubscribers, ICacheAside cacheAside) : base("WatchDog", logger)
        {
            _stateManager = stateManager;
            _batchEngineSubscribers = batchEngineSubscribers;
            _cacheAside = cacheAside;
            _groupsHandler = new GroupsHandler(logger, batchEngineSubscribers, stateManager);
            Interval=TimeSpan.FromSeconds(30);
        }


        internal override void PerformIteration()
        {
            //todo lock other methods
            var groups = _stateManager.GetAllIncomplete<IGroupEntity>().ToList();
            foreach (var groupEntity in groups)
            {
                var processes = _stateManager.GetGroupProcess(groupEntity.Id).ToList(); //.GetPendingGroupProcess(groupEntity.Id).ToList();
                bool hasAnomaly = false;
                SubmittedGroup group;
                if (!_runningGroups.TryGetValue(groupEntity.Id, out group))
                {
                    group = new SubmittedGroup(groupEntity, processes);
                    _runningGroups.TryAdd(groupEntity.Id, group);
                    hasAnomaly = true;
                }
                //todo update _runningGroups and check completed/incomplete processes and tasks

                var processRunnings = processes.Where(p=>p.IsExecuting()).ToList();
                foreach (var running in processRunnings)
                {
                    var countPendingTasksForProcess = _stateManager.CountPendingTasksForProcess(running.Id);
                    if (countPendingTasksForProcess==0)
                    {
                        CheckProcessIdle(running.Id, running.GroupId);
                        hasAnomaly = true;
                    }
                }

                if (hasAnomaly)
                {
                    CheckGroupHealth(group);
                }
            }
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
                        SubmitVolumeRequest(processes, submittedGroup.GroupEntity);
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

        private void SubmitVolumeRequest(IEnumerable<IProcessState> processes, IGroupEntity groupDetailsGroupEntity)
        {
            try
            {
                foreach (var p in processes)
                {
                    var volumeMessage = new ProcessExecutionContext(LoggerFactory.GetProcessLogger(p.Id, p.ProcessKey), p);
                    Bus.Instance.HandleVolumeRequest(volumeMessage);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error submitting processes for volume generation. Message {e.Message}", e);
                //process should be marked as error in volume generation, hence no further processing
                _groupsHandler.StopGroup(groupDetailsGroupEntity, $"Error generating volume. Message {e.Message}");
            }
        }

        //todo: any pub/sub triggering point
        void CheckProcessIdle(int processId, int groupId)
        {
            if (_runningGroups.TryGetValue(groupId, out SubmittedGroup groupDetails))
            {
                var processState = groupDetails.ProcessEntities.FirstOrDefault(p=>p.Id==processId);
                if (processState == null)
                {
                    Logger.Warn($"ProcessId {processId} not in watchdog. Loading from persistance");
                    processState = _stateManager.GetProcessById(processId);
                    if (processState == null)
                    {
                        Logger.Error($"ProcessId {processId} not in watchdog and not in persistance");
                        Bus.Instance.EventAggregator.Publish(Constants.EventProcessFinished, processId.ToString());
                        CheckGroupHealth(groupDetails);
                        return;
                    }
                    else
                    {
                        groupDetails.ProcessEntities.Add(processState);
                    }
                }
                //todo check if process tasks are finished
                //check deferred tasks
                //check process retry
                //complete process
                var isProcessCompleted = CheckProcessCompletion(processState);

            if (isProcessCompleted)
            {
                //get child processes or other pending group processes
                //send for volume generation

                    var nextProcesses = groupDetails.GetNextProcesses(processId).ToList();
                    if (nextProcesses.Any())
                    {
                        Logger.Trace($"Submitting next requests, {nextProcesses.Count}");
                        SubmitVolumeRequest(nextProcesses, groupDetails.GroupEntity);
                    }
                    else
                    {
                        //no further child process(es), check group
                        CheckGroupHealth(groupDetails);
                    }
                }
            }
        }

        private void CheckGroupHealth(SubmittedGroup groupDetails)
        {
            if (groupDetails.GroupEntity.IsFinished)
            {
                _runningGroups.TryRemove(groupDetails.GroupEntity.Id, out groupDetails);
                Bus.Instance.EventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails));
            }
            else
            {
                //any process running right now
                var groupProcesses = groupDetails.ProcessEntities;// _stateManager.GetGroupProcess(groupDetails.GroupEntity.Id).ToList();
                
                if (groupProcesses.Any(p => p.IsExecuting()))
                {
                    Logger.Trace($"Group id {groupDetails.GroupEntity.Id} is running");

                    return;
                }

                //have any process not yet started
                var processesNotStartedYet = groupProcesses.Where(p=> p.StartTime.HasValue==false && p.IsStopped==false).ToList();
                if (processesNotStartedYet.Count == 0)
                {
                    //group processes completed i.e. no process executing and no to start

                    ResultStatus status;
                    TaskCompletionStatus completionStatus = TaskCompletionStatus.Finished;
                    if (groupProcesses.Any(p => p.IsStopped))
                    {
                        status = ResultStatus.Empty;
                        completionStatus= TaskCompletionStatus.Stopped;
                    }
                    else
                    {
                        status = groupProcesses.Any(p => ResultStatus.Error.Id == p.Status.Id)
                            ? ResultStatus.Error
                            : ResultStatus.Success;
                    }
                    groupDetails.GroupEntity.MarkGroupStatus(completionStatus,  status, "Execution end");
                    //todo cleanup group
                    Bus.Instance.EventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails));

                }
                else
                {
                    List<IProcessState> process2Start=new List<IProcessState>();
                    var rootProcess = groupProcesses.Where(p=>p.ParentId==null).ToList();
                    TraverseInComplete(rootProcess, process2Start, groupProcesses);
                    if (process2Start.Any())
                    {
                        SubmitVolumeRequest(process2Start, groupDetails.GroupEntity);
                    }
                    else
                    {
                        groupDetails.GroupEntity.MarkGroupStatus( TaskCompletionStatus.Stopped , ResultStatus.Empty, "Execution faulted");
                        Bus.Instance.EventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails));
                    }
                    //========
                    
                    //process2Start = processesNotStartedYet.Where(g=>g.ParentId==null).ToList();
                    //if (process2Start.Count == 0)
                    //{
                    //    //no root level process
                    //    var finishedProcesses = processesNotStartedYet.Where(p => p.IsFinished).ToList();
                    //    if (finishedProcesses.Count == 0)
                    //    {
                    //        _groupsHandler.StopGroup(groupDetails.GroupEntity, "Group process anomaly. ");
                    //    }

                    //    foreach (var process in groupProcesses)
                    //    {
                    //        if (process.IsStopped)
                    //        {
                    //            Logger.Info($"Process {process.Id} is stopped");
                    //            continue;
                    //        }
                    //        var parentId = process.ParentId.Value;
                    //        var parentProcess = groupProcesses.FirstOrDefault(p=>p.Id==parentId);
                    //        if (parentProcess == null)
                    //        {
                    //            _groupsHandler.StopGroup(groupDetails.GroupEntity,$"Process {process.Id} has invalid parent {parentId}");
                    //            return;
                    //        }

                    //        if (parentProcess.IsFinished)
                    //        {
                    //            process2Start.Add(process);
                    //        }
                    //    }
                    //}

                }

            }
        }

        private void TraverseInComplete(List<IProcessState> rootProcess, List<IProcessState> process2Start,
            List<IProcessState> groupProcesses)
        {
            var yetToStart = rootProcess.Where(p => p.Status.Id == ProcessStatus.New.Id && p.IsStopped == false).ToList();
            var finished = rootProcess.Where(p => p.IsFinished).ToList();
            process2Start.AddRange(yetToStart);

            foreach (var processState in finished)
            {
                var childProcess = groupProcesses.Where(p => p.ParentId.HasValue && p.ParentId.Value == processState.Id).ToList();
                TraverseInComplete(childProcess, process2Start, groupProcesses);
            }

        }


        bool CheckProcessCompletion(IProcessState state)
        {
            Logger.Trace($"Process Watchdog triggered for processId {state.Id}");

            var processId = state.Id;
            var configuration = _cacheAside.GetProcessConfiguration(state.ProcessKey);
            var processLogger = LoggerFactory.GetProcessLogger(processId, state.ProcessKey);

            if (!state.IsExecuting())
            {
                Logger.Trace($"Process Watchdog not executing with statue IsStopped {state.IsStopped}, IsFinished {state.IsFinished} for processId {processId}");
                Bus.Instance.EventAggregator.Publish(Constants.EventProcessFinished, processId.ToString());
                return true;
            }
            var timeoutMins = configuration.ProcessTimeoutMins ?? 0;

            if (!state.StartTime.HasValue)
            {
                state.MarkProcessStatus(TaskCompletionStatus.Finished, ResultStatus.Error,
                    $"Process start time is not marked for {processId}");
                Bus.Instance.EventAggregator.Publish(Constants.EventProcessFinished, processId.ToString());
                return true;
            }

            var incompleteTasks = _stateManager.GetIncompleteTasksForProcess<ITaskState>(processId).ToList();
            if (incompleteTasks.Any())
            {
                //check timeout
                var isTimedout = timeoutMins > 0 && state.StartTime.Value.AddMinutes(timeoutMins) < DateTime.UtcNow;
                if (isTimedout)
                {
                    processLogger.Error("Timeout");
                    state.MarkProcessStatus(TaskCompletionStatus.Finished, ResultStatus.Error,
                        $"Process timeout {processId}");
                    Bus.Instance.EventAggregator.Publish(Constants.EventProcessFinished, processId.ToString());
                    return true;
                }

                var deferredTasks = incompleteTasks.Where(d => d.DeferredCount > 0).ToList();
                Logger.Trace($"Process Watchdog skipped for processId {processId}. {incompleteTasks.Count} incomplete tasks & {deferredTasks.Count} deferred tasks");
                return false;
            }
            else
            {
                //check retry configured
                var erroredTasks = _stateManager.CountFailedTasksForProcess<ITaskState>(processId);
                
                if (erroredTasks > 0)
                {
                    if (configuration.ProcessRetries.HasValue)
                    {
                        var configuredRetries = configuration.ProcessRetries.Value;
                        if (state.RetryCount < configuredRetries)
                        {
                            ProcessRetryContext context =
                                new ProcessRetryContext(processId, state.ProcessKey, processLogger);
                            foreach (var processSubscriber in _batchEngineSubscribers.GetProcessSubscribers())
                            {
                                Robustness.Instance.SafeCall(() => processSubscriber.OnProcessRetry(context),
                                    Logger);
                                if (context.StopFlag)
                                {
                                    processLogger.Warn($"Retry stopped by extension {processSubscriber.GetType()}");

                                    state.MarkProcessStatus(TaskCompletionStatus.Finished, ResultStatus.Error,
                                        $"Retry stopped by extension {processSubscriber.GetType()}");
                                    Bus.Instance.EventAggregator.Publish(Constants.EventProcessStop,
                                        processId.ToString());

                                    return true;
                                }
                            }

                            //todo goto retry
                            processLogger.Warn($"Process going to retry with errors {erroredTasks}");
                            state.RetryProcess();
                            Bus.Instance.EventAggregator.Publish(Constants.EventProcessRetry, processId.ToString());
                            return false; //not yet completed
                        }
                    }

                    state.MarkProcessStatus(TaskCompletionStatus.Finished, ResultStatus.Error, $"Process completed with errors {erroredTasks}");
                    Bus.Instance.EventAggregator.Publish(Constants.EventProcessFinished, processId.ToString());
                    return true;
                }
                else
                {
                    state.MarkProcessStatus(TaskCompletionStatus.Finished, ResultStatus.Success, "Process completed");
                    Bus.Instance.EventAggregator.Publish(Constants.EventProcessFinished, processId.ToString());
                    return true;
                }
                
            }
        }

    }
}