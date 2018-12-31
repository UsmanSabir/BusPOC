//using Serilog;

//namespace BusImpl.SeriLog
//{
//    public class SeriLoggerFactory
//    {
//        public SeriLoggerFactory()
//        {
//            var log = new LoggerConfiguration()
//                .ReadFrom.AppSettings()
//                .WriteTo.Console()
//                .CreateLogger();

//            var logger = new LoggerConfiguration().CreateLogger();

//            Log.Logger = log;
//            Log.Information("The global logger has been configured");
//        }
//    }
//}