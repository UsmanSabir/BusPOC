using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.Process;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Core.Events;
using BusLib.Helper;
using BusLib.Infrastructure;
using BusLib.Serializers;

namespace BusLib.BatchEngineCore.WatchDog
{
    internal class ProcessWatchDog:RepeatingProcess, IHandler<IWatchDogMessage> //IHandler<GroupMessage>, 
    {
        private readonly IStateManager _stateManager;
        private readonly GroupsHandler _groupsHandler;
        private readonly ICacheAside _cacheAside;
        private readonly IEventAggregator _eventAggregator;
        private readonly ConcurrentDictionary<long, SubmittedGroup> _runningGroups=new ConcurrentDictionary<long, SubmittedGroup>();
        private readonly IBatchEngineSubscribers _batchEngineSubscribers;

        readonly ReaderWriterLockSlim _lock=new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly IProcessDataStorage _storage;
        private readonly IPubSubFactory _pubSubFactory;
        private readonly IResolver _resolver;
        private readonly IBatchLoggerFactory _loggerFactory;
        private readonly IProcessRepository _registeredProcesses;
        private readonly ProcessVolumePipeline _volumePipeline;


        private readonly TinyMessageSubscriptionToken _subRem;
        private Bus _bus;
        private TinyMessageSubscriptionToken _checkGroupSub;

        private IDistributedMessageSubscriber _slaveMessageSubscriber;

        private readonly ISerializer _serializer;

        private IDistributedMessagePublisher _publisher;

        private readonly TinyMessageSubscriptionToken _volumeGenSub;

        private readonly TinyMessageSubscriptionToken _retrySub;

        private readonly TinyMessageSubscriptionToken _healthSub;

        private IFrameworkLogger _systemLogger;

        private TinyMessageSubscriptionToken _subGroupRemoved;

        private TinyMessageSubscriptionToken _processAddedSub;

        private TinyMessageSubscriptionToken _volErrorSub;
        //object _syncLock=new object();

        public ProcessWatchDog(ILogger logger, IStateManager stateManager,
            IBatchEngineSubscribers batchEngineSubscribers, ICacheAside cacheAside,
            ISerializersFactory serializersFactory, IEntityFactory entityFactory, IEventAggregator eventAggregator,
            IProcessDataStorage storage, IPubSubFactory pubSubFactory, IResolver resolver,
            IBatchLoggerFactory loggerFactory, IProcessRepository registeredProcesses,
            ProcessVolumePipeline volumePipeline) : base("WatchDog", logger)
        {
            _stateManager = stateManager;
            _batchEngineSubscribers = batchEngineSubscribers;
            _cacheAside = cacheAside;
            _eventAggregator = eventAggregator;
            _storage = storage;
            _pubSubFactory = pubSubFactory;
            _resolver = resolver;
            _loggerFactory = loggerFactory;
            _registeredProcesses = registeredProcesses;
            _volumePipeline = volumePipeline;
            _groupsHandler = new GroupsHandler(logger, batchEngineSubscribers, stateManager, serializersFactory, entityFactory, resolver, loggerFactory);
            Interval=TimeSpan.FromMinutes(3);

            _subGroupRemoved = eventAggregator.Subscribe4Broadcast<ProcessGroupRemovedMessage>(RemoveGroup);
            _subRem = eventAggregator.Subscribe<TextMessage>(ProcessRemoved, Constants.EventProcessFinished);

            _serializer = SerializersFactory.Instance.GetSerializer(typeof(GroupMessage));

            this._volumeGenSub = eventAggregator.Subscribe<TextMessage>(OnVolumeGenerated, Constants.EventProcessVolumeGenerated);
            _checkGroupSub =
                eventAggregator.Subscribe<TextMessage>(CheckProcessGroup, Constants.EventCheckGroupCommand);
            _retrySub = eventAggregator.Subscribe<TextMessage>(OnVolumeGenerated, Constants.EventProcessRetry);
            _healthSub = eventAggregator.Subscribe4Broadcast<HealthMessage>(PublishHealth);
            _systemLogger = _loggerFactory.GetSystemLogger();

            this._processAddedSub = eventAggregator.Subscribe<TextMessage>(OnProcessGroupAdded, Constants.EventProcessGroupAdded);
            _volErrorSub = eventAggregator.Subscribe<VolumeErrorMessage>(OnProcessVolumeError);
            
        }

