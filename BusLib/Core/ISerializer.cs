using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T item);

        T Deserialize<T>(byte[] data);
    }
}
