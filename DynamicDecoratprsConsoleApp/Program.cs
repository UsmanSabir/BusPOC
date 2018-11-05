using BusLib;
using BusLib.Core;
using BusLib.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicDecoratprsConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var sw = Stopwatch.StartNew();
            Parallel.For(1, 800, i =>
             {
                 BusLib.Bus.Instance.TestDecorator(new ActionCommand(HelloCommand));
             });
            sw.Stop();

            SystemFeatureToggleCommand toggleCommand = new SystemFeatureToggleCommand(nameof(ICommand), typeof(PerfMonitorHandler<ICommand>), false);
            Bus.Instance.ExecuteSystemCommand(toggleCommand);

            Console.WriteLine($"Time <<=>> {sw.ElapsedMilliseconds}");

            //BusLib.Bus.Instance.Execute(new ActionCommand(HelloCommand));

            Console.ReadLine();
        }

        private static void HelloCommand()
        {
            //Console.WriteLine("Hello Command");
            Thread.Sleep(1);
        }
    }
}
