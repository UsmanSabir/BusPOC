using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Saga;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Serializers;

namespace BusLib.BatchEngineCore.Handlers
{
    internal class TaskHandler : IHandler<TaskMessage>
    {
        //BlockingCollection<ITaskMessage> _processingQueue=new BlockingCollection<ITaskMessage>();
        readonly ITaskExecutorsPool _taskExecutorsPool;

        public TaskHandler(ITaskExecutorsPool taskExecutorsPool)
        {
            _taskExecutorsPool = taskExecutorsPool;            
        }

        public void Handle(TaskMessage message)
        {
            _taskExecutorsPool.Get(message.TaskState.ProcessId).Add(message);
        }

        public void Dispose()
        {
        }
    }

    class ProcessConsumer: SafeDisposable
    {
        //private readonly int _processId;
        private readonly int _maxConsumers ;
        private readonly CancellationToken _parentToken;
        private readonly ILogger _logger;
        //private readonly IVolumeHandler _volumeHandler;
        //readonly BlockingCollection<ITaskMessage> _taskMessages=new BlockingCollection<ITaskMessage>(2);
        private readonly BlockingCollection<ITaskContext> _taskContexts;
        private CancellationTokenSource _processTokenSource;
        private readonly CancellationToken _processToken;
        private readonly ITask _handler;
        readonly SemaphoreSlim _semaphore=new SemaphoreSlim(1);
        private Task _timeoutObserverTask=null;
        //ConcurrentBag<TaskInProcess> _taskInProcess=new ConcurrentBag<TaskInProcess>();
        ConcurrentDictionary<int, TaskInProcess> _inProcessTasks=new ConcurrentDictionary<int, TaskInProcess>();
        private readonly ProcessConfiguration _processConfiguration;
        private readonly ICacheAside _cacheAside;
        private const int DefaultQueueSize = 5;
        private bool _isStateful;
        ISerializer _serializer;
        readonly int _processKey;
        private readonly IStateManager _stateManager;
        private IProcessExecutionContext _processContext;

        DateTime _lastInputTime;
        private long _tasksReceivedCount = 0;
        private readonly int _processNotificationThresholdMilliSec = 3000;


        public ProcessConsumer(CancellationToken parentToken, int processId, IStateManager stateManager,
            ILogger logger, ICacheAside cacheAside) //, ProcessConfiguration processConfiguration
        {
            _cacheAside = cacheAside;
            _lastInputTime = DateTime.UtcNow;
            
            //_processId = processId;
            _maxConsumers = ProcessConfiguration.BatchSize;
            _parentToken = parentToken;
            _logger = logger;
            //_volumeHandler = volumeHandler;
            _processKey = processId; // processConfiguration.ProcessKey;
            this._stateManager = stateManager;

            _processContext = cacheAside.GetProcessExecutionContext(processId);

            _processConfiguration =  _processContext.Configuration;

            int queueSize = _processConfiguration.QueueSize??DefaultQueueSize; //Environment.ProcessorCount
            _taskContexts = new BlockingCollection<ITaskContext>(queueSize);
            
            _processTokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            _processToken = _processTokenSource.Token;

            _handler = ApplicationTasksHandlers.Instance.GetProcessTaskHandler(_processConfiguration.ProcessKey);
            if (_handler == null)
            {
                var error = $"Task processor for processKey {_processConfiguration.ProcessKey} not found.";
                logger.Fetal(error);
                throw new FrameworkException(error);
            }            

            Type[] interfaces = _handler.GetType().GetInterfaces().Where(intrface=>intrface.IsGenericType).ToArray();

            bool isStateful = interfaces.Any(x => x.GetGenericTypeDefinition() == typeof(ITaskSaga<>)); // (typeof(ITaskSaga<>).IsAssignableFrom(handler.GetType()))

            _isStateful = isStateful;

            _serializer = ApplicationTasksHandlers.Instance.GetSerializer(_handler);
        }

        

