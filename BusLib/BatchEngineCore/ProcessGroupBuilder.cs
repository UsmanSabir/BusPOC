using System;
using BusLib.BatchEngineCore.VolumeAdapters;

namespace BusLib.BatchEngineCore
{
    public class ProcessGroupBuilder
    {
        public void BuildProcessGroup()
        {
            var process = GetProcessByRef("TestProcess");
            var childProcess = GetProcessByRef("TestChildProcess");
            var volumeAdapter = GetVolumeAdapterByName("StreamingAdapter", process.VolumeType, childProcess.VolumeType);


        }

        private object GetVolumeAdapterByName(string adapterName, Type sourceType, Type targetType)
        {
            // ioc, scan assemblies. Generic types
            // or type mapping dictionary to store type mapping with 

            if (targetType.IsAssignableFrom(sourceType))
            {
                //no type conversion required
            }
            else
            {
                //introduce a type adapter
                //ITypeConverterVolumeAdapter<>
            }

            throw new NotImplementedException();
        }
        
        private IBaseProcess GetProcessByRef(string processRef)
        {
            throw new System.NotImplementedException();
        }
    }
}