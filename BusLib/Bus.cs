using BusLib.Core;
using BusLib.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Handlers;
using BusLib.PipelineFilters;

namespace BusLib
{
    public class Bus
    {
        static Bus _instance;
        public static Bus Instance => _instance ?? (_instance = new Bus());

        private Pipeline<ITaskMessage> _taskProcessorPipeline;

        Pipeline<ICommand> _commandPipeLine;
        private ILogger _logger;
        private readonly ITaskExecutorRepository _taskExecutorsRepo;

        public Bus()
        {
            HookExceptionEvents();
            _logger = LoggerFactory.GetSystemLogger();

            BuildCommandHandlerPipeline();
            _taskProcessorPipeline = GetTaskProcessorPipeLine();
        }

        #region Unhandled Exceptions

        private void HookExceptionEvents()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Fetal($"Unhandled task exception message {e.Exception?.GetBaseException()?.ToString() ?? string.Empty}", e.Exception);
            e.SetObserved();
            ((AggregateException)e.Exception).Handle(ex =>
            {
                _logger.Fetal($"Task unhandled exception type: {ex.ToString()}", ex);
                return true;
            });
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fetal($"Unhandled application error with terminating flag {e.IsTerminating} and message {e.ExceptionObject??string.Empty}");
            
        }

        #endregion

        private Pipeline<ITaskMessage> GetTaskProcessorPipeLine()
        {
            Pipeline<ITaskMessage> tasksPipeline=new TaskProcessingPipeline(LoggerFactory.GetSystemLogger(), _taskExecutorsRepo);
            
            throw new NotImplementedException();
        }

        private void BuildCommandHandlerPipeline()
        {
            _commandPipeLine = new Pipeline<ICommand>(new CommandHandler());
            _commandPipeLine.RegisterFeatureDecorator(new PerfMonitorHandler<ICommand>());
        }
                
        public void Execute(ICommand message)
        {
            _commandPipeLine.Invoke(message);
        }

        internal void QueryAction<T>(Func<T> action, Action<T> onResult)
        {
            
        }

        public void ExecuteSystemCommand(ISystemCommand message)
        {
            if(message.PipeLineKey == nameof(ICommand))
            {
                _commandPipeLine.HandleSystemCommand(message);
            }
        }


        public void TestDecorator(ICommand command)
        {
            Execute(command);

            //_decorator.Disable();

            Execute(command);

            //_decorator.Enable();
            Execute(command);
            Execute(command);

        }
    }
}