        private void OnProcessVolumeError(VolumeErrorMessage msg)
        {
            var state = msg.Content.WritableProcessState;
            var config = msg.Content.Configuration;
            if (config.ErrorThreshold.HasValue && config.ErrorThreshold.Value > 0)
            //if (_runningGroups.TryGetValue(state.GroupId, out SubmittedGroup grp))
            {
                var message = $"Process QId {state.Id}, PId {state.ProcessId} volume generation failed. Going to stop group.";
                msg.Content.Logger.Error(message);
                try
                {
                    _groupsHandler.StopGroup(msg.Content.GroupEntity, message);
                }
                catch (Exception e)
                {
                    _systemLogger.Fatal("Failed to stop group with error {error}", e);
                }
            }
            else
            {
                _systemLogger.Info("VolumeError: Process ErrorThreshold not configured.");
            }
        }

        private void OnProcessGroupAdded(TextMessage msg)
        {
            if (long.TryParse(msg.Parameter, out long groupId))
            {
                HandleNewGroup(groupId);
            }
        }

        private void CheckProcessGroup(TextMessage groupMessage)
        {
            Logger.Trace("Check group command received with groupId {groupId}", groupMessage.Parameter);
            
            if (long.TryParse(groupMessage.Parameter, out long groupId))
            {
                if (_runningGroups.TryGetValue(groupId, out SubmittedGroup group))
                {
                    CheckGroupHealth(group);
                }
                else
                {
                    Logger.Warn("Group {groupId} not in running pool", groupId);
                    var groupEntity = _stateManager.GetGroupEntity(groupId);
                    if (groupEntity != null)
                    {
                        CheckSyncGroup(groupEntity);
                    }
                    else
                    {
                        Logger.Error("Group entity {groupId} not found", groupId);
                    }
                }
            }
        }

        private void PublishHealth(HealthMessage health)
        {
            Logger.Trace($"HealthCheck '{nameof(ProcessWatchDog)}' Start");
            HealthBlock block = new HealthBlock()
            {
                Name = "ProcessWatchDog"
            };

            block.AddMatrix("IsActive", !IsDisposed && !Interrupter.IsCancellationRequested);

            health.Add(block);
            Logger.Trace($"HealthCheck '{nameof(ProcessWatchDog)}' End");
        }

        private void ProcessRemoved(TextMessage msg)
        {
            Robustness.Instance.SafeCall(() =>
            {
                _storage.CleanProcessData(msg.Parameter);
            });
            

            if (long.TryParse(msg.Parameter, out var processId))
            {
                Robustness.Instance.SafeCall(() =>
                {
                    var packet = _serializer.SerializeToString(new ProcessRemovedWatchDogMessage()
                    {
                        ProcessId = processId
                    });
                    _publisher?.PublishMessage(packet, nameof(ProcessRemovedWatchDogMessage));

                }, Logger);
            }
        }

