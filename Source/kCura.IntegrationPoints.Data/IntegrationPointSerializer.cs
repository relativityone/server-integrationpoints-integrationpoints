using System;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
    public class IntegrationPointSerializer : ISerializer, ICamelCaseSerializer
    {
        private readonly IAPILog _logger;
        private readonly JsonSerializerSettings _camelCaseSettings;

        public static IntegrationPointSerializer CreateWithoutLogger()
        {
            return new IntegrationPointSerializer(null);
        }

        public IntegrationPointSerializer(IAPILog logger)
        {
            _logger = logger?.ForContext<IntegrationPointSerializer>();
            _camelCaseSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
        }

        public string Serialize(object @object)
        {
            try
            {
                return JsonConvert.SerializeObject(@object);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "An error occurred serializing object. Type: {objectType}", @object?.GetType());
                throw new RipSerializationException("An error occurred serializing object.", @object.ToString(), e);
            }
        }

        public string SerializeCamelCase(object @object)
        {
            try
            {
                return JsonConvert.SerializeObject(@object, _camelCaseSettings);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "An error occurred serializing object. Type: {objectType}", @object?.GetType());
                throw new RipSerializationException("An error occurred serializing object.", @object.ToString(), e);
            }
        }

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

        public T Deserialize<T>(string serializedString) => (T)this.Deserialize(typeof(T), serializedString);
    }
}