        public void Add(TaskMessage msg)
        {
            try
            {
                _semaphore.Wait(_processToken);
                _lastInputTime = DateTime.UtcNow;

                //msg.TaskState.Status = TaskCompletionStatus.FromName(msg.TaskState.CurrentState); //todo
                TaskContext taskContext = new TaskContext(msg.OnCompleteActions)
                {
                    Logger = msg.Logger,
                    Transaction = msg.Transaction,
                    State = msg.TaskState,
                    ProcessExecutionContext = _processContext,
                };
                
                if (_isStateful)
                {
                    RestoreTaskState(taskContext);
                }
                _taskContexts.Add(taskContext, _processToken);
                msg.OnCompleteActions = null;
                Interlocked.Increment(ref _tasksReceivedCount);
            }
            catch (OperationCanceledException e)
            {
                var errMsg =
                    $"Consumer canceled for ProcessId: {msg.TaskState.ProcessId} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                _logger.Info(errMsg);
                Robustness.Instance.SafeCall(()=> msg.OnCompleteActions?.Dispose());
            }
            catch (Exception)
            {
                _logger.Error($"Error creating task context {msg.TaskState.Id}");
                Robustness.Instance.SafeCall(()=> msg.OnCompleteActions?.Dispose());
            }
            finally
            {
                _semaphore.Release();
            }
            
        }

        private void RestoreTaskState(TaskContext taskContext)
        {
            var taskStates = _stateManager.GetTaskStates(taskContext.State.Id, taskContext.State.ProcessId);
            if (taskStates != null && taskStates.Any())
            {
                string prevState = string.Empty;
                string nextState = string.Empty ;
                ConcurrentDictionary<string, string> taskStatesCollection = null;

                foreach (var pair in taskStates)
                {
                    if (pair.Key == KeyConstants.TaskPreviousState)
                    {
                        prevState = pair.Value;
                        continue;
                    }
                    else if (pair.Key == KeyConstants.TaskNextState)
                    {
                        nextState = pair.Value;
                        continue;
                    }
                    else
                    {
                        //store custom states
                        if (taskStatesCollection == null)
                            taskStatesCollection = new ConcurrentDictionary<string, string>();

                        taskStatesCollection.AddOrUpdate(pair.Key, pair.Value, (k, val) => pair.Value);
                    }

                    taskContext.ReloadTaskState(prevState, nextState, taskStatesCollection);
                }
            }
        }

        public Task Start()
        {
            Completion = StartInternal();
            //if(ProcessConfiguration.TaskTimeout>0)
                //_timeoutObserverTask = GetTimeoutObserverTask();

            return Completion;
        }

        //private Task GetTimeoutObserverTask()
        //{
            //var task = Task.Factory.StartNew(async () =>
            //{
            //    while (!_processToken.IsCancellationRequested)
            //    {

            //        try
            //        {
            //            await Task.Delay(Math.Min(TaskTimoutCheckInterval,_processNotificationThresholdMilliSec), _processToken);

            //            if (_processToken.IsCancellationRequested)
            //                return;

            //            await SweepTimedoutTasks();

            //            CheckLastInputInterval();

            //        }
            //        catch (TaskCanceledException e)
            //        {
            //            var msg =
            //                $"Timeout observer task canceled Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
            //            _logger.Info(msg);
            //        }
            //        catch (OperationCanceledException e)
            //        {
            //            var msg =
            //                $"Timeout observer task canceled Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
            //            _logger.Info(msg);
            //        }
            //        catch (Exception e)
            //        {
            //            _logger.Error($"Timeout observer got unexpected error with message {e.Message}", e);
            //        }
            //    }
            //    _logger.Trace($"Timeout observer stopped for Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))}");

            //}, _processToken);
            //return task;
        //    return  Task.CompletedTask;
        //}

        private void CheckLastInputInterval()
        {
            var count = _inProcessTasks.Count;
            if (count > 0)
            {
                _logger.Trace($"{count} tasks running, skipping input check");
                return;
            }
            var lastInp = _lastInputTime;
            var mdt = lastInp.AddMilliseconds(_processNotificationThresholdMilliSec);
            if (mdt < DateTime.UtcNow)
            {
                IProcessInputIdleWatchDogMessage message =null;//todo
                //_processContext.ProcessState.Id
                Bus.Instance.HandleWatchDogMessage(message);
            }

            //todo: die if no further input after specific interval and alerts
            //Dispose();
        }

