using System;
using System.Collections.Generic;
using System.Linq;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Volume;
using BusLib.Serializers;

namespace BusImpl
{
    public class VolumeHandler : IVolumeHandler
    {
        public IReadWritableTaskState GetNextTaskWithTransaction(out ITransaction transaction)
        {
            throw new NotImplementedException();
        }

        public void Handle<T>(IEnumerable<T> volume, IProcessExecutionContext processContext)
        {
            throw new NotImplementedException();
        }
    }
}