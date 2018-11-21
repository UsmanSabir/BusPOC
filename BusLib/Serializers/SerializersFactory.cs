using BusLib.Core;
using System;
using System.Collections.Concurrent;

namespace BusLib.Serializers
{
    internal class SerializersFactory
    {
        readonly ConcurrentDictionary<Type, ISerializer> _serializers;
        readonly ISerializer _defaultSerializer;

        public SerializersFactory()
        {
            _serializers = new ConcurrentDictionary<Type, ISerializer>();
            _defaultSerializer = new ObjectSerializer();
            PrimitiveSerializer defaultSer = new PrimitiveSerializer();


            _serializers.TryAdd(typeof(int), defaultSer);
            _serializers.TryAdd(typeof(short), defaultSer);
            _serializers.TryAdd(typeof(long), defaultSer);
            _serializers.TryAdd(typeof(decimal), defaultSer);
            _serializers.TryAdd(typeof(float), defaultSer);
            _serializers.TryAdd(typeof(char), defaultSer);
            _serializers.TryAdd(typeof(double), defaultSer);
            _serializers.TryAdd(typeof(bool), defaultSer);

            _serializers.TryAdd(typeof(string), new StringSerializer());
        }

        public ISerializer GetSerializer<T>()
        {
            if(_serializers.TryGetValue(typeof(T), out ISerializer serializer))
            {
                return serializer;
            }
            return _defaultSerializer;            
        }

    }
}
