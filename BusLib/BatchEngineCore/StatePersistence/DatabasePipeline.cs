using System.Threading;
using BusLib.BatchEngineCore.StatePersistence;
using BusLib.Core;
using BusLib.Infrastructure;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore.Volume
{
    internal class DatabasePipeline: Pipeline<Infrastructure.DbAction>
    {
        private readonly DbCircuitBreakerFeatureHandler _handler;

        public DatabasePipeline(ILogger logger, CancellationToken token, int? maxDbCalls ) : base(new DbActionCommandHandler())
        {
            if (maxDbCalls.HasValue && maxDbCalls.Value>0)
            {
                RegisterFeatureDecorator(new TimeBasedThrottlingFilter<DbAction>(maxDbCalls.Value, token, logger));
            }
            this._handler = new DbCircuitBreakerFeatureHandler(nameof(DatabasePipeline), logger, token, 9, 500);
            RegisterFeatureDecorator(_handler);
            //RegisterFeatureDecorator(new RetryFeatureHandler<DbAction>(9, 500, logger, token));
        }
    }
}