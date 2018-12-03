using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Core;

namespace BusLib.PipelineFilters
{
    public class ConsumerFilter<T>: FeatureCommandHandlerBase<T> where T : IMessage
    {
        BlockingCollection<T> _collection=new BlockingCollection<T>(new ConcurrentQueue<T>());
        private CancellationToken _token;
        private ILogger _logger;
        private string _name;
        private Task _task;
        private readonly CancellationTokenSource _linkedTokenSource;

        public ConsumerFilter(CancellationToken token, ILogger logger, string name)
        {
            _token = token;
            _logger = logger;
            _name = name??string.Empty;

            _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            _task = Task.Run(()=>Consume(), _linkedTokenSource.Token);
        }

        void Consume()
        {
            try
            {
                while (!_token.IsCancellationRequested)
                {
                    var t = _collection.Take(_token);
                    Handler?.Handle(t);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.Warn($"Consumer {_name} cancelled while taking");
            }
        }

        public override void FeatureDecoratorHandler(T message)
        {
            try
            {
                _collection.Add(message, _token);
            }
            catch (OperationCanceledException)
            {
                _logger.Warn($"Consumer {_name} add cancelled");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(_linkedTokenSource.IsCancellationRequested)
                _linkedTokenSource.Cancel();

            try
            {
                _task?.Wait();
            }
            catch (Exception e)
            {
                _logger.Trace($"Consumer {_name} got exception while stopping.{e.Message}");
            }

            base.Dispose(disposing);
        }
    }
}