        private async Task SweepTimedoutTasks()
        {
            if (!ProcessConfiguration.TaskTimeout.HasValue)
            {
                _logger.Trace($"Timeout observer task don't have configured task timeout from process {_processKey} key {ProcessConfiguration.ProcessKey}");
                return;
            }

            var now = DateTime.UtcNow;
            foreach (var pair in _inProcessTasks.Where(t=>t.Value.StartTime.AddSeconds(ProcessConfiguration.TaskTimeout.Value)< now))
            {
                pair.Value.TaskContext.Logger.Warn("Timeout, setting cancellation token");
                pair.Value.CancellationTokenSource.Cancel();

                try
                {
                    await Task.Delay(3000, pair.Value.TaskContext.CancellationToken);//wait for graceful shutdown
                }
                catch (TaskCanceledException e)
                {
                    //its OK
                }
                var thread = pair.Value.Thread;
                if (thread != null && thread.IsAlive)
                {
                    pair.Value.TaskContext.Logger.Error("Task timeout");
                    pair.Value.TaskContext.DashboardService?.LogError("Task timeout, sending abort command");
                    _logger.Warn("Task timeout, aborting thread");
                    pair.Value.Thread?.Abort(); //just to be safe
                }
            }
        }

        internal async Task SweepItems()
        {
            try
            {
                if (_processToken.IsCancellationRequested)
                    return;

                await SweepTimedoutTasks();

                CheckLastInputInterval();

            }
            catch (TaskCanceledException e)
            {
                var msg =
                    $"Timeout observer task canceled Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                _logger.Info(msg);
            }
            catch (OperationCanceledException e)
            {
                var msg =
                    $"Timeout observer task canceled Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                _logger.Info(msg);
            }
            catch (Exception e)
            {
                _logger.Error($"Timeout observer got unexpected error with message {e.Message}", e);
            }
            
        }

