using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore;

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

        public static void MarkAsVolumeGenerated(this IProcessExecutionContext context)
        {
            //todo: send to stateManager queue
        }

        internal static string GetFormatedLogMessage(this IProcessExecutionContext context, string msg,
            Exception exception = null)
        {
            return
                $"{msg ?? string.Empty} for process Id: {context.ProcessState.Id}, Key: {context.ProcessState.ProcessKey}, CorrelationId: {context.ProcessState.CorrelationId}{(exception != null ? exception.ToString() : string.Empty)}";
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