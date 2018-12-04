using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.PipelineFilters
{
    public class RetryFeatureHandler<T> : FeatureCommandHandlerBase<T> where T : IMessage
    {
        private readonly int _maxRetries;
        private readonly int _delayInRetries;
        private readonly ILogger _logger;

        public RetryFeatureHandler(int maxRetries, int delayInRetries, ILogger logger)
        {
            _maxRetries = maxRetries;
            _delayInRetries = delayInRetries;
            _logger = logger;
        }

        public override void FeatureDecoratorHandler(T message)
        {
            RetryExecute(message).Wait();
        }


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
                catch (Exception ex)
                {
                    _logger.Error($"Operation Exception in Retry Handler {ex.Message}");

                    currentRetry++;

                    // Check if the exception thrown was a transient exception
                    // based on the logic in the error detection strategy.
                    // Determine whether to retry the operation, as well as how
                    // long to wait, based on the retry strategy.
                    if ((_maxRetries > 0 && currentRetry > _maxRetries) || !IsTransient(ex))
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

        //todo
        private bool IsTransient(Exception ex)
        {
            return TransientFaultHandling.IsTransient(ex);

            // Determine if the exception is transient.
            // In some cases this is as simple as checking the exception type, in other
            // cases it might be necessary to inspect other properties of the exception.
            //if (ex is OperationTransientException)
                return true; //todo

            var webException = ex as WebException;
            if (webException != null)
            {
                // If the web exception contains one of the following status values
                // it might be transient.
                return new[]
                {
                    WebExceptionStatus.ConnectionClosed,
                    WebExceptionStatus.Timeout,
                    WebExceptionStatus.RequestCanceled
                }.Contains(webException.Status);
            }

            // Additional exception checking logic goes here.
            return false;
        }

    }
}