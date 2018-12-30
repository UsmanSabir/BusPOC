using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.PipelineFilters
{
    public class DuplicateCheckFilter<T, TU> : FeatureCommandHandlerBase<T> where TU : IComparable where T : IMessage
    {
        readonly List<TU> _processedIds = new List<TU>();
        private readonly Func<T, TU> _idExtractorFunc;
        private readonly string _name;
        private readonly ILogger _logger;
        readonly ReaderWriterLockSlim _readerWriter = new ReaderWriterLockSlim();

        public DuplicateCheckFilter(Func<T, TU> idExtractorFunc, string name, ILogger logger)
        {
            _idExtractorFunc = idExtractorFunc;
            _name = name;
            _logger = logger;
        }


        internal void Cleanup(IEnumerable<TU> items)
        {
            _readerWriter.EnterWriteLock();
            try
            {
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var id = item;// _idExtractorFunc(item);
                        _processedIds.Remove(id);
                    }
                }
            }
            finally
            {
                _readerWriter.ExitWriteLock();
            }

            
        }

        public override void FeatureDecoratorHandler(T message)
        {
            TU id;
            try
            {
                _readerWriter.EnterReadLock();
                id = _idExtractorFunc(message);
            }
            finally
            {
                _readerWriter.ExitReadLock();
            }

            var exist = _processedIds.Any(r => EqualityComparer<TU>.Default.Equals(id, r));
            if (exist)
            {
                _logger.Warn($"{_name} filter found duplicate entry for id {id}. Discarding");
                return;
            }


            try
            {
                _readerWriter.EnterWriteLock();
                _processedIds.Add(id);
            }
            finally
            {
                _readerWriter.ExitWriteLock();
            }

            try
            {
                Handler?.Handle(message);
            }
            catch (Exception e)
            {
                Robustness.Instance.SafeCall(() =>
                {
                    _readerWriter.EnterWriteLock();
                    try
                    {
                        _processedIds.Remove(id);
                    }
                    finally
                    {
                        _readerWriter.ExitWriteLock();
                    }
                });

                throw;
            }
        }

        
    }
}