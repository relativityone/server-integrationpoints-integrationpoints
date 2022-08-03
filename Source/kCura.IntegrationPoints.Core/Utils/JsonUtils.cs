using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.Core.Utils
{
    public static class JsonUtils
    {
        public static string ReplacePropertyNameIfPresent(string json, string oldPropertyName, string newPropertyName)
        {
            JObject sourceObject = JObject.Parse(json);
            
            JProperty oldProperty = sourceObject.Property(oldPropertyName);
            if (oldProperty == null)
            {
                return json;
            }
            
            oldProperty.Replace(new JProperty(newPropertyName, oldProperty.Value));

            return JsonConvert.SerializeObject(sourceObject, Formatting.None);
        }

        public static string RemoveProperty(string json, string propertyName)
        {
            JObject sourceObject = JObject.Parse(json);

            sourceObject.Remove(propertyName);

            return JsonConvert.SerializeObject(sourceObject, Formatting.None);
        }

        public static string AddOrUpdatePropertyValues(string json, IDictionary<string, object> propertyValues)
        {
            JObject sourceObject = JObject.Parse(json);
            foreach (var propertyValue in propertyValues)
            {
                JProperty property = sourceObject.Property(propertyValue.Key);
                if (property == null)
                {
                    sourceObject.Add(new JProperty(propertyValue.Key, propertyValue.Value));
                }
                else
                {
                    property.Replace(new JProperty(propertyValue.Key, propertyValue.Value));
                }
            }

            return JsonConvert.SerializeObject(sourceObject, Formatting.None);
        }
    }
}
