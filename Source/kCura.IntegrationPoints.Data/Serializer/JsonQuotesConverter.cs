using System;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Data
{
    /// <summary>
    /// Forces surrounding the value with quotes, regardless if the value is a string or any other type.
    /// BEFORE: { "artifactId": 12345 }
    /// AFTER:  { "artifactId": "12345" }
    /// NOTE: This converter was created as a workaround for keeping compliant with RIP front-end where many of boolean and integer values are treated as strings.
    /// </summary>
    public class JsonQuotesConverter : JsonConverter
    {
        private readonly Type _boolType = typeof(bool);
        private readonly Type _intType = typeof(int);


        public override bool CanConvert(Type objectType)
        {
            return objectType == _boolType || objectType == _intType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == _boolType)
            {
                return Convert.ToBoolean(reader.Value);
            }

            if (objectType == _intType)
            {
                return Convert.ToInt32(reader.Value);
            }

            throw new NotSupportedException($"Unsupported object type in {nameof(JsonQuotesConverter)}: {objectType.FullName}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // string values are surrounded with quotes, so let's pass the value converted to string:
            writer.WriteValue(JsonConvert.ToString(value));
        }
    }
}
