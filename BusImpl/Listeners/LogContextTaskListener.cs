using BusLib.BatchEngineCore;
using BusLib.Core;

namespace BusImpl.Listeners
{
    public class LogContextTaskListener:ITaskListener
    {
        private readonly ILogger _logger;

        public LogContextTaskListener(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger();
        }

        public void BeforeExecute(ITaskContext taskContext)
        {
        }

        public void AfterExecute(ITaskContext taskContext)
        {
            
        }

        public string Name { get; } = nameof(LogContextTaskListener);
    }
}