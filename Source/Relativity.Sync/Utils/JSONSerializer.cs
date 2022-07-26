using System;
using Newtonsoft.Json;

namespace Relativity.Sync.Utils
{
    internal sealed class JSONSerializer : ISerializer
    {
        public string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public object Deserialize(Type objectType, string serializedString)
        {
            return JsonConvert.DeserializeObject(serializedString, objectType);
        }

        public T Deserialize<T>(string serializedString)
        {
            return (T) Deserialize(typeof(T), serializedString);
        }
    }
}