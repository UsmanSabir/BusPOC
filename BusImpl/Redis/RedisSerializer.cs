using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BusImpl.Redis
{
    public class RedisSerializer //:ISerializer
    {
        public byte[] Serialize(object item)
        {

            //using (MemoryStream memoryStream = new MemoryStream())
            {
                //  TextWriter tx = new StreamWriter(memoryStream);
                var txt = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                return Encoding.UTF8.GetBytes(txt);
                //tx.Write(txt);
                //JSON.Serialize(item, tx);
                //tx.Flush();
                //return memoryStream.ToArray();
            }
        }

        public Task<byte[]> SerializeAsync(object item)
        {
            return Task.Run(() => Serialize(item));
        }

        public object Deserialize(byte[] serializedObject)
        {
            return Deserialize<object>(serializedObject);
        }

        public Task<object> DeserializeAsync(byte[] serializedObject)
        {
            return Task.Run(() => Deserialize<object>(serializedObject));
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            //using (MemoryStream memoryStream = new MemoryStream(serializedObject))
            //{
            //    TextReader tx = new StreamReader(memoryStream);
            //    return JSON.Deserialize<T>(tx);
            //}

            var json = Encoding.UTF8.GetString(serializedObject);
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return obj;
            //using (var stream = new MemoryStream(serializedObject))
            //using (var reader = new StreamReader(stream, Encoding.UTF8))
            //    return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }

        public Task<T> DeserializeAsync<T>(byte[] serializedObject)
        {
            return Task.Run(() => Deserialize<T>(serializedObject));
        }

        public string SerializeToString<T>(T item)
        {
            var txt = Newtonsoft.Json.JsonConvert.SerializeObject(item);
            return txt;
        }

        public T DeserializeFromString<T>(string json)
        {
            var txt = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return txt;
        }

    }
}