        private void RemoveGroup(ProcessGroupRemovedMessage obj)
        {
            //lock allow re-entrancy

            //Task.Run(() =>{
            _lock.EnterReadLock();
            try
            {

                //lock (_syncLock)
                {
                    _runningGroups.TryRemove(obj.Group.GroupEntity.Id, out _);
                }
                //});
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        internal override void PerformIteration()
        {
            //lock (_syncLock)
            _lock.EnterWriteLock();
            try
            {

                SyncWatchDog();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void SyncWatchDog()
        {
            //todo lock other methods
            var groups = _stateManager.GetAllIncomplete().ToList();

            foreach (var groupEntity in groups)
            {
                if (groupEntity.IsGenerated == false && !_runningGroups.ContainsKey(groupEntity.Id))
                {
                    HandleNewGroup(groupEntity.Id);//new group request
                }
                else
                {
                    CheckSyncGroup(groupEntity);
                }
            }

            var orphanGroups = _runningGroups.Values.Where(r=> groups.All(g => g.Id != r.GroupEntity.Id)).ToList();
            foreach (var orphan in orphanGroups)
            {
                CheckGroupHealth(orphan);
            }

            var incompleteProcesses = _stateManager.GetIncompleteProcesses();
            foreach (var incompleteProcess in incompleteProcesses)
            {
                if (!_runningGroups.ContainsKey(incompleteProcess.GroupId))
                {
                    Logger.Warn("Orphan Process {Id} for GroupId {GroupId} and ProcessId {ProcessId} and CorrelationId {CorrelationId}", 
                        incompleteProcess.Id, incompleteProcess.GroupId, incompleteProcess.ProcessId, incompleteProcess.CorrelationId);

                    var groupEntity = _stateManager.GetGroupEntity(incompleteProcess.GroupId);
                    if (groupEntity.IsFinished)
                    {
                        Logger.Warn("UnSynced Process {Id} and Group {GroupId}. Resetting group", incompleteProcess.Id,
                            incompleteProcess.GroupId, groupEntity);

                        groupEntity.IsFinished = false;
                        //groupEntity.IsStopped = false;//WARN
                        groupEntity.State = CompletionStatus.Pending.Name;
                        groupEntity.MarkGroupStatus(CompletionStatus.Pending, ResultStatus.Empty, "Incomplete process", _stateManager);
                    }
                    CheckSyncGroup(groupEntity);
                    //CheckProcessIdle(incompleteProcess.Id, incompleteProcess.GroupId);
                }
            }
        }

        void CheckSyncGroup(IReadWritableGroupEntity groupEntity)
        {
            List<IReadWritableProcessState>
                processes = _stateManager.GetSubmittedGroupProcesses(groupEntity.Id)
                    .ToList(); //.GetPendingGroupProcess(groupEntity.Id).ToList();
            bool hasAnomaly = false;

            if (!_runningGroups.TryGetValue(groupEntity.Id, out var @group))
            {
                @group = new SubmittedGroup(groupEntity, processes);
                _runningGroups.TryAdd(groupEntity.Id, @group);
                hasAnomaly = true;
            }
            //todo update _runningGroups and check completed/incomplete processes and tasks

            var processRunning = processes.Where(p => p.IsExecuting()).ToList();
            foreach (var running in processRunning)
            {
                var countPendingTasksForProcess = _stateManager.CountPendingTasksForProcess(running.Id);
                if (countPendingTasksForProcess == 0)
                {
                    CheckProcessIdle(running.Id, running.GroupId, true);
                    hasAnomaly = true;
                }
            }

            if (hasAnomaly)
            {
                CheckGroupHealth(@group, false);
            }
        }
        
        private Bus Bus
        {
            get { return _bus ?? (_bus = _resolver.Resolve<Bus>()); }
        }


        private void SubmitVolumeRequest(IEnumerable<IReadWritableProcessState> processes, IReadWritableGroupEntity groupDetailsGroupEntity)
        {
            try
            {
                foreach (var p in processes)
                {
                    var volumeMessage = new ProcessExecutionContext(_loggerFactory.GetProcessLogger(p.Id, p.ProcessId, p.CorrelationId), p, _cacheAside.GetProcessConfiguration(p.ProcessId), _storage, groupDetailsGroupEntity);
                    //Bus.HandleVolumeRequest(volumeMessage);
                    _volumePipeline.Invoke(volumeMessage);
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
        void CheckProcessIdle(long processId, long groupId, bool skipGroupHealthCheck=false)
        {
            if (_runningGroups.TryGetValue(groupId, out SubmittedGroup groupDetails))
            {
                var processState = groupDetails.ProcessEntities.FirstOrDefault(p => p.Id == processId);
                var processStorageState = _stateManager.GetProcessById(processId);
                if (processStorageState == null)
                {
                    Logger.Error($"ProcessId {processId} not in persistence");
                    Bus.EventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                    if (!skipGroupHealthCheck) CheckGroupHealth(groupDetails);
                    return;
                }

                if (processState == null)
                {
                    Logger.Warn($"ProcessId {processId} not in watchdog. Loading from persistence");
                    groupDetails.ProcessEntities.Add(processStorageState);
                }

                processState = processStorageState;
                groupDetails.UpdateProcessInstance(processState);

                //todo check if process tasks are finished
                //check deferred tasks
                //check process retry
                //complete process
                var isProcessCompleted = CheckProcessCompletion(processState);

                if (isProcessCompleted)
                {
                    //get child processes or other pending group processes
                    //send for volume generation
                    groupDetails.Refresh(_stateManager);

                    var nextProcesses = groupDetails.GetNextProcesses(processId).ToList();
                    if (nextProcesses.Any())
                    {
                        Logger.Trace($"Submitting next requests, {nextProcesses.Count}");
                        SubmitVolumeRequest(nextProcesses, groupDetails.GroupEntity);
                    }
                    else
                    {
                        //no further child process(es), check group
                        if(!skipGroupHealthCheck) CheckGroupHealth(groupDetails, false);
                    }
                }
            }
        }

        private void CheckGroupHealth(SubmittedGroup groupDetails, bool refresh=true)
        {
            if (refresh)
            {
                groupDetails.Refresh(_stateManager);
            }

            if (groupDetails.GroupEntity.IsFinished)
            {
                _runningGroups.TryRemove(groupDetails.GroupEntity.Id, out groupDetails);
                _eventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails, this));
            }
            else
            {
                //if (!groupDetails.GroupEntity.IsGenerated)
                //{
                //    HandleNewGroup(groupDetails.GroupEntity.Id);
                //    return;
                //}

                //any process running right now
                var groupProcesses = groupDetails.ProcessEntities;// _stateManager.GetConfiguredGroupProcessKeys(groupDetails.GroupEntity.Id).ToList();
                
                if (groupProcesses.Any(p => p.IsExecuting()))
                {
                    Logger.Trace($"Group id {groupDetails.GroupEntity.Id} is running");

                    return;
                }

                if (groupProcesses.Any(p => p.IsStopped))
                {
                    Logger.Trace($"Group id {groupDetails.GroupEntity.Id} is stopped");
                    groupDetails.GroupEntity.MarkGroupStatus(CompletionStatus.Stopped, ResultStatus.Error, "Execution stopped. Process stopped", _stateManager);
                    _eventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails, this));
                    return;
                }

                //have any process not yet started
                //var processNotGeneratedYet = groupProcesses.Where(p => p.IsVolumeGenerated == false).ToList();
                //if (processNotGeneratedYet.Count != 0)
                //{
                //    SubmitVolumeRequest(processNotGeneratedYet, groupDetails.GroupEntity);
                //}

                var processesNotStartedYet = groupProcesses.Where(p=> (p.IsVolumeGenerated==false || p.StartTime.HasValue==false) && p.Status.Id == CompletionStatus.Pending.Id && p.IsStopped==false).ToList();// && processNotGeneratedYet.Exists(g=>g.Id==p.Id)==false).ToList();
                if (processesNotStartedYet.Count == 0)
                {
                    //group processes completed i.e. no process executing and no to start

                    ResultStatus status;
                    CompletionStatus completionStatus = CompletionStatus.Finished;
                    if (groupProcesses.Any(p => p.IsStopped))
                    {
                        status = ResultStatus.Empty;
                        completionStatus= CompletionStatus.Stopped;
                    }
                    else
                    {
                        status = groupProcesses.Any(p => ResultStatus.Error.Id == p.Status.Id)
                            ? ResultStatus.Error
                            : ResultStatus.Success;
                    }
                    groupDetails.GroupEntity.MarkGroupStatus(completionStatus,  status, "Execution end", _stateManager);
                    //todo cleanup group
                    _eventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails, this));

                }
                else
                {
                    var process2Start=new List<IReadWritableProcessState>();
                    var rootProcess = groupProcesses.Where(p=>p.ParentId==null).ToList();
                    TraverseInComplete(rootProcess, process2Start, groupProcesses);
                    if (process2Start.Any())
                    {
                        SubmitVolumeRequest(process2Start, groupDetails.GroupEntity);
                    }
                    else
                    {
                        groupDetails.GroupEntity.MarkGroupStatus(CompletionStatus.Stopped, ResultStatus.Invalid, "Execution faulted. No process found", _stateManager);
                        _eventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails, this));
                    }
                }

            }
        }

