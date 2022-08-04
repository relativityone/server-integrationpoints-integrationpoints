using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Exceptions;
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
                _logger?.LogError(e, "An error occurred serializing object. Type: {objectType}", @object?.GetType());
                throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, e);
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
                _logger?.LogError(e, "An error occurred deserializing object. Type: {objectType}", objectType);
                throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, e);
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
                _logger?.LogError(e, "An error occurred deserializing object. Type: {objectType}", typeof(T));
                throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, e);
            }
        }
    }
}
