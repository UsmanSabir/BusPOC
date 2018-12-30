using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BusLib.Core;
using BusLib.Helper;

namespace BusLib.BatchEngineCore.Handlers
{
    class TaskContext : SafeDisposable, ITaskContext
    {
        private readonly bool _isStateful;

        private ConcurrentDictionary<string, string> _stateDictionary=null;

        public TaskContext(bool isStateful, SafeDisposableActions onCompleteActions, IReadWritableTaskState state)
        {
            _isStateful = isStateful;
            OnCompleteActions = onCompleteActions;
            TaskStateWritable = state;
        }

        public ILogger Logger { get; internal set; }
        public IDashboardService DashboardService { get; internal set; }

        protected override void Dispose(bool disposing)
        {
            OnCompleteActions?.Dispose();
            OnCompleteActions = null;

            base.Dispose(disposing);

            Transaction?.Rollback();
        }

        internal IReadWritableTaskState TaskStateWritable { get; }

        public ITaskState State => TaskStateWritable;

        public string PrevState { get; internal set; }
        public string NextState { get; internal set; }
        public IProcessExecutionContext ProcessExecutionContext { get; internal set; }
        public ITransaction Transaction { get; internal set; }
        public CancellationToken CancellationToken { get; internal set; }
        public ResultStatus Result { get; set; }

        public bool IsDeferred { get; private set; } = false;

        public bool Defer()
        {
            IsDeferred = this.ReleaseTaskWithDeferFlag();
            return IsDeferred;
        }

        public int DeferredCount => State.DeferredCount;

        internal SafeDisposableActions OnCompleteActions { get; private set; }


        internal void SetStates(ConcurrentDictionary<string, string> statesDictionary)
        {
            if(statesDictionary==null) 
                return;

            if (_stateDictionary == null)
            {
                _stateDictionary = statesDictionary;
            }
            else
            {
                foreach (var keyValuePair in statesDictionary)
                {
                    _stateDictionary.AddOrUpdate(keyValuePair.Key, keyValuePair.Value, (k, v) => keyValuePair.Value);
                }
            }


        }

        public bool SetNextState(string next)
        {
            if (!_isStateful)
            {
#if DEBUG
                if (Debugger.IsAttached)
                {
                    throw new InvalidOperationException("Stateless process can't have state data");
                } 
#endif
            }
            PrevState = NextState;
            NextState = next;
            return true;
        }

    }
}