        private void TraverseInComplete(List<IReadWritableProcessState> rootProcess, List<IReadWritableProcessState> process2Start,
            List<IReadWritableProcessState> groupProcesses)
        {
            var yetToStart = rootProcess.Where(p => p.IsVolumeGenerated==false && p.IsStopped == false).ToList(); //&& p.Status.Id == CompletionStatus.Pending.Id 
            var finished = rootProcess.Where(p => p.IsFinished).ToList();
            process2Start.AddRange(yetToStart);

            foreach (var processState in finished)
            {
                var childProcess = groupProcesses.Where(p => p.ParentId.HasValue && p.ParentId.Value == processState.Id).ToList();
                TraverseInComplete(childProcess, process2Start, groupProcesses);
            }

        }

        //void InvokeProcessExecuteComplete(IProcessState state, bool isFailed)
        //{er
        //    var process = _registeredProcesses.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessKey == state.ProcessKey);
        //    var context = _cacheAside.GetProcessExecutionContext(state.Id);
        //    process?.InvokeProcessCompeteEvent(context, isFailed);
        //}

        //void InvokeProcessRetry(IProcessState state)
        //{
        //    var process = _registeredProcesses.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessKey == state.ProcessKey);
        //    var context = _cacheAside.GetProcessExecutionContext(state.Id);
        //    process?.InvokeProcessRetry(context);
        //}

