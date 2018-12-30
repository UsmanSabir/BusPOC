using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Exceptions;
using BusLib.Core;
using BusLib.Helper;
using BusLib.Helper.CB;

namespace BusLib.PipelineFilters
{
    public class CircuitBreakerFeatureHandler<T> : FeatureCommandHandlerBase<T> where T : IMessage
    {
        private readonly string _name;
        private readonly ILogger _logger;
        protected readonly CircuitBreaker _breaker;
        private CancellationToken _token;

        public CircuitBreakerFeatureHandler(string name, ILogger logger, CancellationToken token, int delayInRetries=1000)
        {
            _name = name;
            _logger = logger;
            _token = token;
            _breaker=new CircuitBreaker(name, delayInRetries);
        }

        public override void FeatureDecoratorHandler(T message)
        {
            while (true)
            {
                try
                {
                    _token.ThrowIfCancellationRequested();

                    _breaker.Invoke(
                        // This is the operation we want to execute.
                        () => Handler?.Handle(message)
                    );
                    
                    _logger.Trace($"CircuitBreaker '{_name}' request completed");
                    return;
                }
                catch (CircuitBreakerOpenException e)
                {
                    _logger.Warn($"CircuitBreaker '{_name}' Open with internal exception {e.InnerException?.Message}");
                }
                catch (Exception e)
                {
                    //if (IsTransient(e))
                    //{
                    //    _logger.Warn($"CircuitBreaker '{_name}' retrying with transient error {e.Message}");
                    //}
                    //else
                    {
                        _logger.Warn($"CircuitBreaker '{_name}' got error {e.Message}", e);
                        throw;
                    }
                }
            }
        }

        //public override TRes FeatureDecoratorQuery<TRes>(T message)
        //{
        //    if (!IsEnabled)
        //        return Handler.Query<TRes>(message);

        //    while (true)
        //    {
        //        try
        //        {
        //            TRes res = default;
        //            _breaker.Invoke(
        //                // This is the operation we want to execute.
        //                () =>
        //                {
        //                    res = Handler.Query<TRes>(message);
        //                });

        //            _logger.Trace($"CircuitBreaker '{_name}' request completed");
        //            return res;
        //        }
        //        catch (CircuitBreakerOpenException e)
        //        {
        //            _logger.Warn($"CircuitBreaker '{_name}' Open with internal exception {e.InnerException?.Message}");
        //        }
        //        catch (Exception e)
        //        {
        //            //if (IsTransient(e))
        //            //{
        //            //    _logger.Warn($"CircuitBreaker '{_name}' retrying with transient error {e.Message}");
        //            //}
        //            //else
        //            {
        //                _logger.Warn($"CircuitBreaker '{_name}' got error {e.Message}", e);
        //                throw;
        //            }
        //        }
        //    }
        //}
    }
}