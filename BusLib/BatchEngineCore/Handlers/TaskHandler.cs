using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using BusLib.ProcessLocks;
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
            _taskExecutorsPool.Get(message.TaskState.ProcessId, message.ProcessContext.ProcessState.ProcessKey, message.ProcessContext.ProcessState.GroupId).Add(message);
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
        private readonly IFrameworkLogger _systemLogger;
        //private readonly IVolumeHandler _volumeHandler;
        //readonly BlockingCollection<ITaskMessage> _taskMessages=new BlockingCollection<ITaskMessage>(2);
        private readonly BlockingCollection<TaskContext> _taskContextsQueue;
        private CancellationTokenSource _processTokenSource;
        private readonly CancellationToken _processToken;
        private readonly ITask _handler;
        //readonly SemaphoreSlim _semaphore=new SemaphoreSlim(1); handled through queue size
        private readonly Task _timeoutObserverTask=null;
        //ConcurrentBag<TaskInProcess> _taskInProcess=new ConcurrentBag<TaskInProcess>();
        ConcurrentDictionary<long, TaskInProcess> _inProcessTasks=new ConcurrentDictionary<long, TaskInProcess>();
        private readonly IProcessConfiguration _processConfiguration;
        private readonly ICacheAside _cacheAside;
        private const int DefaultQueueSize = 5;
        private readonly bool _isStateful;
        readonly ISerializer _serializer;
        readonly int _processKey;
        readonly long _processId;
        private readonly long _groupId;
        private readonly IStateManager _stateManager;
        private readonly IProcessExecutionContext _processContext;

        DateTime _lastInputTime;
        private long _tasksReceivedCount = 0;
        private readonly int _processNotificationThresholdMilliSec = 3000;
        internal Action SweeperAction { get; set; }

        public ProcessConsumer(CancellationToken parentToken, int processKey, long processId,
            long groupId,
            IStateManager stateManager,
            ILogger logger, ICacheAside cacheAside, ITask taskHandler, ISerializer serializer) //, ProcessConfiguration processConfiguration
        {
            _systemLogger = LoggerFactory.GetSystemLogger();
            _cacheAside = cacheAside;

            _processContext = cacheAside.GetProcessExecutionContext(processId);
            _processConfiguration = _processContext.Configuration;

            _lastInputTime = DateTime.UtcNow;
            
            //_processId = processId;
            _maxConsumers = ProcessConfiguration.BatchSize;
            _parentToken = parentToken;
            _logger = logger;
            //_volumeHandler = volumeHandler;
            _processId = processId; // processConfiguration.ProcessKey;
            _groupId = groupId;
            _processKey = processKey;
            _stateManager = stateManager;

            
            int queueSize = _processConfiguration.QueueSize??DefaultQueueSize; //Environment.ProcessorCount
            queueSize = queueSize <= 0 ? 1 : queueSize;
            _taskContextsQueue = new BlockingCollection<TaskContext>(queueSize);
            
            _processTokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            _processToken = _processTokenSource.Token;

            _handler = taskHandler;// ApplicationTasksHandlers.Instance.GetProcessTaskHandler(_processConfiguration.ProcessKey);
            if (_handler == null)
            {
                var error = $"Task processor for processKey {_processConfiguration.ProcessKey} not found.";
                logger.Fetal(error);
                throw new FrameworkException(error);
            }            

            Type[] interfaces = _handler.GetType().GetInterfaces().Where(t=>t.IsGenericType).ToArray();

            bool isStateful = interfaces.Any(x => x.GetGenericTypeDefinition() == typeof(ITaskSaga<>)); // (typeof(ITaskSaga<>).IsAssignableFrom(handler.GetType()))

            _isStateful = isStateful;

            _serializer = serializer; // ApplicationTasksHandlers.Instance.GetSerializer(_handler);
        }

        

        public void Add(TaskMessage msg)
        {
            try
            {
                //_semaphore.Wait(_processToken); handled through queue size
                _lastInputTime = DateTime.UtcNow;

                //msg.TaskState.Status = TaskCompletionStatus.FromName(msg.TaskState.CurrentState); //todo
                TaskContext taskContext = new TaskContext(_isStateful, msg.OnCompleteActions, msg.TaskStateWritable)
                {
                    Logger = msg.Logger,
                    Transaction = msg.Transaction,
                    ProcessExecutionContext = _processContext,
                };
                
                if (_isStateful)
                {
                    RestoreTaskState(taskContext);
                }
                _taskContextsQueue.Add(taskContext, _processToken);
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
                //_semaphore.Release();
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
                }
                taskContext.ReloadTaskState(prevState, nextState, taskStatesCollection);
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

            //            await SweepTimeoutTasks();

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

        void CheckInvokeCleaner()
        {
            var count = _inProcessTasks.Count;
            if (count > 0)
            {
                _logger.Trace($"CheckInvokeCleaner: {count} tasks running, skipping input check");
                return;
            }

            Action a = SweeperAction;
            a?.Invoke();
        }

        private void CheckLastInputInterval()
        {
            var count = _inProcessTasks.Count;
            if (count > 0)
            {
                _logger.Trace($"{count} tasks running, skipping input check");
                return;
            }

            //lock (this)
            {
                var lastInp = _lastInputTime;
                var mdt = lastInp.AddMilliseconds(_processNotificationThresholdMilliSec);
                if (mdt < DateTime.UtcNow)
                {
                    IProcessInputIdleWatchDogMessage message =new ProcessInputIdleWatchDogMessage(_processContext.ProcessState.GroupId, _processContext.ProcessState.Id, _processContext.ProcessState.ProcessKey, this);
                
                    Bus.Instance.HandleWatchDogMessage(message);
                }
            }

            //todo: die if no further input after specific interval and alerts
            //Dispose();
        }

        private async Task SweepTimeoutTasks()
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
                catch (TaskCanceledException)
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

                await SweepTimeoutTasks();

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
            //_systemLogger.Trace($"Is ThreadPool Thread {Thread.CurrentThread.IsThreadPoolThread}");
            var task = Task.Factory.StartNew( Run , _processToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            return task;
        }

        void Run()
        {
            //_systemLogger.Trace($"Is ThreadPoolThread {Thread.CurrentThread.IsThreadPoolThread}");
            try
            {
                var res = Parallel.ForEach(_taskContextsQueue.GetConsumingPartitioner(),
                    new ParallelOptions { CancellationToken = _processToken, MaxDegreeOfParallelism = _maxConsumers },
                    ExecuteTask);
                //_systemLogger.Trace($"Is ThreadPool Thread {Thread.CurrentThread.IsThreadPoolThread}");
                _logger.Trace($"Consumer process {_processId} stopped gracefully with completion flag: {res.IsCompleted}");
            }
            catch (TaskCanceledException e)
            {
                var msg =
                    $"Consumer canceled for Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                _systemLogger.Info(msg);
            }
            catch (OperationCanceledException e)
            {
                var msg =
                    $"Consumer canceled for Process: {_processKey} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                _systemLogger.Info(msg);
            }
            catch (AggregateException e)
            {
                //probably thread abort exception
                _systemLogger.Warn(
                    $"Consumer got exception while processing Process: {_processKey} with message {e.InnerException?.Message ?? string.Empty}",
                    e);
            }
            catch (Exception e)
            {
                _logger.Error($"Consumer stopped unexpectedly Process: {_processKey} with message {e.Message}", e);
            }
            //finally
            //{
            //    _systemLogger.Trace($"Consumer final for Process: {_processKey}");
            //}
        }

        private void ExecuteTask(TaskContext task)
        {
            _logger.Trace($"Is ThreadPool Threaed {Thread.CurrentThread.IsThreadPoolThread}");

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

                if(!task.IsDeferred)
                    task.MarkTaskStatus(CompletionStatus.Finished, task.Result ?? ResultStatus.Success, Constants.ReasonCompleted);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == _processToken ||
                                                       e.CancellationToken == _parentToken || e.CancellationToken == task.CancellationToken)
            {
                var msg =
                    $"Task canceled by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                task.Logger.Info(msg);
                task.MarkTaskStatus(CompletionStatus.Finished, ResultStatus.Error, Constants.ReasonCancelled);
            }
            catch (ThreadInterruptedException e)
            {
                task.Logger.Error("Task interrupted", e);
                task.MarkTaskStatus(CompletionStatus.Finished, ResultStatus.Error, Constants.ReasonCancelled);
            }
            catch (ThreadAbortException e)
            {
                task.Logger.Error("Task aborted", e);
                Thread.ResetAbort();
                task.MarkTaskStatus(CompletionStatus.Finished, ResultStatus.Error, Constants.ReasonCancelled);                
            }
            catch (Exception e)
            {
                task.Logger.Error($"Error executing task with message {e.Message}", e);
                task.MarkTaskStatus(CompletionStatus.Finished, ResultStatus.Error, e.Message);
            }
            finally
            {
                task.Logger.Trace("Task removing from processing queue");
                _inProcessTasks.TryRemove(task.State.Id, out taskInProcess);
                bool isDeferred = task.IsDeferred;
                task.Dispose();
                //CheckLastInputInterval();
                if(!isDeferred)
                    CheckInvokeCleaner();
            }
        }

        private async Task ExecuteWithRetryAsync(TaskContext context)
        {
            int currentRetry = 0;
            var maxRetries = ProcessConfiguration.TaskRetries ?? 0;
            for (; ; )
            {
                try
                {
                    InvokeTaskHandler(context);
                    break;
                }
                catch (Exception ex) 
                when (!(ex is ThreadAbortException) && !(ex is ThreadInterruptedException) && !(ex is OperationCanceledException)) //propogate cancel exceptions
                {                    
                    currentRetry++;
                    //task.State.FailedCount //todo increment
                    if (currentRetry > maxRetries || !IsTransient(context, ex))
                    {
                        throw;
                    }
                    context.Logger.Warn($"Task failed and going to retry {currentRetry} with error '{ex.Message}'", ex);

                    var waitBeforeRetry = ProcessConfiguration.RetryDelayMilli ?? 100;
                    ////Wait time increases exponentially
                    //var expTime = Math.Pow(waitBeforeRetry, currentRetry);
                    //waitBeforeRetry = (int)(expTime>int.MaxValue?int.MaxValue:expTime);

                    await Task.Delay(waitBeforeRetry , context.CancellationToken);
                }
            }
        }

        private void InvokeTaskHandler(TaskContext task)
        {
            if (_isStateful)
            {
                HandleStatefulTaskRequest(task);
            }
            else
            {
                _handler.Handle(task, _serializer);
            }
        }

        private void HandleStatefulTaskRequest(TaskContext task)
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
        internal IProcessConfiguration ProcessConfiguration { get => _processConfiguration; }

        private bool _stopped = false;
        internal void Stop()
        {
            if(_stopped)
                return;

            _taskContextsQueue.CompleteAdding();

            var tokenSource = Interlocked.Exchange(ref _processTokenSource, null);
            if (tokenSource != null && !tokenSource.Token.IsCancellationRequested)
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

            _stopped = true;
        }

        protected override void Dispose(bool disposing)
        {
            Stop();

            foreach (var taskContext in _taskContextsQueue)
            {
                taskContext?.Dispose();
            }
            //_semaphore.Dispose();
            _taskContextsQueue.Dispose();

            base.Dispose(disposing);
        }

        private class TaskInProcess
        {
            public Thread Thread { get; set; }

            public ITaskContext TaskContext { get; set; }

            public DateTime StartTime { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }


        public void UpdateHealthStatus(HealthBlock taskExecutorHealth)
        {
            var executing = _inProcessTasks.Count;
            var queueSize = _taskContextsQueue.Count;

            taskExecutorHealth.AddMatrix("Executing", executing);
            taskExecutorHealth.AddMatrix("QueueSize", queueSize);
            taskExecutorHealth.AddMatrix("IsStopped", _stopped);

            if (executing + queueSize == 0)
                CheckInvokeCleaner();
            
        }
    }

}