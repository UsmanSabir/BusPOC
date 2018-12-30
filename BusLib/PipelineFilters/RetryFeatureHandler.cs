﻿using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.PipelineFilters
{
    public class RetryFeatureHandler<T> : FeatureCommandHandlerBase<T> where T : IMessage
    {
        private readonly int? _maxRetries;
        private readonly int _delayInRetries;
        private readonly ILogger _logger;
        private readonly Action<Exception> _errorAction;
        private CancellationToken _token;

        public RetryFeatureHandler(int? maxRetries, int delayInRetries, ILogger logger, CancellationToken token, Action<Exception> errorAction=null)
        {
            _maxRetries = maxRetries;
            _delayInRetries = delayInRetries;
            _logger = logger;
            _token = token;
            _errorAction = errorAction;
        }

        public override void FeatureDecoratorHandler(T message)
        {
            RetryExecute(message).Wait();
            //var task = RetryExecuteQuery(message);
            //var res = task.Result;//ignore
        }

        //public override TU FeatureDecoratorQuery(T message)
        //{
        //    var task = RetryExecuteQuery(message);
        //    return task.Result;
        //}


        async Task RetryExecute(T message)
        {
            int currentRetry = 0;

            for (;;)
            {
                try
                {
                    Handler?.Handle(message);
                    //await TransientOperationAsync();

                    // Return or break.
                    break;
                }
                catch (Exception ex) when (!(ex is FrameworkException))
                {
                    _token.ThrowIfCancellationRequested();
                        
                    
                    _logger.Error($"Operation Exception in Retry Handler {ex.Message}");

                    Robustness.Instance.SafeCall(()=>_errorAction?.Invoke(ex), _logger, "Error invoking error action in RetryHandler {0}");

                    if (_maxRetries.HasValue) //infinite
                    {
                        currentRetry++;
                    }
                    

                    // Check if the exception thrown was a transient exception
                    // based on the logic in the error detection strategy.
                    // Determine whether to retry the operation, as well as how
                    // long to wait, based on the retry strategy.
                    if (_maxRetries.HasValue && ((_maxRetries > 0 && currentRetry > _maxRetries) || !TransientFaultHandling.IsTransient(ex)))
                    {
                        // If this isn't a transient error or we shouldn't retry, 
                        // rethrow the exception.
                        throw;
                    }
                }

                // Wait to retry the operation.
                // Consider calculating an exponential delay here and
                // using a strategy best suited for the operation and fault.
                await Task.Delay(_delayInRetries);

            }
        }

        #region Query commented

        //async Task<TU> RetryExecuteQuery(T message)
        //    {
        //        int currentRetry = 0;

        //        for (; ; )
        //        {
        //            try
        //            {
        //                return Handler.Query(message);
        //                //await TransientOperationAsync();

                        
        //            }
        //            catch (Exception ex) when (!(ex is FrameworkException))
        //            {
        //                _logger.Error($"Operation Exception in Retry Handler {ex.Message}");

        //                currentRetry++;

        //                // Check if the exception thrown was a transient exception
        //                // based on the logic in the error detection strategy.
        //                // Determine whether to retry the operation, as well as how
        //                // long to wait, based on the retry strategy.
        //                if ((_maxRetries > 0 && currentRetry > _maxRetries) || !IsTransient(ex))
        //                {
        //                    // If this isn't a transient error or we shouldn't retry, 
        //                    // rethrow the exception.
        //                    throw;
        //                }
        //            }

        //            // Wait to retry the operation.
        //            // Consider calculating an exponential delay here and
        //            // using a strategy best suited for the operation and fault.
        //            await Task.Delay(_delayInRetries);

        //        }

        //    }
        

        #endregion


    }
}