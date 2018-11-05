using BusLib.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface IQueryHandler<in TRequest, TResponse> where TRequest: IQuery
    {
        TResponse Handle(TRequest message);
    }
}
