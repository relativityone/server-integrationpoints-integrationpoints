using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Relativity.Sync.Tests.Common
{
    [ExcludeFromCodeCoverage]
    public static class JsonHelpers
    {
        /// <summary>
        /// Serializes an object to JSON, then deserializes into an intermediate representation.
        /// </summary>
        /// <typeparam name="T">Type to which deserialized object should be casted. Type must inherit from <see cref="JToken"/>.</typeparam>
        /// <param name="value">Value to convert</param>
        /// <returns>Given value serialized and then deserialized into type <typeparamref name="T"/>.</returns>
        public static T ToJToken<T>(object value) where T : JToken
        {
            var writer = new StringWriter();
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, value);
            string serialized = writer.ToString();
            return (T)serializer.Deserialize(new JsonTextReader(new StringReader(serialized)));
        }

        /// <summary>
        /// Deserializes an JSON string into an intermediate representation
        /// </summary>
        /// <param name="json">JSON to deserialize</param>
        /// <returns><see cref="JToken"/> representing the deserialized JSON.</returns>
        public static object DeserializeJson(string json)
        {
            var serializer = new JsonSerializer();
            return serializer.Deserialize(new JsonTextReader(new StringReader(json)));
        }
    }
}
