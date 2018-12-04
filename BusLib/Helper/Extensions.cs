using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Groups;
using BusLib.BatchEngineCore.Handlers;
using BusLib.Core;

namespace BusLib.Helper
{
    public static class Extensions
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

        public static void MarkAsError(this IProcessExecutionContext context, string errorMessage)
        {
            context.Logger.Error(errorMessage);
            //todo: send to stateManager queue
        }
        
        public static void MarkAsVolumeGenerated(this IProcessState context)
        {
            //todo: send to stateManager queue
        }

        public static bool IsExecuting(this IProcessState process)
        {
            return !(process.IsFinished || process.IsStopped) && process.StartTime.HasValue;
        }
        

        internal static string GetFormattedLogMessage(this IProcessExecutionContext context, string msg,
            Exception exception = null)
        {
            return
                $"{msg ?? string.Empty} for process Id: {context.ProcessState.Id}, Key: {context.ProcessState.ProcessKey}, CorrelationId: {context.ProcessState.CorrelationId}{(exception != null ? exception.ToString() : string.Empty)}";
        }

        public static void MarkProcessStatus(this IProcessState state, TaskCompletionStatus completionStatus, ResultStatus result, string reason)
        {
            //todo: 
            throw new NotImplementedException();
        }

        public static void RetryProcess(this IProcessState state)
        {
            //todo: update retrycount, start time and reset error tasks
            throw new NotImplementedException();
        }

        

        internal static void ReloadTaskState(this TaskContext context, string prevState, string nextState, ConcurrentDictionary<string, string> taskStatesCollection)
        {
            context.PrevState = prevState;
            context.NextState = nextState;
            context.SetStates(taskStatesCollection);
            //context.Result
        }

        public static void MarkTaskStatus(this ITaskContext context, TaskCompletionStatus completionStatus, ResultStatus result, string reason)
        {
            //todo: 
            throw new NotImplementedException();
        }

        public static void MarkGroupStatus(this IGroupEntity context, TaskCompletionStatus completionStatus, ResultStatus result, string reason)
        {
            //todo: 
            throw new NotImplementedException();
        }

        public static void PassToTask<T, TU>(this ITask<T, TU> task, TU context,ISerializer serializer) where TU:ITaskContext
        {
            task.Execute(serializer.DeserializeFromString<T>(context.State.Payload), context);
            //todo: 
            throw new NotImplementedException();
        }

        public static bool IsFinished(this ITaskContext context)
        {
            return context.State.IsFinished; // TaskStatus.Finished.Equals(context.State.Status);
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
            private BlockingCollection<T> _collection;

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