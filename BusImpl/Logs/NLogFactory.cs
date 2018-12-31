using BusImpl.SeriLog;
using BusLib.BatchEngineCore;
using NLog;
using ILogger = BusLib.Core.ILogger;

namespace BusImpl.Logs
{
    public class NLogFactory: ILoggerFactory
    {
        
        public static Logger Create(string name="logger")
        {
            var logger = LogManager.GetLogger(name);
            return logger;
        }

        public ILogger CreateLogger(string name = "logger")
        {
            //var logger = LogManager.GetLogger(name);
            return new NLogWrapper();
            //return logger;
        }
    }
}