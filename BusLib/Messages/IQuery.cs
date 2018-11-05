using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Messages
{
    public interface IQuery : IMessage
    {

    }

    public interface IQuery<T> : IQuery
    {

    }

}
