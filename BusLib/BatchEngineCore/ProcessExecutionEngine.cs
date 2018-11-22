using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Helper;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore
{
    //pipeline
    class ProcessVolumeRequestHandler: IHandler<IProcessExecutionContext>
    {
        List<IBaseProcess> _registeredProcesses=new List<IBaseProcess>(); // todo: scan all assemblies for implemented processes
        private ICacheAside _cacheAside;
        ManualResetEvent _pauseHandler=new ManualResetEvent(true);
        private IFrameworkLogger _logger;

        readonly ConcurrentDictionary<int, Pipeline<IProcessExecutionContext>> _processPipelines = new ConcurrentDictionary<int, Pipeline<IProcessExecutionContext>>();
        private int _delayInRetries = 3000; //todo

        void Execute(IProcessExecutionContext context)
        {
            _logger.Trace(context.GetFormatedLogMessage("Volume request received"));
            _pauseHandler.WaitOne();

            var processKey = context.ProcessState.ProcessKey;
            var pipeline = GetPipeline(processKey);

            if (pipeline == null)
            {
                var error = context.GetFormatedLogMessage("Volume handler not found");
                _logger.Error(error);
                context.MarkAsError(error);
                return;
            }

            _logger.Trace(context.GetFormatedLogMessage("Volume request sending to pipeline"));

            try
            {
                pipeline.Invoke(context);
            }
            catch (Exception e)
            {
                var error = context.GetFormatedLogMessage("Error generating volume", e);
                _logger.Error(error);
                context.MarkAsError(error);
            }

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
        
        Pipeline<IProcessExecutionContext> GetPipeline(int processKey)
        {
            var pipeLine = _processPipelines.GetOrAdd(processKey, key =>
            {
                var process = _registeredProcesses.FirstOrDefault(p => p.ProcessKey == processKey);
                if (process == null)
                    return null;
                var processConfig = _cacheAside.GetProcessConfiguration(key);
                var maxVolumeRetries = processConfig.MaxVolumeRetries;

                Pipeline<IProcessExecutionContext> pipeline=new Pipeline<IProcessExecutionContext>(new VolumeGenerator(process));
                
                if (maxVolumeRetries != 0) // -1 means unlimited retries
                {
                    pipeline.RegisterFeatureDecorator(
                        new RetryFeatureHandler<IProcessExecutionContext>(maxVolumeRetries,
                            _delayInRetries, _logger));
                }

                return pipeline;
            });
            return pipeLine;
        }

        public void Handle(IProcessExecutionContext message)
        {
            Execute(message);
        }

        class VolumeGenerator:IHandler<IProcessExecutionContext>
        {
            private readonly IBaseProcess _process;

            public VolumeGenerator(IBaseProcess process)
            {
                _process = process;
            }

            public void Handle(IProcessExecutionContext message)
            {
                message.Logger.Trace("Volume handler ");

                IVolumeHandler volumeHandler = null; //todo initialize based on configuration or fixed?



                _process.HandleVolume(volumeHandler, message);
                message.Logger.Info("Volume generated successfully ");

                if (_process is IHasSupportingData processWithSupportingData)
                {
                    message.Logger.Trace("Initializing Supporting Data ");

                    //initializer process wise cache data
                    Robustness.Instance.SafeCallWithRetry(() => processWithSupportingData.InitializeSupportingData(message), 3, 1000, message.Logger);
                }

                message.Logger.Trace("Marking process as Generated");
                message.MarkAsVolumeGenerated();
                
            }
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
