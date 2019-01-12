using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.Handlers;
using BusLib.BatchEngineCore.Process;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;

namespace BusLib.Helper
{
    internal static class Extensions
    {
        public static int ToInt32Timeout(this TimeSpan timeout, string paramName = null)
        {
            // based on http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,959427ac16fa52fa

            var totalMilliseconds = (long) timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(paramName ?? "timeout");
            }

            return (int) totalMilliseconds;
        }

        public static void MarkAsError(this ProcessExecutionContext context, IStateManager stateManager,
            string errorMessage, IProcessRepository registeredProcesses, IBatchEngineSubscribers batchEngineSubscribers,
            IFrameworkLogger fLogger)
        {
            context.Logger.Error(errorMessage);
            context.WritableProcessState.Status = CompletionStatus.Finished;
            context.WritableProcessState.Result= ResultStatus.Error;
            context.WritableProcessState.CompleteTime=DateTime.UtcNow;
            stateManager.SaveProcess(context.WritableProcessState);

            context.WritableProcessState.TriggerProcessEvents(registeredProcesses, context, true, context.ProcessState.IsStopped, batchEngineSubscribers, fLogger, errorMessage);
        }

        #region InvokeProcessCompeteEvent(Commented)

        //public static void InvokeProcessCompeteEvent(this IBaseProcess process, IProcessExecutionContext context, bool isFailed=false)
        //{
        //    if (isFailed)
        //    {
        //        Robustness.Instance.SafeCall(() =>
        //        {
        //            process.ProcessCompleted(context);
        //        });
        //    }
        //    else
        //    {
        //        Robustness.Instance.SafeCall(() =>
        //        {
        //            process.ProcessFailed(context);
        //        });
        //    }

        //    Robustness.Instance.SafeCall(() =>
        //    {
        //        process.ProcessFinalizer(context);
        //    });
        //}

        #endregion

        public static void InvokeProcessRetry(this IBaseProcess process, IProcessRetryContext context)
        {
            {
                Robustness.Instance.SafeCall(() =>
                {
                    process.OnRetry(context);
                }, context.Logger);
            }
        }


        public static void MarkAsVolumeGenerated(this IReadWritableProcessState context, IStateManager stateManager)
        {
            //todo: send to stateManager queue
            context.Status = CompletionStatus.Processing;
            context.IsVolumeGenerated = true;
            context.GenerationCompleteTime=DateTime.UtcNow;
            stateManager.SaveProcess(context);
        }

        public static bool CanGenerateProcessVolume(this IReadWritableProcessState processState)
        {
            return CompletionStatus.Pending.Id != processState.Status.Id || processState.IsStopped;
        }

        public static bool IsExecuting(this IProcessState process)
        {
            return !(process.IsFinished || process.IsStopped) && process.StartTime.HasValue && process.IsVolumeGenerated;
        }

        public static bool IsGenerating(this IProcessState process)
        {
            return !(process.IsFinished || process.IsStopped) && process.StartTime.HasValue;
        }

        public static bool IsDone(this CompletionStatus status)
        {
            return (status.Id == CompletionStatus.Finished.Id || status.Id== CompletionStatus.Stopped.Id);
        }

        public static IReadWritableProcessState Clone(this IReadWritableProcessState process, IEntityFactory factory)
        {
            var entity = factory.CreateProcessEntity();
            entity.ProcessId = process.ProcessId;
            entity.BranchId = process.BranchId;
            entity.CompanyId = process.CompanyId;
            entity.CorrelationId = process.CorrelationId;
            entity.Criteria = process.Criteria;
            entity.GroupId = process.GroupId;
            //process.Id = entity.Id;
            entity.IsFinished = process.IsFinished;

            return entity;
            //todo
        }


        internal static string GetFormattedLogMessage(this IProcessExecutionContext context, string msg,
            Exception exception = null)
        {
            return
                $"{msg ?? string.Empty} for process QId: {context.ProcessState.Id}, Id: {context.ProcessState.ProcessId}, CorrelationId: {context.ProcessState.CorrelationId}{(exception != null ? exception.ToString() : string.Empty)}";
        }

