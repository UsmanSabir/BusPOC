using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusLib.BatchEngineCore.Handlers;

namespace BusLib.BatchEngineCore.Saga
{
    internal interface ITaskSaga<in T> : ITask<T, TaskContext>
    {
        
    }

}
