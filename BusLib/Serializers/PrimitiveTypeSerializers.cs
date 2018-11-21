using BusLib.Core;
using System;

namespace BusLib.Serializers
{
    
    internal class PrimitiveSerializer : ISerializer
    {
        public PrimitiveSerializer()
        {
            
        }

        public T DeserializeFromString<T>(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return default(T);

            return (T)Convert.ChangeType(data, typeof(T));            
        }

        public string SerializeToString<T>(T item)
        {
            if (item.Equals(default(T)))
                return string.Empty;

            return item.ToString();
        }
    }

    internal class StringSerializer : ISerializer
    {
        public StringSerializer()
        {

        }

        public T DeserializeFromString<T>(string data)
        {
            return (T)((object)data);
        }

        public string SerializeToString<T>(T item)
        {
            return item?.ToString();
        }
    }

    internal class ObjectSerializer : ISerializer
    {
        public ObjectSerializer()
        {

        }

        public T DeserializeFromString<T>(string data)
        {
            throw new NotImplementedException();
        }

        public string SerializeToString<T>(T item)
        {
            throw new NotImplementedException();
        }
    }


}
