using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Saga;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Handlers
{
    internal class TaskHandler : IHandler<ITaskMessage>
    {
        BlockingCollection<ITaskMessage> _processingQueue=new BlockingCollection<ITaskMessage>();
        private int _maxParallel;

        public TaskHandler()
        {
            _maxParallel = Environment.ProcessorCount * 2;
        }

        public TaskHandler(int maxParallel)
        {
            _maxParallel = maxParallel;
        }

        public void Handle(ITaskMessage message)
        {
            _processingQueue.Add(message);
        }

        void ProcessMessage()
        {
            Parallel.ForEach(_processingQueue, new ParallelOptions{MaxDegreeOfParallelism = _maxParallel}, ProcessTask);
        }

        private void ProcessTask(ITaskMessage task)
        {
            throw new System.NotImplementedException();
        }
    }

    class ProcessConsumer: SafeDisposable
    {
        private readonly int _processId;
        private readonly int _maxConsumers ;
        private readonly CancellationToken _parentToken;
        private readonly ILogger _logger;
        private readonly IVolumeHandler _volumeHandler;
        //readonly BlockingCollection<ITaskMessage> _taskMessages=new BlockingCollection<ITaskMessage>(2);
        private readonly BlockingCollection<ITaskContext> _taskContexts;
        private CancellationTokenSource _processTokenSource;
        private readonly CancellationToken _processToken;
        readonly SemaphoreSlim _semaphore=new SemaphoreSlim(1);
        private Task _timeoutObserverTask=null;
        //ConcurrentBag<TaskInProcess> _taskInProcess=new ConcurrentBag<TaskInProcess>();
        ConcurrentDictionary<int, TaskInProcess> _inProcessTasks=new ConcurrentDictionary<int, TaskInProcess>();
        private ProcessConfiguration _processConfiguration;
        private const int DefaultQueueSize = 5;
        private bool _isStateful;

        public ProcessConsumer(int processId, CancellationToken parentToken, ILogger logger, ProcessConfiguration processConfiguration, IVolumeHandler volumeHandler)
        {
            _processId = processId;
            _maxConsumers = _processConfiguration.BatchSize;
            _parentToken = parentToken;
            _logger = logger;
            _processConfiguration = processConfiguration;
            _volumeHandler = volumeHandler;
            int queueSize = processConfiguration.QueueSize??DefaultQueueSize; //Environment.ProcessorCount
            _taskContexts = new BlockingCollection<ITaskContext>(queueSize);
            
            _processTokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            _processToken = _processTokenSource.Token;

            var handler = ApplicationTasksHandlers.Instance.GetProcessTaskHandler(processConfiguration.ProcessKey);
            if (handler == null)
            {
                var error = $"Task processor for processKey {processConfiguration.ProcessKey} not found.";
                logger.Fetal(error);
                throw new MissingMemberException(error);
            }

            bool isStateful = handler.GetType().GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(ITaskSaga<>)); //// (typeof(ITaskSaga<>).IsAssignableFrom(handler.GetType()))
            _isStateful = isStateful;
            
        }

        

        public void Add(ITaskMessage msg)
        {
            try
            {
                _semaphore.Wait(_processToken);

                ITaskContext taskContext = null; //todo
                if (_isStateful)
                {
                    RestoreTaskState(taskContext);
                }
                _taskContexts.Add(taskContext, _processToken);
            }
            catch (OperationCanceledException e)
            {
                var errMsg =
                    $"Consumer canceled for ProcessId: {_processId} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                _logger.Info(errMsg);
                Robustness.Instance.SafeCall(msg.OnCompleteActions.Dispose);
            }
            catch (Exception)
            {
                Robustness.Instance.SafeCall(msg.OnCompleteActions.Dispose);
            }
            finally
            {
                _semaphore.Release();
            }
            
        }

        private void RestoreTaskState(ITaskContext taskContext)
        {
            var taskStates = _volumeHandler.GetTaskStates(taskContext.State.Id, taskContext.State.ProcessId);
            if (taskStates != null && taskStates.Any())
            {
                string prevState;
                string nextState;
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
                        ConcurrentDictionary<string, string> taskStatesCollection = null;
                        if (taskStatesCollection == null)
                            taskStatesCollection = new ConcurrentDictionary<string, string>();

                        taskStatesCollection.AddOrUpdate(pair.Key, pair.Value, (k, val) => pair.Value);
                    }


                }
            }
        }

        public Task Start()
        {
            Completion = StartInternal();
            _timeoutObserverTask = GetTimeoutObserverTask();
            return Completion;
        }

        private Task GetTimeoutObserverTask()
        {
            var task = Task.Factory.StartNew(async () =>
            {
                while (!_processToken.IsCancellationRequested)
                {

                    try
                    {
                        await Task.Delay(5000, _processToken);

                        if (_processToken.IsCancellationRequested)
                            return;

                        SweepTimedoutTasks();
                        
                    }
                    catch (TaskCanceledException e)
                    {
                        var msg =
                            $"Timeout observer task canceled ProcessId: {_processId} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                        _logger.Info(msg);
                    }
                    catch (OperationCanceledException e)
                    {
                        var msg =
                            $"Timeout observer task canceled ProcessId: {_processId} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                        _logger.Info(msg);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Timeout observer got unexpected error with message {e.Message}", e);
                    }
                }
                _logger.Trace($"Timeout observer stopped for ProcessId: {_processId} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))}");

            }, _processToken);
            return task;
        }

        private async Task SweepTimedoutTasks()
        {
            if (!_processConfiguration.TaskTimeout.HasValue)
            {
                _logger.Warn($"Timeout observer task don't have configured task timeout from process id {_processId} key {_processConfiguration.ProcessKey}");
                return;
            }

            var now = DateTime.UtcNow;
            foreach (var pair in _inProcessTasks.Where(t=>t.Value.StartTime.AddSeconds(_processConfiguration.TaskTimeout.Value)< now))
            {
                pair.Value.TaskContext.Logger.Warn("Timeout, setting cancellation token");
                pair.Value.CancellationTokenSource.Cancel();

                await Task.Delay(3000, pair.Value.TaskContext.CancellationToken);//wait for graceful shutdown

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

        private Task StartInternal()
        {
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    var res = Parallel.ForEach(_taskContexts.GetConsumingPartitioner(),
                        new ParallelOptions {CancellationToken = _processToken, MaxDegreeOfParallelism = _maxConsumers},
                        ProcessTask);
                    _logger.Trace($"Consumer stopped gracefully with completion flag: {res.IsCompleted}");
                }
                catch (OperationCanceledException e)
                {
                    var msg =
                        $"Consumer canceled for ProcessId: {_processId} by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                    _logger.Info(msg);
                }
                catch (AggregateException e)
                {
                    //probably thread abort exception
                    _logger.Warn($"Consumer got exception while processing ProcessId: {_processId} with message {e.GetBaseException().Message}", e);
                }
                catch (Exception e)
                {
                    _logger.Error($"Consumer stopped unexpectedly ProcessId: {_processId} with message {e.Message}", e);
                }
            }, _processToken);
            
            return task;
        }

        private void ProcessTask(ITaskContext task)
        {
            TaskInProcess taskInProcess;
            
            try
            {
                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = "TaskWorker";
                }
                var taskCancellationTknSrc = CancellationTokenSource.CreateLinkedTokenSource(_processToken);
                //task.CancellationToken = taskCancellationTknSrc.Token;

                taskInProcess = new TaskInProcess
                    { StartTime = DateTime.UtcNow, Thread = Thread.CurrentThread, TaskContext = task, CancellationTokenSource = taskCancellationTknSrc };

                _inProcessTasks.TryAdd(task.State.Id, taskInProcess);

                //todo

                taskInProcess.Thread = null;
                _inProcessTasks.TryRemove(task.State.Id, out taskInProcess); //exclude from timeout check

            }
            catch (OperationCanceledException e) when (e.CancellationToken == _processToken ||
                                                       e.CancellationToken == _parentToken)
            {
                var msg =
                    $"Task canceled by token {(_processToken.IsCancellationRequested ? "ProcessToken" : (_parentToken.IsCancellationRequested ? "ParentToken" : "NoToken"))} with msg {e.Message}";
                task.Logger.Info(msg);
            }
            catch (ThreadInterruptedException e)
            {
                task.Logger.Error("Task interrupted", e);
            }
            catch (ThreadAbortException e)
            {
                task.Logger.Error("Task aborted", e);

                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                task.Logger.Error($"Error executing task with message {e.Message}", e);
            }
            finally
            {
                task.Logger.Trace("Task removed");
                _inProcessTasks.TryRemove(task.State.Id, out taskInProcess);
                task.Dispose();
                
            }
        }

        public Task Completion { get; private set; }

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
                        $"Error while canceling process pipeline for id {_processId} with msg {e.GetBaseException().Message}");
                }
                catch (Exception e)
                {
                    //should not be here
                    _logger.Error(
                        $"Error while canceling process pipeline for id {_processId} with msg {e.GetBaseException().Message}");
                }
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