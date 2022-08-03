using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
    public static class SanitizationTestUtils
    {
        public static object DeserializeJson(string json)
        {
            var serializer = new JsonSerializer();
            return serializer.Deserialize(new JsonTextReader(new StringReader(json)));
        }

        public static T ToJToken<T>(object value) where T : JToken
        {
            var writer = new StringWriter();
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, value);
            string serialized = writer.ToString();
            return (T) serializer.Deserialize(new JsonTextReader(new StringReader(serialized)));
        }
    }
}
