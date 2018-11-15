using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    //pipeline
    class ProcessVolumeRequestHandler: IHandler<IProcessExecutionContext>
    {
        IProcessFactory _processFactory;

        void Execute(IProcessExecutionContext context)
        {
            var processKey = context.ProcessState.ProcessKey;
            var processConfig = _processFactory.GetProcessConfiguration(context.ProcessState.ProcessKey);

            var processInstance = _processFactory.GetProcess(context.ProcessState.ProcessKey);
            IVolumeHandler volumeHandler = null; //todo initialize based on configuration or fixed?

            processInstance.HandleVolume(volumeHandler, context);
            

            //var t = typeof(GenericProcessHandler<>);
            //var gType = t.MakeGenericType(processInstance.VolumeDataType);
            //var handler = Activator.CreateInstance(gType, processInstance, context);

            ////update start date

            //if( processInstance.VolumeDataType == typeof(int))
            //{
            //    var pr = GenericProcessHandler.Create(processInstance);

            //    //send to query pipeline
            //    IBaseProcess<int> p = (IBaseProcess<int>)processInstance;
            //    Bus.Instance.QueryAction(()=> p.GetVolume(context), HandleVolume) //IEnumerable<int>
            //    ;
            //}
            //else
            //{
            //    //todo
            //}

        }

        public void Handle(IProcessExecutionContext message)
        {
            Execute(message);
        }

        //private void HandleVolume(IEnumerable<int> obj)
        //{
        //    throw new NotImplementedException();
        //}

        //class GenericProcessHandler
        //{
        //    internal static GenericProcessHandler<T> Create<T>(T it)
        //    {
        //        return new GenericProcessHandler<T>();
        //    }
        //}

        //class GenericProcessHandler<T>
        //{
        //    IBaseProcess<T> _process;
        //    IProcessExecutionContext _context;
        //    IBaseProcess<T> GetProcess(string key)
        //    {
        //        IEnumerable<T> res = _process.GetVolume(_context);
        //        //todo persist
        //        throw new NotImplementedException();
        //    }
        //}
    }

}
