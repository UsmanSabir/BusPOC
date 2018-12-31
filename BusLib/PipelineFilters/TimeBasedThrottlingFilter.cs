using System;
using System.Threading;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.PipelineFilters
{
    public class TimeBasedThrottlingFilter<T>: FeatureCommandHandlerBase<T> where T : IMessage
    {
        private readonly CancellationToken _token;
        private readonly ILogger _logger;
        private readonly ThrottlerGate _throttlerGate;

        public TimeBasedThrottlingFilter(int maxOccurence, CancellationToken token, ILogger logger, int timeSpatMiliSecs=1000) 
        {
            _token = token;
            _logger = logger;
            _throttlerGate=new ThrottlerGate(maxOccurence, TimeSpan.FromMilliseconds(timeSpatMiliSecs), token);
        }

        public override void FeatureDecoratorHandler(T message)
        {
            _logger.Trace("Enter throttling decorator");

            try
            {
                _throttlerGate.WaitToProceed();
                //todo cancellation token 

                _logger.Trace("Throttling decorator, passing to handler");
                Handler?.Handle(message);
            }
            finally
            {
                _logger.Trace("Leaving throttling decorator");
            }
        }
    }
}