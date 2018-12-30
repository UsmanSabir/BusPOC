using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.PubSub;
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

        private readonly TinyMessageSubscriptionToken _subRem;

        private IDistributedMessageSubscriber _slaveMessageSubscriber;

        private readonly ISerializer _serializer;

        private IDistributedMessagePublisher _publisher;

        private readonly TinyMessageSubscriptionToken _volumeGenSub;

        private readonly TinyMessageSubscriptionToken _retrySub;

        private readonly TinyMessageSubscriptionToken _healthSub;
        //object _syncLock=new object();

        public ProcessWatchDog(ILogger logger, IStateManager stateManager,
            IBatchEngineSubscribers batchEngineSubscribers, ICacheAside cacheAside,
            ISerializersFactory serializersFactory, IEntityFactory entityFactory, IEventAggregator eventAggregator,
            IProcessDataStorage storage, IPubSubFactory pubSubFactory) : base("WatchDog", logger)
        {
            _stateManager = stateManager;
            _batchEngineSubscribers = batchEngineSubscribers;
            _cacheAside = cacheAside;
            _eventAggregator = eventAggregator;
            _storage = storage;
            _pubSubFactory = pubSubFactory;
            _groupsHandler = new GroupsHandler(logger, batchEngineSubscribers, stateManager, serializersFactory, entityFactory);
            Interval=TimeSpan.FromSeconds(30);

            eventAggregator.Subscribe4Broadcast<ProcessGroupRemovedMessage>(RemoveGroup);
            _subRem = eventAggregator.Subscribe<TextMessage>(ProcessRemoved, Constants.EventProcessFinished);

            _serializer = SerializersFactory.Instance.GetSerializer(typeof(GroupMessage));

            this._volumeGenSub = eventAggregator.Subscribe<TextMessage>(OnVolumeGenerated, Constants.EventProcessVolumeGenerated);
            _retrySub = Bus.Instance.EventAggregator.Subscribe<TextMessage>(OnVolumeGenerated, Constants.EventProcessRetry);
            _healthSub = Bus.Instance.EventAggregator.Subscribe4Broadcast<HealthMessage>(PublishHealth);
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
                CheckSyncGroup(groupEntity);
            }

            var orphanGroups = _runningGroups.Values.Where(r=> groups.All(g => g.Id != r.GroupEntity.Id)).ToList();
            foreach (var orphan in orphanGroups)
            {
                CheckGroupHealth(orphan);
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


        private void SubmitVolumeRequest(IEnumerable<IReadWritableProcessState> processes, IReadWritableGroupEntity groupDetailsGroupEntity)
        {
            try
            {
                foreach (var p in processes)
                {
                    var volumeMessage = new ProcessExecutionContext(LoggerFactory.GetProcessLogger(p.Id, p.ProcessKey), p, _cacheAside.GetProcessConfiguration(p.ProcessKey), _storage);
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
        void CheckProcessIdle(long processId, long groupId, bool skipGroupHealthCheck=false)
        {
            if (_runningGroups.TryGetValue(groupId, out SubmittedGroup groupDetails))
            {
                var processState = groupDetails.ProcessEntities.FirstOrDefault(p => p.Id == processId);
                var processStorageState = _stateManager.GetProcessById(processId);
                if (processStorageState == null)
                {
                    Logger.Error($"ProcessId {processId} not in persistence");
                    Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
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
                Bus.Instance.EventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails, this));
            }
            else
            {
                //any process running right now
                var groupProcesses = groupDetails.ProcessEntities;// _stateManager.GetConfiguredGroupProcessKeys(groupDetails.GroupEntity.Id).ToList();
                
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
                    groupDetails.GroupEntity.MarkGroupStatus(completionStatus,  status, "Execution end");
                    //todo cleanup group
                    Bus.Instance.EventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails, this));

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
                        groupDetails.GroupEntity.MarkGroupStatus(CompletionStatus.Stopped, ResultStatus.Invalid, "Execution faulted. No process found");
                        Bus.Instance.EventAggregator.Broadcast(new ProcessGroupRemovedMessage(groupDetails, this));
                    }
                }

            }
        }

        private void TraverseInComplete(List<IReadWritableProcessState> rootProcess, List<IReadWritableProcessState> process2Start,
            List<IReadWritableProcessState> groupProcesses)
        {
            var yetToStart = rootProcess.Where(p => p.Status.Id == ResultStatus.Empty.Id && p.IsStopped == false).ToList();
            var finished = rootProcess.Where(p => p.IsFinished).ToList();
            process2Start.AddRange(yetToStart);

            foreach (var processState in finished)
            {
                var childProcess = groupProcesses.Where(p => p.ParentId.HasValue && p.ParentId.Value == processState.Id).ToList();
                TraverseInComplete(childProcess, process2Start, groupProcesses);
            }

        }


        bool CheckProcessCompletion(IReadWritableProcessState state)
        {
            Logger.Trace($"Process Watchdog triggered for processId {state.Id}");

            var processId = state.Id;
            var configuration = _cacheAside.GetProcessConfiguration(state.ProcessKey);
            var processLogger = LoggerFactory.GetProcessLogger(processId, state.ProcessKey);

            if (!state.IsExecuting())
            {
                Logger.Trace($"Process Watchdog not executing with statue IsStopped {state.IsStopped}, IsFinished {state.IsFinished} for processId {processId}");
                Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                return true;
            }
            var timeoutMins = configuration.ProcessTimeoutMins ?? 0;

            if (!state.StartTime.HasValue)
            {
                state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                    $"Process start time is not marked for {processId}", _stateManager);
                Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                return true;
            }

            var incompleteTasks = _stateManager.GetIncompleteTasksForProcess(processId).ToList();
            if (incompleteTasks.Any())
            {
                //check timeout
                var isTimedout = timeoutMins > 0 && state.StartTime.Value.AddMinutes(timeoutMins) < DateTime.UtcNow;
                if (isTimedout)
                {
                    processLogger.Error("Timeout");
                    state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                        $"Process timeout {processId}", _stateManager);
                    Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
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

                                    state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                                        $"Retry stopped by extension {processSubscriber.GetType()}", _stateManager);
                                    Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessStop,
                                        processId.ToString());

                                    return true;
                                }
                            }

                            //todo goto retry
                            processLogger.Warn($"Process going to retry with errors {erroredTasks}");
                            var retryTasksCount = state.RetryProcess();
                            processLogger.Info($"{retryTasksCount} marked for retry");
                            Bus.Instance.EventAggregator.PublishAsync(this, Constants.EventProcessRetry, processId.ToString());
                            return false; //not yet completed
                        }
                    }

                    state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error, $"Process completed with errors {erroredTasks}", _stateManager);
                    Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
                    return true;
                }
                else
                {
                    state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Success, "Process completed", _stateManager);
                    Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, processId.ToString());
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
                    _lock.EnterReadLock();
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
                        _lock.ExitReadLock();
                    }
                }
            }
            else if (GroupActions.Stop == message.Action)
            {
                //lock (_syncLock)
                _lock.EnterReadLock();
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
                    _lock.ExitReadLock();
                }
            }

        }

        public void Handle(IWatchDogMessage message)
        {
            if (message is ProcessInputIdleWatchDogMessage processIdleMessage)
            {
                //lock (_syncLock)
                _lock.EnterReadLock();
                try
                {
                    CheckProcessIdle(processIdleMessage.ProcessId, processIdleMessage.GroupId);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            else if (message is GroupMessage groupMessage)
            {
                HandleGroup(groupMessage);
            }
            else if (message is ProcessRemovedWatchDogMessage processRemoved)
            {
                Logger.Warn($"Master shouldn't receive process remove message. {processRemoved.ProcessId}");
                Bus.Instance.EventAggregator.Publish(this, Constants.EventProcessFinished, processRemoved.ToString()); //republish
            }
        }
    

        #endregion

        internal override void OnStart()
        {
            base.OnStart();
            _slaveMessageSubscriber = Bus.PubSubFactory.GetSubscriber(Interrupter.Token, Logger, nameof(IWatchDogMessage));
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

            _publisher = Bus.PubSubFactory.GetPublisher(Interrupter.Token, Logger, nameof(IWatchDogMessage));

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
            Robustness.Instance.SafeCall(() =>
            {
                var obj = DeserializeMessage<T>(msg);
                var handled = postAction?.Invoke(obj);
                if(handled==null || handled.Value==false)
                    Handle(obj);
            }, Logger, $"Error unpacking message {msg}. Error {{0}}");

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
            base.Dispose(disposing);
        }
    }

    
}