using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ISerializer
    {
        //byte[] SerializeToBinary<T>(T item);
        string SerializeToString<T>(T item);

        ////T DeserializeFromBinary<T>(byte[] data);
        T DeserializeFromString<T>(string data);
    }

    public interface ISerializer<T> //: ISerializer
    {
        string SerializeToString(T item);

        T DeserializeFromString(string data);
    }
}
