using System.Threading;
using BusLib.Core;
using BusLib.Infrastructure;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore.Volume
{
    internal class StatePersistencePipeline: ReliablePipeline<Infrastructure.ActionCommand>
    {
        public StatePersistencePipeline(ILogger logger, CancellationToken token) : base(new ActionCommandHandler(), "StatePersistencePipeline", logger, 9, 500, token)
        {
            RegisterFeatureDecorator(new RetryAlertFeatureHandler<ActionCommand>(50000, 5000, logger, token));
        }
    }
}