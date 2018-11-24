using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore.Saga
{
    public abstract class BaseSagaTask<T> : ITaskSaga<T>
    {
        private readonly ConcurrentDictionary<string, Action<ITaskContext<T>>> _sagaStateDictionary =
            new ConcurrentDictionary<string, Action<ITaskContext<T>>>();

        protected BaseSagaTask()
        {
            //DefineSagaStates();
        }

        // protected abstract void DefineSagaStates();

        protected void DefineState(string name, Action<ITaskContext<T>> action)
        {
            _sagaStateDictionary.AddOrUpdate(name, action, (s,a)=> action);            
        }

        protected internal virtual string GetNextState(ITaskContext<T> context)
        {
            if (!string.IsNullOrWhiteSpace(context.NextState))
                return context.NextState;

            var nextState = _sagaStateDictionary.Keys.SkipWhile(s => s != context.State.CurrentState).ElementAtOrDefault(1);
            return nextState;
        }

        public virtual void Started(ITaskContext<T> context)
        {
            
        }

        public virtual void Completed(ITaskContext<T> context)
        {
            
        }

        public void Execute(ITaskContext<T> context)
        {
            Started(context);
        }

        public abstract int ProcessKey { get; }
    }
}