        bool CheckProcessCompletion(IReadWritableProcessState state)
        {
            Logger.Trace($"Process Watchdog triggered for processId {state.Id}");

            var processId = state.Id;
            var configuration = _cacheAside.GetProcessConfiguration(state.ProcessId);
            var processLogger = _loggerFactory.GetProcessLogger(processId, state.ProcessId, state.CorrelationId);
            var executionContext = (ProcessExecutionContext) _cacheAside.GetProcessExecutionContext(state.Id);
            executionContext.UpdateProcessEntity(state);


            if (!state.IsExecuting())
            {
                Logger.Trace($"Process Watchdog not executing with status IsStopped {state.IsStopped}, IsFinished {state.IsFinished} for processId {processId}");
                _eventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());//clean resources
                return true;
            }
            var timeoutMins = configuration.ProcessTimeoutMins ?? 0;

            if (!state.StartTime.HasValue)
            {
                state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                    $"Process start time is not marked for {processId}", _stateManager,
                    _registeredProcesses, executionContext, _batchEngineSubscribers, _systemLogger);

                //InvokeProcessExecuteComplete(state, true); //moved to extensions
                _eventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                return true;
            }

            var incompleteTasks = _stateManager.GetIncompleteTasksCountForProcess(processId); // .GetIncompleteTasksForProcess(processId).ToList();
            if (incompleteTasks>0)
            {
                //check timeout
                var isTimedout = timeoutMins > 0 && state.StartTime.Value.AddMinutes(timeoutMins) < DateTime.UtcNow;
                if (isTimedout)
                {
                    processLogger.Error("Timeout");
                    state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                        $"Process timeout {processId}", _stateManager,
                        _registeredProcesses, executionContext, _batchEngineSubscribers, _systemLogger);
                    //InvokeProcessExecuteComplete(state, true);//moved to extensions
                    _eventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                    return true;
                }

                //var deferredTasks = incompleteTasks.Where(d => d.DeferredCount > 0).ToList();
                Logger.Trace($"Process Watchdog skipped for processId {processId}. {incompleteTasks} incomplete tasks"); // & {deferredTasks.Count} deferred tasks
                _eventAggregator.PublishAsync(this, Constants.EventInvokeProducer); //todo publish to other nodes
                return false;
            }
            else
            {
                //check retry configured
                var erroredTasks = _stateManager.CountFailedTasksForProcess<ITaskState>(processId);

                void StopGroup(string message)
                {
                    state.IsStopped = true;
                    state.MarkProcessStatus(CompletionStatus.Stopped, ResultStatus.Error,
                        message, _stateManager, _registeredProcesses,
                        executionContext, _batchEngineSubscribers, _systemLogger);


                    if (_runningGroups.TryGetValue(state.GroupId, out SubmittedGroup grp))
                    {
                        executionContext.Logger.Error(message);
                        _groupsHandler.StopGroup(grp.GroupEntity, message);
                    }
                    else
                    {
                        message =$"ProcessStop not found in running groups => QId {state.Id}, PId {state.ProcessId} => {message}";

                        _systemLogger.Error(message);
                    }

                    _eventAggregator.Publish(this, Constants.EventProcessStop, processId.ToString());
                }

                bool CheckStopGroup()
                {
                    var stopNeeded = executionContext.Configuration.ErrorThreshold.HasValue &&
                                     executionContext.Configuration.ErrorThreshold.Value > 0;
                    if (stopNeeded && erroredTasks >= executionContext.Configuration.ErrorThreshold.Value)
                    {
                        var message =
                            $"ProcessStop QId {state.Id}, PId {state.ProcessId} meets errors threshold {executionContext.Configuration.ErrorThreshold.Value} with errors count {erroredTasks}. Going to stop";

                        StopGroup(message);
                        return true;
                    }

                    return false;
                }

                if (erroredTasks > 0)
                {
                    if (configuration.ProcessRetries.HasValue)
                    {
                        var configuredRetries = configuration.ProcessRetries.Value;
                        if (state.RetryCount < configuredRetries && configuredRetries>0)
                        {
                            bool retry = state.CanRetryProcess(executionContext, executionContext.Logger,
                                _registeredProcesses, _batchEngineSubscribers, _stateManager, _systemLogger, out bool stop, out string message);

                            if (stop)
                            {
                                StopGroup(message);
                                return true;
                            }

                            if (!retry)
                            {
                                //state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                                //    $"Retry stopped by extension {processSubscriber.GetType()}", stateManager, registeredProcesses, executionContext, batchEngineSubscribers, fLogger);

                                CheckStopGroup();
                                //_eventAggregator.Publish(this, Constants.EventProcessStop, processId.ToString());

                                return true; //process completed
                            }

                            #region commented

                            //ProcessRetryContext context =
                            //    new ProcessRetryContext(processId, state.ProcessKey, processLogger);
                            //foreach (var processSubscriber in _batchEngineSubscribers.GetProcessSubscribers())
                            //{
                            //    InvokeProcessRetry(state);
                            //    Robustness.Instance.SafeCall(() => processSubscriber.OnProcessRetry(context),
                            //        Logger);
                            //    if (context.StopFlag)
                            //    {
                            //        processLogger.Warn($"Retry stopped by extension {processSubscriber.GetType()}");

                            //        state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                            //            $"Retry stopped by extension {processSubscriber.GetType()}", _stateManager, TODO, TODO, TODO, TODO);
                            //        InvokeProcessExecuteComplete(state, true);
                            //        _eventAggregator.Publish(this, Constants.EventProcessStop,
                            //            processId.ToString());

                            //        return true;
                            //    }
                            //}

                            #endregion

                            //todo goto retry
                            processLogger.Warn($"Process going to retry with errors {erroredTasks}");
                            var retryTasksCount = state.ResetProcessTasksForRetry(_stateManager);
                            processLogger.Info($"{retryTasksCount} marked for retry");
                            _eventAggregator.PublishAsync(this, Constants.EventProcessRetry, processId.ToString());
                            return false; //not yet completed
                        }
                        else
                        {
                            //retries complete
                            
                        }
                    }
                    if (CheckStopGroup()) return true;

                    state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error, $"Process completed with errors {erroredTasks}", _stateManager,
                        _registeredProcesses, executionContext, _batchEngineSubscribers, _systemLogger);
                    //InvokeProcessExecuteComplete(state, true); //moved to extensions
                    _eventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                    return true;
                }
                else
                {
                    state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Success, "Process completed",
                        _stateManager,
                        _registeredProcesses, executionContext, _batchEngineSubscribers, _systemLogger);
                    //InvokeProcessExecuteComplete(state, false); ////moved to extensions
                    _eventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                    return true;
                }
                
            }
        }

        #region Handlers

        void HandleGroup(GroupMessage message)
        {
           if (GroupActions.Start == message.Action)
            {
                var submittedGroup = _groupsHandler.HandleStartMessage(message);

                if (submittedGroup != null)
                {
                    //lock (_syncLock)
                    //_lock.EnterReadLock();
                    try
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
                    finally
                    {
                       // _lock.ExitReadLock();
                    }
                }
                else
                {
                    //todo check group health
                }
            }
            else if (GroupActions.Stop == message.Action)
            {
                //lock (_syncLock)
                //_lock.EnterReadLock();
                try
                {
                    _groupsHandler.StopGroup(message.Group, message.Message);
                    if (!_runningGroups.TryRemove(message.Group.Id, out SubmittedGroup g))
                    {
                        Logger.Warn($"Failed to remove group from process queue. Id {message.Group.Id}");
                    }
                }
                finally
                {
                    //_lock.ExitReadLock();
                }
            }

        }

        public void Handle(IWatchDogMessage message)
        {
            _lock.EnterReadLock();
            try
            {

                if (message is ProcessInputIdleWatchDogMessage processIdleMessage)
                {
                    //lock (_syncLock)
                    //_lock.EnterReadLock();
                    try
                    {
                        CheckProcessIdle(processIdleMessage.ProcessId, processIdleMessage.GroupId);
                    }
                    finally
                    {
                        //_lock.ExitReadLock();
                    }
                }
                else if (message is GroupMessage groupMessage)
                {
                    if (_runningGroups.ContainsKey(groupMessage.GroupId))
                    {
                        Logger.Warn("Group with Id {groupId} already exist in running pool. Skipping Group message.", groupMessage.GroupId);
                    }
                    else
                    {
                        HandleGroup(groupMessage);
                    }
                }
                else if (message is ProcessRemovedWatchDogMessage processRemoved)
                {
                    Logger.Warn($"Master shouldn't receive process remove message. {processRemoved.ProcessId}");
                    _eventAggregator.Publish(this, Constants.EventProcessFinished, processRemoved.ToString()); //republish
                }
                else if (message is ProcessGroupAddedMessage groupAddedMessage)
                {
                    if (_runningGroups.ContainsKey(groupAddedMessage.GroupId))
                    {
                        Logger.Warn("Group with Id {groupId} already exist in running pool. Skipping Group added message.", groupAddedMessage.GroupId);
                    }
                    HandleNewGroup(groupAddedMessage.GroupId);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

        }

        private void HandleNewGroup(long groupId)//todo creates new entry each time
        {
            var groupEntity = _stateManager.GetGroupEntity(groupId);

            GroupMessage msg = new GroupMessage
            {
                Action = GroupActions.Start,
                Criteria = _serializer.DeserializeFromString<List<JobCriteria>>(groupEntity.Criteria),
                GroupId = groupId,
                Group = groupEntity
            };
            if (!string.IsNullOrWhiteSpace(groupEntity.Payload))
            {
                msg.IsProcessSubmission = true;
                msg.ProcessKeys = _serializer.DeserializeFromString<List<int>>(groupEntity.Payload);
            }
            else
            {
                if (groupEntity.GroupKey == 0)
                {
                    Logger.Error("Group {id} invalid with no payload and GroupKey", groupId);
                    groupEntity.MarkGroupStatus(CompletionStatus.Stopped, ResultStatus.Invalid, "Execution faulted. No process found", _stateManager);
                    return;
                }
                
            }

            msg.SetGroupEntity(groupEntity);

            HandleGroup(msg);
        }

        #endregion

        internal override void OnStart()
        {
            base.OnStart();
            _slaveMessageSubscriber = _pubSubFactory.GetSubscriber(Interrupter.Token, Logger, nameof(IWatchDogMessage));
            //_slaveMessageSubscriber.Subscribe(nameof(IWatchDogMessage), HandleSlaveMessages);

            _slaveMessageSubscriber.Subscribe(nameof(GroupMessage), s => HandleWatchDogMessage<GroupMessage>(s,
                message =>
                {
                    message.SetGroupEntity(_stateManager.GetGroupEntity(message.GroupId));
                    if (message.Action == GroupActions.Stop)
                    {
                        Logger.Info($"Received group stop from slave for Id {message.Group.Id}");
                        CheckSyncGroup(message.Group);
                        return true;
                    }

                    return false;
                }));

            
            _slaveMessageSubscriber.Subscribe(nameof(ProcessInputIdleWatchDogMessage),
                s => HandleWatchDogMessage<ProcessInputIdleWatchDogMessage>(s));

            _slaveMessageSubscriber.Subscribe<ProcessGroupAddedMessage>(s =>
            {
                _lock.EnterReadLock();
                try
                {
                    if (_runningGroups.ContainsKey(s.GroupId))
                    {
                        Logger.Trace("Watchdog New Group {groupId} already processing. dropping message.", s.GroupId);
                        return;
                    }

                    Robustness.Instance.SafeCall(() =>
                    {
                        HandleNewGroup(s.GroupId);
                    });
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            });


            _publisher = _pubSubFactory.GetPublisher(Interrupter.Token, Logger, nameof(IWatchDogMessage));

        }

        private void OnVolumeGenerated<T>(T msg) where T : TextMessage
        {
            //if (long.TryParse(msg.Parameter, out var processId))
            {
                Robustness.Instance.SafeCall(() =>
                {
                    var packet = _serializer.SerializeToString(new TriggerProducerWatchDogMessage()
                    {
                        ProcessId = msg.Parameter //processId
                    });
                    _publisher?.PublishMessage(packet, nameof(TriggerProducerWatchDogMessage));

                }, Logger);
            }
        }

        private void HandleWatchDogMessage<T>(string msg, Func<T,bool> postAction=null) where T: IWatchDogMessage
        {
            try
            {
                //_lock.EnterReadLock();

                Robustness.Instance.SafeCall(() =>
                {
                    var obj = DeserializeMessage<T>(msg);
                    var handled = postAction?.Invoke(obj);
                    if(handled==null || handled.Value==false)
                        Handle(obj);
                }, Logger, $"Error unpacking message {msg}. Error {{0}}");

            }
            finally
            {
                //_lock.ExitReadLock();
            }
        }


        //private void HandleSlaveMessages(string msg)
        //{
        //    Robustness.Instance.SafeCall(() =>
        //    {
        //        var obj = DeserializeMessage<IWatchDogMessage>(msg);
        //        Handle(obj);
        //    }, Logger, $"Error unpacking message {msg}. Error {{0}}");
            
        //}

        T DeserializeMessage<T>(string message)
        {
            var obj = _serializer.DeserializeFromString<T>(message);
            return obj;
        }

        protected override void Dispose(bool disposing)
        {
            Robustness.Instance.SafeCall(() => _slaveMessageSubscriber?.Dispose());

            Robustness.Instance.SafeCall(()=>_publisher?.Dispose());

            _slaveMessageSubscriber = null;
            
            _eventAggregator.Unsubscribe(_healthSub);
            _eventAggregator.Unsubscribe(_volumeGenSub);
            _eventAggregator.Unsubscribe(_retrySub);
            _eventAggregator.Unsubscribe(_subRem);
            _eventAggregator.Unsubscribe(_checkGroupSub);
            _eventAggregator.Unsubscribe(_subGroupRemoved);
            _eventAggregator.Unsubscribe(_processAddedSub);
            _eventAggregator.Unsubscribe(_volErrorSub);

            base.Dispose(disposing);
        }
    }

    
}