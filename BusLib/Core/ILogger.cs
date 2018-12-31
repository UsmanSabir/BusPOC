using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ILogger
    {
        void Trace(string message, params object[] args);

        void Info(string info, params object[] args);

        void Warn(string warn, params object[] args);
        void Warn(string message, Exception e, params object[] args);
        void Error(string error);
        void Error(string error, params object[] args);
        void Error(string error, Exception exception, params object[] args);

        void Fatal(string error, params object[] args);
        void Fatal(string error, Exception exception, params object[] args);
    }

    public interface IFrameworkLogger : ILogger
    {
    }

    internal class PrependedLogger :ILogger
    {
        private readonly ILogger _loggerImplementation;
        private readonly string _prependMessage;

        public PrependedLogger(ILogger loggerImplementation, string prependMessage)
        {
            _loggerImplementation = loggerImplementation;
            _prependMessage = prependMessage;
        }

        public void Trace(string message, params object[] args)
        {
            _loggerImplementation.Trace($"{_prependMessage} {message}", args);
        }

        public void Info(string info, params object[] args)
        {
            _loggerImplementation.Info(_prependMessage + " " + info, args);
        }

        public void Warn(string warn, params object[] args)
        {
            _loggerImplementation.Warn(_prependMessage + " " + warn, args);
        }

        public void Warn(string message, Exception e, params object[] args)
        {
            _loggerImplementation.Warn(_prependMessage + " " + message, e, args);
        }

        public void Error(string error)
        {
            _loggerImplementation.Error(_prependMessage + " " + error);
        }

        public void Error(string error, params object[] args)
        {
            _loggerImplementation.Error(_prependMessage, args);
        }

        public void Error(string error, Exception exception, params object[] args)
        {
            _loggerImplementation.Error(_prependMessage + " " + error, exception, args);
        }

        public void Fatal(string error, params object[] args)
        {
            _loggerImplementation.Fatal(_prependMessage + " " + error, args);
        }

        public void Fatal(string error, Exception exception, params object[] args)
        {
            _loggerImplementation.Fatal(_prependMessage + " " + error, exception, args);
        }
    }
}
