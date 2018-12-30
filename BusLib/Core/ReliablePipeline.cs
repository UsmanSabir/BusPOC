using System;
using System.Threading;
using BusLib.PipelineFilters;

namespace BusLib.Core
{
    public class ReliablePipeline<T> : Pipeline<T> where T : IMessage
    {
        public ReliablePipeline(IHandler<T> handler, string name, ILogger logger, int maxRetries, int delayInRetries,
            CancellationToken token, Action<Exception> errorAction=null) : base(handler)
        {
            RegisterFeatureDecorator(new CircuitBreakerFeatureHandler<T>(name, logger, token));
            RegisterFeatureDecorator(new RetryFeatureHandler<T>(maxRetries, delayInRetries, logger, token, errorAction));
        }
    }
}