        public static void MarkProcessStatus(this IReadWritableProcessState state, CompletionStatus completionStatus,
            ResultStatus result, string reason, IStateManager stateManager, IProcessRepository registeredProcesses,
            IProcessExecutionContext executionContext, IBatchEngineSubscribers batchEngineSubscribers,
            IFrameworkLogger fLogger)
        {
            state.Status = completionStatus;
            state.Result = result;
            if (completionStatus.IsDone())
            {
                state.CompleteTime=DateTime.UtcNow;
            }
            state.IsFinished = completionStatus.Id == CompletionStatus.Finished.Id;
            state.IsStopped = completionStatus.Id == CompletionStatus.Stopped.Id;

            stateManager.SaveProcess(state);

            state.TriggerProcessEvents(registeredProcesses, executionContext, result.Id == ResultStatus.Error.Id, state.IsStopped, 
                batchEngineSubscribers, fLogger, reason);
        }

        static void TriggerProcessEvents(this IReadWritableProcessState state, IProcessRepository registeredProcesses, IProcessExecutionContext context,
            bool isFailed, bool isStopped, IBatchEngineSubscribers batchEngineSubscribers, IFrameworkLogger fLogger, string reason)
        {
            
            var process = registeredProcesses.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessKey == context.Configuration.ProcessKey);
            //var context = _cacheAside.GetProcessExecutionContext(state.Id);
            //process?.InvokeProcessCompeteEvent(context, isFailed);

            var subscribers = batchEngineSubscribers.GetProcessSubscribers().Where(p => p.ProcessKey == context.Configuration.ProcessKey);
            //ProcessRetryContext ct =new ProcessRetryContext(context.ProcessState.Id, context.ProcessState.ProcessKey, context.Logger);
            ProcessCompleteContext ct = new ProcessCompleteContext(context.ProcessState.Id, context.ProcessState.ProcessId, false, false, context.Logger);

            //IProcessCompleteContext ctx=new proccom
            ProcessStoppedContext processStoppedContext=null;

            foreach (var subscriber in subscribers)
            {
                if (isFailed)
                {
                    Robustness.Instance.SafeCall(()=>
                    subscriber.ProcessFailed(ct)
                    , fLogger);
                }
                else if (isStopped)
                {
                    if(processStoppedContext==null)
                        processStoppedContext = new ProcessStoppedContext(state.Id, state.ProcessId, context.Configuration.ProcessKey, reason, context.Logger);

                    var stoppedContext = processStoppedContext;//access to modified closure
                    Robustness.Instance.SafeCall(() =>
                            subscriber.OnProcessStop(stoppedContext)
                        , fLogger);
                }
                else
                {
                    Robustness.Instance.SafeCall(() =>
                        subscriber.OnProcessComplete(ct)
                    , fLogger);
                }
            }

            if (isStopped)
            {
                Robustness.Instance.SafeCall(() =>
                {
                    if (processStoppedContext == null)
                        processStoppedContext = new ProcessStoppedContext(state.Id, state.ProcessId, context.Configuration.ProcessKey, reason, context.Logger);

                    process?.ProcessStopped(processStoppedContext);
                });
            }
            else if (isFailed)
            {
                Robustness.Instance.SafeCall(() =>
                {
                    process?.ProcessFailed(context);
                });
            }
            else
            {
                Robustness.Instance.SafeCall(() =>
                {
                    process?.ProcessCompleted(context);
                });
            }

