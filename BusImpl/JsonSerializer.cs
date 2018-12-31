using BusLib.Core;


namespace BusImpl
{
    public class JsonSerializer: ISerializer
    {
        public string SerializeToString<T>(T item)
        {
            var serializeObject = Newtonsoft.Json.JsonConvert.SerializeObject(item);
            return serializeObject;
            //var serialize = JSON.Serialize<T>(item, Options.IncludeInheritedUtc);
            //return serialize;
        }

        public T DeserializeFromString<T>(string data)
        {
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
            return obj;
            //var item = JSON.Deserialize<T>(data);
            //return item;
        }
    }
}