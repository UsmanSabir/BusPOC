using System;
using BusLib.Core;

namespace BusLib.Serializers
{
    internal interface ISerializersFactory
    {
        ISerializer GetSerializer(Type type);
        ISerializer GetSerializer<T>();
    }
}