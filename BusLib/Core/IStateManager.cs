using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface IStateManager
    {
        IEnumerable<T> GetAllIncomplete<T>() where T : ICompletableState;

        void Save<T>(T item) where T: ICompletableState;

        T GetById<T>(object id);
    }
}
