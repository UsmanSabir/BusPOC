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

        public DatabasePipeline(ILogger logger, CancellationToken token) : base(new DbActionCommandHandler())
        {
            this._handler = new DbCircuitBreakerFeatureHandler(nameof(DatabasePipeline), logger, token);
            RegisterFeatureDecorator(_handler);
            RegisterFeatureDecorator(new RetryFeatureHandler<DbAction>(9, 500, logger, token));
        }
    }
}