using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.Handlers;
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

        public static void MarkAsError(this ProcessExecutionContext context, IStateManager stateManager, string errorMessage)
        {
            context.Logger.Error(errorMessage);
            context.WritableProcessState.Status = CompletionStatus.Finished;
            context.WritableProcessState.Result= ResultStatus.Error;
            context.WritableProcessState.CompleteTime=DateTime.UtcNow;
            stateManager.SaveProcess(context.WritableProcessState);
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
            entity.ProcessKey = process.ProcessKey;
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
                $"{msg ?? string.Empty} for process Id: {context.ProcessState.Id}, Key: {context.ProcessState.ProcessKey}, CorrelationId: {context.ProcessState.CorrelationId}{(exception != null ? exception.ToString() : string.Empty)}";
        }

        public static void MarkProcessStatus(this IReadWritableProcessState state, CompletionStatus completionStatus, ResultStatus result, string reason, IStateManager stateManager)
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
        }

        public static int RetryProcess(this IReadWritableProcessState state)
        {
            //todo: update retrycount, start time and reset error tasks
            IStateManager steManager = Bus.StateManager;
            return steManager.MarkProcessForRetry(state);
        }
        
        internal static void ReloadTaskState(this TaskContext context, string prevState, string nextState, ConcurrentDictionary<string, string> taskStatesCollection)
        {
            context.PrevState = prevState;
            context.NextState = nextState;
            context.SetStates(taskStatesCollection);
            //context.Result
        }

        public static void MarkTaskStarted(this TaskContext context)
        {
            IStateManager steManager = Bus.StateManager;

            IReadWritableTaskState task = context.TaskStateWritable;
            task.StartedOn = DateTime.UtcNow;
            task.UpdatedOn = DateTime.UtcNow;

            steManager.UpdateTask(task, context.Transaction);
        }

        public static void MarkTaskStatus(this TaskContext context, CompletionStatus completionStatus, ResultStatus result, string reason)
        {
            context.Logger.Info(reason);
            IStateManager steManager = Bus.StateManager;

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

            steManager.UpdateTask(task, context.Transaction);
            if (isDone)
            {
                context.Logger.Info("Done");
                context.Transaction.Commit();
            }
        }

        public static bool ReleaseTaskWithDeferFlag(this TaskContext context)
        {
            context.Logger.Trace($"Defer task from {context.State.DeferredCount}");

            IStateManager steManager = Bus.StateManager;

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

            steManager.UpdateTask(task, context.Transaction);
            context.Logger.Info($"Task deferred {nextDeferVal} time");
            context.Transaction.Commit();
            return true;
        }

        internal static void PreserveNextState(this TaskContext context, string state, string taskState=null)
        {
            IStateManager steManager = Bus.StateManager;

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

        public static void MarkGroupStatus(this IReadWritableGroupEntity entity, CompletionStatus completionStatus, ResultStatus result, string reason)
        {
            IStateManager steManager = Bus.StateManager;
            var isDone = completionStatus.IsDone();
            if (isDone)
            {
                entity.IsFinished = true;
                //entity.CompletedOn = DateTime.UtcNow;
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