using BusLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore.Volume
{
    internal class ProcessVolumePipeline:Pipeline<IProcessExecutionContext>
    {
        public ProcessVolumePipeline():base(new ProcessVolumeRequestHandler())
        {

        }

        //public ProcessVolumePipeline(IHandler<IProcessExecutionContext> handler):base(handler)
        //{

        //}
    }
}
