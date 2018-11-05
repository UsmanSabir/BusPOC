using BusLib.Core;
using BusLib.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib
{
    public class Bus
    {
        static Bus _instance;
        public static Bus Instance
        {
            get
            {
                return _instance ?? (_instance = new Bus());
            }
        }


        Pipeline<ICommand> _commandPipeLine;

        public Bus()
        {
            BuildCommandHandlerPipeline();
            
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