            Robustness.Instance.SafeCall(() =>
            {
                process?.ProcessFinalizer(context);
            });
            
        }

        internal static bool CanRetryProcess(this IReadWritableProcessState state,
            IProcessExecutionContext executionContext,
            ILogger processLogger, IProcessRepository registeredProcesses,
            IBatchEngineSubscribers batchEngineSubscribers, IStateManager stateManager, IFrameworkLogger fLogger,
            out bool stop, out string stopMessage)
        {
            stop = false;
            stopMessage = string.Empty;

            var process = registeredProcesses.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessKey == executionContext.Configuration.ProcessKey);

            ProcessRetryContext context = new ProcessRetryContext(state.Id, state.ProcessId, processLogger, executionContext);

            process?.InvokeProcessRetry(context);

            if (context.StopFlag)
            {
                processLogger.Warn("Retry stopped by process class {processType}", process?.GetType());

                //state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                //    "Retry stopped by process class", stateManager, registeredProcesses, executionContext, batchEngineSubscribers, fLogger);
                stopMessage = "Retry stopped by process class";
                stop = true;
                return false;
            }

            foreach (var processSubscriber in batchEngineSubscribers.GetProcessSubscribers().Where(s=>s.ProcessKey== context.Configuration.ProcessKey))
            {
                Robustness.Instance.SafeCall(() => processSubscriber.OnProcessRetry(context),
                    executionContext.Logger);
                if (context.StopFlag)
                {
                    processLogger.Warn($"Retry stopped by extension {processSubscriber.GetType()}");
                    stop = true;
                    stopMessage = $"Retry stopped by extension {processSubscriber.GetType()}";
                    
                    //state.MarkProcessStatus(CompletionStatus.Finished, ResultStatus.Error,
                    //    $"Retry stopped by extension {processSubscriber.GetType()}", stateManager, registeredProcesses, executionContext, batchEngineSubscribers, fLogger);

                    //_eventAggregator.Publish(this, Constants.EventProcessStop,
                    //    processId.ToString());

                    return false;
                }
            }

            return true;
        }

        public static int ResetProcessTasksForRetry(this IReadWritableProcessState state, IStateManager steManager)
        {
            //todo: update retrycount, start time and reset error tasks
            //IStateManager steManager = Resolver.Instance.Resolve<IStateManager>();
            return steManager.MarkProcessForRetry(state);
        }
        
        internal static void ReloadTaskState(this TaskContext context, string prevState, string nextState, ConcurrentDictionary<string, string> taskStatesCollection)
        {
            context.PrevState = prevState;
            context.NextState = nextState;
            context.SetStates(taskStatesCollection);
            //context.Result
        }

        public static void MarkTaskStarted(this TaskContext context, IStateManager steManager)
        {
            //IStateManager steManager = Bus.StateManager;

            IReadWritableTaskState task = context.TaskStateWritable;
            task.StartedOn = DateTime.UtcNow;
            task.UpdatedOn = DateTime.UtcNow;

            steManager.UpdateTask(task, context.Transaction);
        }

        public static bool MarkTaskStatus(this TaskContext context, CompletionStatus completionStatus, ResultStatus result, string reason, IStateManager steManager)
        {
            try
            {
                context.Logger.Info(reason);
                //IStateManager steManager = Bus.StateManager;

                //todo: 
                IReadWritableTaskState task = context.TaskStateWritable;
                var isDone = completionStatus.IsDone();
                if (isDone)
                {
                    task.IsFinished = true;
                    task.CompletedOn=DateTime.UtcNow;
                }

                if (result.Id == ResultStatus.Error.Id)
                {
                    task.FailedCount = task.FailedCount + 1;
                }
                task.UpdatedOn=DateTime.UtcNow;
                task.Status = result;

                steManager.UpdateTask(task, context.Transaction, isDone);
                if (isDone)
                {
                    context.Logger.Info("Done");
                    //context.Transaction.Commit();
                }

                return true;
            }
            catch (FrameworkException e)
            {
                context.Logger.Error("FrameworkException while updating task status from node {node}, Complete: {complete}, Result: {result}, Reason: {reason} with error {error}", NodeSettings.Instance.Name, completionStatus.Name, result.Name, reason, e);

                return false;
            }
            catch (Exception e)
            {
                context.Logger.Error("Failed to update task status from node {node}, Complete: {complete}, Result: {result}, Reason: {reason} with error {error}", NodeSettings.Instance.Name, completionStatus.Name, result.Name, reason, e);
                return false;
            }
        }

        public static bool ReleaseTaskWithDeferFlag(this TaskContext context, IStateManager steManager)
        {
            context.Logger.Trace($"Defer task from {context.State.DeferredCount}");

            //IStateManager steManager = Bus.StateManager;

            IReadWritableTaskState task = context.TaskStateWritable;
            var nextDeferVal = task.DeferredCount + 1;

            if (nextDeferVal > 3)
            {
                var pendingTasks =
                    steManager.CountPendingTasksForProcess(context.ProcessExecutionContext.ProcessState.Id);
                var deferLimitExceeded = pendingTasks <= nextDeferVal; //(pendingTasks - 1) <= nextDeferVal;
                if (deferLimitExceeded) //skip itself
                {
                    context.Logger.Warn(
                        $"Can't further defer task because only {pendingTasks} tasks remaining and current task is deferred {task.DeferredCount} times. Check circular dependency");
                    return false;
                }
            }

            task.DeferredCount = nextDeferVal;

            task.UpdatedOn = DateTime.UtcNow;
            //task.Status = ResultStatus.Empty;
            task.IsFinished = false;
            task.IsStopped = false;
            task.NodeKey = null;

            steManager.UpdateTask(task, context.Transaction, true);
            context.Logger.Info($"Task deferred {nextDeferVal} time");
            //context.Transaction.Commit();
            return true;
        }

        internal static void PreserveNextState(this TaskContext context, string state, IStateManager steManager ,  string taskState=null)
        {
            //IStateManager steManager = Bus.StateManager;

            List<KeyValuePair<string, string>> statList=new List<KeyValuePair<string, string>>();
            statList.Add(new KeyValuePair<string, string>(KeyConstants.TaskNextState, state));
            statList.Add(new KeyValuePair<string, string>(KeyConstants.TaskPreviousState, context.NextState));
            steManager.SaveTaskStates(context.State.Id, context.State.ProcessId, statList);

            IReadWritableTaskState task = context.TaskStateWritable;
            var next = context.State.CurrentState ?? context.NextState;
            context.NextState = state; //string.Empty;
            context.PrevState = next;
            task.CurrentState = taskState ?? state;
            steManager.UpdateTask(task, context.Transaction);
        }

        public static void MarkGroupStatus(this IReadWritableGroupEntity entity, CompletionStatus completionStatus, ResultStatus result, string reason, IStateManager steManager)
        {
            //IStateManager steManager = Bus.StateManager;
            var isDone = completionStatus.IsDone();
            if (isDone)
            {
                entity.IsFinished = true;
                //entity.CompletedOn = DateTime.UtcNow;
            }
            else
            {
                entity.IsFinished = false;
            }

            //entity.UpdatedOn = DateTime.UtcNow;
            entity.State = result.Name;

            steManager.SaveGroup(entity);
            if (isDone)
            {
                //todo: 
            }
        }

        //public static void PassToTask<T, TU>(this ITask<T, TU> task, TU context,ISerializer serializer) where TU:ITaskContext
        //{
        //    task.Execute(serializer.DeserializeFromString<T>(context.State.Payload), context);
        //    //todo: 
        //    throw new NotImplementedException();
        //}

        public static bool IsFinished(this ITaskContext context)
        {
            return context.State.IsFinished; // TaskStatus.Finished.Equals(context.State.Status);
        }

        public static void AddMatrix(this HealthBlock health, string matrix, object val)
        {
            health.Matrix.Add(new KeyValuePair<string, object>(matrix, val));
        }
        public static void AddNested(this HealthBlock block, HealthBlock nestedBlock)
        {
            if (block.Details == null)
                block.Details=new List<HealthBlock>();

            block.Details.Add(nestedBlock);
        }
        public static void Add(this HealthMessage health, HealthBlock block)
        {
            health.Matrix.Add(block);
        }


        //public static IDisposable ToDisposable(this Action action)
        //{
        //    return new DisposableAction(action);
        //}

        public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> collection)
        {
            return new BlockingCollectionPartitioner<T>(collection);
        }

        #region Custom Partitioner
        //https://blogs.msdn.microsoft.com/pfxteam/2010/04/06/parallelextensionsextras-tour-4-blockingcollectionextensions/
        private class BlockingCollectionPartitioner<T> : Partitioner<T>
        {
            private readonly BlockingCollection<T> _collection;

            internal BlockingCollectionPartitioner(BlockingCollection<T> collection)
            {
                if (collection == null)
                    throw new ArgumentNullException(nameof(collection));

                _collection = collection;
            }

            public override bool SupportsDynamicPartitions => true;


            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
            {
                if (partitionCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(partitionCount));

                var dynamicPartitioner = GetDynamicPartitions();

                return Enumerable.Range(0, partitionCount)
                    .Select(_ => dynamicPartitioner.GetEnumerator()).ToArray();
            }

            public override IEnumerable<T> GetDynamicPartitions()
            {
                return _collection.GetConsumingEnumerable();
            }
        }

        #endregion
    }
}