using System;

namespace BusLib.Core
{
    public class PerfMonitorHandler<T> : FeatureCommandHandlerBase<T> where T: IMessage
    {
        public PerfMonitorHandler()
        {
            
        }        

        public override void FeatureDecoratorHandler(T message) 
        {
            //Console.WriteLine($"Performance Monitor start {DateTime.Now}");

            Handler?.Handle(message);

            //Console.WriteLine($"Performance Monitor end {DateTime.Now}");
        }

        
    }


}