        private Task StartInternal()
        {
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    var res = Parallel.ForEach(_taskContexts.GetConsumingPartitioner(),
                        new ParallelOptions {CancellationToken = _processToken, MaxDegreeOfParallelism = _maxConsumers},
                        ExecuteTask);
                    _logger.Trace($"Consumer stopped gracefully with completion flag: {res.IsCompleted}");
                }
                catch (OperationCanceledException e)
                {
                    var msg =
                        $"Consumer canceled for Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                    _logger.Info(msg);
                }
                catch (AggregateException e)
                {
                    //probably thread abort exception
                    _logger.Warn($"Consumer got exception while processing Process: {_processKey} with message {e.GetBaseException().Message}", e);
                }
                catch (Exception e)
                {
                    _logger.Error($"Consumer stopped unexpectedly Process: {_processKey} with message {e.Message}", e);
                }
            }, _processToken);
            
            return task;
        }

        private void ExecuteTask(ITaskContext task)
        {
            TaskInProcess taskInProcess;
            
            try
            {
                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = "TaskWorker";
                }

                var taskCancellationTknSrc = CancellationTokenSource.CreateLinkedTokenSource(_processToken);
                
                taskInProcess = new TaskInProcess
                    { StartTime = DateTime.UtcNow, Thread = Thread.CurrentThread, TaskContext = task, CancellationTokenSource = taskCancellationTknSrc };

                _inProcessTasks.TryAdd(task.State.Id, taskInProcess);

                ExecuteWithRetryAsync(task).Wait();

                taskInProcess.Thread = null;
                _inProcessTasks.TryRemove(task.State.Id, out taskInProcess); //exclude from timeout check

                task.MarkTaskStatus(TaskCompletionStatus.Finished, task.Result ?? ResultStatus.Success, Constants.ReasonCompleted);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == _processToken ||
                                                       e.CancellationToken == _parentToken || e.CancellationToken == task.CancellationToken)
            {
                var msg =
                    $"Task canceled by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                task.Logger.Info(msg);
                task.MarkTaskStatus(TaskCompletionStatus.Finished, ResultStatus.Error, Constants.ReasonCancelled);
            }
            catch (ThreadInterruptedException e)
            {
                task.Logger.Error("Task interrupted", e);
                task.MarkTaskStatus(TaskCompletionStatus.Finished, ResultStatus.Error, Constants.ReasonCancelled);
            }
            catch (ThreadAbortException e)
            {
                task.Logger.Error("Task aborted", e);
                Thread.ResetAbort();
                task.MarkTaskStatus(TaskCompletionStatus.Finished, ResultStatus.Error, Constants.ReasonCancelled);                
            }
            catch (Exception e)
            {
                task.Logger.Error($"Error executing task with message {e.Message}", e);
                task.MarkTaskStatus(TaskCompletionStatus.Finished, ResultStatus.Error, e.Message);
            }
            finally
            {
                task.Logger.Trace("Task removing from processing queue");
                _inProcessTasks.TryRemove(task.State.Id, out taskInProcess);
                task.Dispose();
            }
        }

        private async Task ExecuteWithRetryAsync(ITaskContext task)
        {
            int currentRetry = 0;
            var maxRetries = ProcessConfiguration.TaskRetries ?? 0;
            for (; ; )
            {
                try
                {
                    InvokeTaskHandler(task);
                    break;
                }
                catch (Exception ex) 
                when (!(ex is ThreadAbortException) && !(ex is ThreadInterruptedException) && !(ex is OperationCanceledException)) //propogate cancel exceptions
                {                    
                    currentRetry++;

                    if (currentRetry > maxRetries || !IsTransient(task, ex))
                    {
                        throw;
                    }
                    task.Logger.Warn($"Task failed and going to retry {currentRetry} with error '{ex.Message}'", ex);

                    var waitBeforeRetry = ProcessConfiguration.RetryDelayMilli ?? 100;
                    ////Wait time increases exponentially
                    //var expTime = Math.Pow(waitBeforeRetry, currentRetry);
                    //waitBeforeRetry = (int)(expTime>int.MaxValue?int.MaxValue:expTime);

                    await Task.Delay(waitBeforeRetry , task.CancellationToken);
                }
            }
        }

        private void InvokeTaskHandler(ITaskContext task)
        {
            if (_isStateful)
            {
                HandleStatfulTaskRequest(task);
            }
            else
            {
                _handler.Handle(task, _serializer);
            }
        }

        private void HandleStatfulTaskRequest(ITaskContext task)
        {
            _handler.Handle(task, _serializer); //todo think again
        }

        private bool IsTransient(ITaskContext task, Exception ex)
        {
            var isTrans = TransientFaultHandling.IsTransient(ex);
            if (!isTrans)
            {
                task.Logger.Warn($"Exception type {ex.GetType()} is not Transient. Msg {ex.Message}");
            }
            return isTrans;
        }

        public Task Completion { get; private set; }
        internal ProcessConfiguration ProcessConfiguration { get => _processConfiguration; }

        protected override void Dispose(bool disposing)
        {
            var tokenSource = Interlocked.Exchange(ref _processTokenSource, null);
            if (tokenSource!=null && !tokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    tokenSource.Cancel();
                }
                catch (AggregateException e)
                {
                    _logger.Warn(
                        $"Error while canceling process pipeline for Process {_processKey} with msg {e.GetBaseException().Message}");
                }
                catch (Exception e)
                {
                    //should not be here
                    _logger.Error(
                        $"Error while canceling process pipeline for Process {_processKey} with msg {e.GetBaseException().Message}");
                }
            }

            foreach (var taskContext in _taskContexts)
            {
                taskContext?.Dispose();
            }
            _semaphore.Dispose();
            _taskContexts.Dispose();
            base.Dispose(disposing);

        }

        private class TaskInProcess
        {
            public Thread Thread { get; set; }

            public ITaskContext TaskContext { get; set; }

            public DateTime StartTime { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }
    }

}