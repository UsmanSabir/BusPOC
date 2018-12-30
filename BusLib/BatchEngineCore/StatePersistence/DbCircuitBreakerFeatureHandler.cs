using System;
using System.Threading;
using BusLib.Core;
using BusLib.Helper.CB;
using BusLib.Infrastructure;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore.StatePersistence
{
    public class DbCircuitBreakerFeatureHandler: CircuitBreakerFeatureHandler<DbAction>
    {
        private readonly string _name;
        private readonly ILogger _logger;
        private readonly CancellationToken _token;

        public DbCircuitBreakerFeatureHandler(string name, ILogger logger, CancellationToken token, int delayInRetries = 1000) : base(name, logger, token, delayInRetries)
        {
            _name = name;
            _logger = logger;
            _token = token;
        }

        public override void FeatureDecoratorHandler(DbAction message)
        {
            //base.FeatureDecoratorHandler(message);
            while (true)
            {
                try
                {
                    _token.ThrowIfCancellationRequested();

                    if (message.Action== DbActions.Error)
                    {
                        //todo what now
                    }
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
    }
}