using System;
using System.Runtime.InteropServices;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Saga;
using BusLib.Core;

namespace CQRsConsole.TestProcess
{
    public class TestEngine
    {
        IStateManager _stateManager;

        void Start(string name)
        {
            var p=new UnitTaskProcess();
            var volume = p.GetVolume(null);
            //save state i.e. volume, sql or redis or mongodb etc

            var type = p.VolumeDataType;
            var isSaga = typeof(ITaskSaga<>).IsAssignableFrom(p.TaskActorType);
            
            //todo resolve actor_type using ioc container

            foreach (var i in volume)
            {
                var ctxt = SagaTaskContext.Create(i);

            }

        }
    }
    
    class SagaTaskContext
    {
        public static SagaTaskContext<T> Create<T>(T val)
        {
            return new SagaTaskContext< T>(val);
        }
    }
    class SagaTaskContext<T>: ISagaTaskContext<T>
    {
        public SagaTaskContext(T val)
        {
            Data = val;
        }
        public DateTime UpdatedOn { get; }
        public TaskStatus Status { get; }
        public int RetryCount { get; }
        public IProcessExecutionContext ProcessExecutionContext { get; }
        public T Data { get; }
        public string State { get; }
        public string PreviousState { get; }
        public string NextState { get; }
    }
}