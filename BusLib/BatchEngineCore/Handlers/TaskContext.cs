using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private ConcurrentDictionary<string, string> _stateDictionary=null;

        public TaskContext(SafeDisposableActions onCompleteActions)
        {
            OnCompleteActions = onCompleteActions;
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

        
        public ITaskState State { get; internal set; }
        public string PrevState { get; internal set; }
        public string NextState { get; internal set; }
        public IProcessExecutionContext ProcessExecutionContext { get; internal set; }
        public ITransaction Transaction { get; internal set; }
        public CancellationToken CancellationToken { get; internal set; }
        public ResultStatus Result { get; set; }
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

    }
}
