using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
    public class RipJsonSerializer : ISerializer, ICamelCaseSerializer
    {
        private readonly IAPILog _logger;
        private readonly JsonSerializerSettings _camelCaseSettings;
        private readonly JsonSerializerSettings _defaultCaseSettings;

        public static RipJsonSerializer CreateWithoutLogger()
        {
            return new RipJsonSerializer(null);
        }

        public RipJsonSerializer(IAPILog logger)
        {
            _logger = logger?.ForContext<RipJsonSerializer>();

            _camelCaseSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() },
            };

            _defaultCaseSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() },
            };
        }


        public T Deserialize<T>(string serializedString) => (T)this.Deserialize(typeof(T), serializedString);

        public object Deserialize(Type objectType, string serializedString)
        {
            try
            {
                return JsonConvert.DeserializeObject(serializedString, objectType);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "An error occurred deserializing object. Type: {objectType}", objectType);
                throw new RipSerializationException("An error occurred deserializing object.", serializedString, e);
            }
        }

        public string Serialize(object @object)
        {
            return SerializeInternal(@object, _defaultCaseSettings);
        }

        public string SerializeCamelCase(object @object)
        {
            return SerializeInternal(@object, _camelCaseSettings);
        }

        private string SerializeInternal(object @object, JsonSerializerSettings settings)
        {
            try
            {
                return JsonConvert.SerializeObject(@object, settings);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "An error occurred serializing object. Type: {objectType}", @object?.GetType());
                throw new RipSerializationException("An error occurred serializing object.", @object?.ToString(), e);
            }
        }
    }
}
