using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore.Saga
{
    public interface ITaskSaga<in T> :ITask<T, ITaskContext<T>>
    {
        
    }

}
