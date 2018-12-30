using BusLib.Core;
using System;
using System.Collections.Concurrent;

namespace BusLib.Serializers
{
    internal class SerializersFactory : ISerializersFactory
    {
        static Lazy<SerializersFactory> instance = new Lazy<SerializersFactory>();
        public static SerializersFactory Instance
        {
            get
            {
                return instance.Value;
            }
        }

        readonly ConcurrentDictionary<Type, ISerializer> _serializers;
        internal ISerializer DefaultSerializer { get; set; }

        public SerializersFactory()
        {
            _serializers = new ConcurrentDictionary<Type, ISerializer>();
            DefaultSerializer = new ObjectSerializer();
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
            return GetSerializer(typeof(T));
        }

        public ISerializer GetSerializer(Type type)
        {
            if (_serializers.TryGetValue(type, out ISerializer serializer))
            {
                return serializer;
            }
            return DefaultSerializer;
        }
    }
}
