using System.Threading;
using BusLib.Core;

namespace BusLib.PipelineFilters
{
    public class ThrottlingFilter<T>: FeatureCommandHandlerBase<T> where T : IMessage
    {
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphoreSlim;

        public ThrottlingFilter(int maxParallelRequest, ILogger logger)
        {
            _logger = logger;
            _semaphoreSlim = new SemaphoreSlim(maxParallelRequest);
        }

        public override void FeatureDecoratorHandler(T message)
        {
            _logger.Trace("Enter throttling decorator");

            try
            {
                _semaphoreSlim.Wait();

                Handler?.Handle(message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            _logger.Trace("Leaving throttling decorator");
        }
    }
}