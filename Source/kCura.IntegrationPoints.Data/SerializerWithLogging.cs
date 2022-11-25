using System;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
    public class SerializerWithLogging : ISerializer
    {
        private readonly ISerializer _serializerImplementation;
        private readonly IAPILog _logger;

        private SerializerWithLogging(ISerializer serializer, IAPILog logger)
        {
            _serializerImplementation = serializer;
            _logger = logger?.ForContext<SerializerWithLogging>();
        }

        public static SerializerWithLogging Create(IAPILog logger)
        {
            var serializer = new JSONSerializer();
            return new SerializerWithLogging(serializer, logger);
        }

        public string Serialize(object @object)
        {
            try
            {
                return _serializerImplementation.Serialize(@object);
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
                return _serializerImplementation.Deserialize(objectType, serializedString);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "An error occurred deserializing object. Type: {objectType}", objectType);
                throw new RipSerializationException("An error occurred deserializing object.", serializedString, e);
            }
        }

        public T Deserialize<T>(string serializedString)
        {
            try
            {
                return _serializerImplementation.Deserialize<T>(serializedString);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "An error occurred deserializing object. Type: {objectType}", typeof(T));
                throw new RipSerializationException("An error occurred deserializing object.", serializedString, e);
            }
        }
    }
}
