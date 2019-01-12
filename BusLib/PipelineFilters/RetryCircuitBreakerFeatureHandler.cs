using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Helper.CB;

namespace BusLib.PipelineFilters
{
    public class RetryCircuitBreakerFeatureHandler<T> : FeatureCommandHandlerBase<T> where T : IMessage
    {
        private readonly int? _maxRetries;
        private readonly int _delayInRetries;
        private readonly ILogger _logger;
        private readonly Action<Exception> _errorAction;
        private CancellationToken _token;
        protected readonly CircuitBreaker _breaker;
        private string _name;

        public RetryCircuitBreakerFeatureHandler(
            string name, int? maxRetries, ILogger logger, CancellationToken token, int delayInRetries = 1000,
            Action<Exception> errorAction=null)
        {
            _maxRetries = maxRetries;
            _delayInRetries = delayInRetries;
            _logger = logger;
            _token = token;
            _errorAction = errorAction;
            _name = name;
            _breaker = new CircuitBreaker(name, delayInRetries, logger);
        }

        public override void FeatureDecoratorHandler(T message)
        {
            RetryExecute(message);//.Wait();
            //var task = RetryExecuteQuery(message);
            //var res = task.Result;//ignore
        }

        //public override TU FeatureDecoratorQuery(T message)
        //{
        //    var task = RetryExecuteQuery(message);
        //    return task.Result;
        //}


        void RetryExecute(T message)
        {
            int currentRetry = 0;
            //IRetryableMessage<T> retryable=new RetryableMessage<T>(message){IsRetry = false};
            for (;;)
            {
                try
                {
                    _token.ThrowIfCancellationRequested();

                    _breaker.Invoke(
                        // This is the operation we want to execute.
                        () => Handler?.Handle(message), currentRetry>0
                    );

                    
                    // Return or break.
                    break;
                }
                catch (CircuitBreakerOpenException e)
                {
                    _token.ThrowIfCancellationRequested();

                    _logger.Warn($"CircuitBreaker '{_name}' Open with internal exception {e.InnerException?.Message}");
                    //Thread.Sleep(Math.Max(_delayInRetries, 100));
                    //await Task.Delay(Math.Min(_delayInRetries, 100));
                    Task.Delay(Math.Min(_delayInRetries, 100)).Wait(_token);
                    //ignore and retry. this is not a genuine error
                }
                catch (AggregateException aex) when (aex.InnerExceptions.Any(r => r is FrameworkException))
                {
                    var fex = aex.InnerExceptions.First(r => r is FrameworkException);
                    //if (fex != null)
                    {
                        ExceptionDispatchInfo.Capture(fex).Throw();
                    }
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
                try
                {
                    Task.Delay(_delayInRetries, _token).Wait(_token);
                }
                catch (TaskCanceledException)
                {
                    //cancelled
                }

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