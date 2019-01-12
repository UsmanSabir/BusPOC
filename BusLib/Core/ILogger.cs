using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ILogger
    {
        void Debug(string msg);

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

    internal class CorrelationLogger : ILogger
    {
        private readonly ILogger _loggerImplementation;
        private readonly Guid _correlationId;

        public CorrelationLogger(Guid correlationId, ILogger loggerImplementation) //correlationId
        {
            this._correlationId = correlationId;
            _loggerImplementation = loggerImplementation;
        }

        public void Debug(string msg)
        {
            Console.WriteLine(msg);
            _loggerImplementation.Debug(msg);
        }

        public void Trace(string message, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Trace(message, nArgs);
        }

        public void Info(string info, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Info("{CorrelationId} " + info, nArgs);
        }

        public void Warn(string warn, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Warn("{CorrelationId} " + warn, nArgs);
        }

        public void Warn(string message, Exception e, params object[] args)
        {
            var nArgs = new object[args.Length + 2];
            nArgs[0] = _correlationId;
            nArgs[1] = e.ToString(); //todo seq target doesn't support exception log
            args.CopyTo(nArgs, 2);

            _loggerImplementation.Warn("{CorrelationId} " + " {exception} " + message, nArgs);
        }

        public void Error(string error)
        {
            _loggerImplementation.Error("{CorrelationId} " + error, _correlationId);
        }

        public void Error(string error, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Error("{CorrelationId} " + error, nArgs);
        }

        public void Error(string error, Exception exception, params object[] args)
        {
            var nArgs = new object[args.Length + 2];
            nArgs[0] = _correlationId;
            nArgs[1] = exception.ToString();
            args.CopyTo(nArgs, 2);

            _loggerImplementation.Error("{CorrelationId} " + " {exception} " + error, nArgs);
        }

        public void Fatal(string error, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Fatal("{CorrelationId} " + error, nArgs);
        }

        public void Fatal(string error, Exception exception, params object[] args)
        {
            var nArgs = new object[args.Length + 2];
            nArgs[0] = _correlationId;
            nArgs[1] = exception.ToString();
            args.CopyTo(nArgs, 2);

            _loggerImplementation.Fatal("{CorrelationId} " + " {exception} " + error, nArgs);
        }
    }

    internal class CorrelationStringLogger : ILogger
    {
        private readonly ILogger _loggerImplementation;
        private readonly string _correlationId;

        public CorrelationStringLogger(string correlationId, ILogger loggerImplementation) //correlationId
        {
            this._correlationId = correlationId;
            _loggerImplementation = loggerImplementation;
        }

        public void Debug(string msg)
        {
            Console.WriteLine(msg);
            _loggerImplementation.Debug(msg);
        }

        public void Trace(string message, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Trace(message, nArgs);
        }

        public void Info(string info, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Info("{CorrelationId} " + info, nArgs);
        }

        public void Warn(string warn, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Warn("{CorrelationId} " + warn, nArgs);
        }

        public void Warn(string message, Exception e, params object[] args)
        {
            var nArgs = new object[args.Length + 2];
            nArgs[0] = _correlationId;
            nArgs[1] = e.ToString();
            args.CopyTo(nArgs, 2);

            _loggerImplementation.Warn("{CorrelationId} " + " {exception} " + message, nArgs);
        }

        public void Error(string error)
        {
            _loggerImplementation.Error("{CorrelationId} " + error, _correlationId);
        }

        public void Error(string error, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Error("{CorrelationId} " + error, nArgs);
        }

        public void Error(string error, Exception exception, params object[] args)
        {
            var nArgs = new object[args.Length + 2];
            nArgs[0] = _correlationId;
            nArgs[1] = exception.ToString();
            args.CopyTo(nArgs, 2);

            _loggerImplementation.Error("{CorrelationId} " + " {exception} " + error, nArgs);
        }

        public void Fatal(string error, params object[] args)
        {
            var nArgs = new object[args.Length + 1];
            nArgs[0] = _correlationId;
            //nArgs[1] = e;
            args.CopyTo(nArgs, 1);

            _loggerImplementation.Fatal("{CorrelationId} " + error, nArgs);
        }

        public void Fatal(string error, Exception exception, params object[] args)
        {
            var nArgs = new object[args.Length + 2];
            nArgs[0] = _correlationId;
            nArgs[1] = exception.ToString();
            args.CopyTo(nArgs, 2);

            _loggerImplementation.Fatal("{CorrelationId} " + " {exception} " + error, nArgs);
        }
    }

    internal class PrependedLogger : ILogger
    {
        private readonly ILogger _loggerImplementation;
        private readonly string _prependMessage;

        public PrependedLogger(ILogger loggerImplementation, string prependMessage)
        {
            _loggerImplementation = loggerImplementation;
            _prependMessage = prependMessage;
        }

        public void Debug(string msg)
        {
            Console.WriteLine(msg);
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

    //internal class PrependedLogger :ILogger
    //{
    //    private readonly ILogger _loggerImplementation;
    //    private readonly string _prependMessage;

    //    public PrependedLogger(ILogger loggerImplementation, string prependMessage)
    //    {
    //        _loggerImplementation = loggerImplementation;
    //        _prependMessage = prependMessage;
    //    }

    //    public void Debug(string msg)
    //    {
    //        Console.WriteLine(msg);
    //    }

    //    public void Trace(string message, params object[] args)
    //    {
    //        _loggerImplementation.Trace($"{_prependMessage} {message}", args);
    //    }

    //    public void Info(string info, params object[] args)
    //    {
    //        _loggerImplementation.Info(_prependMessage + " " + info, args);
    //    }

    //    public void Warn(string warn, params object[] args)
    //    {
    //        _loggerImplementation.Warn(_prependMessage + " " + warn, args);
    //    }

    //    public void Warn(string message, Exception e, params object[] args)
    //    {
    //        _loggerImplementation.Warn(_prependMessage + " " + message, e, args);
    //    }

    //    public void Error(string error)
    //    {
    //        _loggerImplementation.Error(_prependMessage + " " + error);
    //    }

    //    public void Error(string error, params object[] args)
    //    {
    //        _loggerImplementation.Error(_prependMessage, args);
    //    }

    //    public void Error(string error, Exception exception, params object[] args)
    //    {
    //        _loggerImplementation.Error(_prependMessage + " " + error, exception, args);
    //    }

    //    public void Fatal(string error, params object[] args)
    //    {
    //        _loggerImplementation.Fatal(_prependMessage + " " + error, args);
    //    }

    //    public void Fatal(string error, Exception exception, params object[] args)
    //    {
    //        _loggerImplementation.Fatal(_prependMessage + " " + error, exception, args);
    //    }
    //}
}
