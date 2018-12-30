using BusLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Process;
using BusLib.Helper;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore.Volume
{
    internal class ProcessVolumePipeline:Pipeline<ProcessExecutionContext>
    {
        public ProcessVolumePipeline(CancellationToken token, ILogger logger, IStateManager stateManager,
            ICacheAside cacheAside, IProcessRepository processRepository, IVolumeHandler volumeHandler):base(new ProcessVolumeRequestHandler(logger, stateManager, cacheAside, processRepository, volumeHandler, token))
        {
            RegisterFeatureDecorator(new ConsumerFilter<ProcessExecutionContext>(token, logger, "VolumeGeneratorConsumer"));
        }

        //public ProcessVolumePipeline(IHandler<IProcessExecutionContext> handler):base(handler)
        //{

        //}
    }
}
