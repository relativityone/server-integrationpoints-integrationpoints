using System;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
    public class IntegrationPointSerializer : IIntegrationPointSerializer
    {
        private readonly ISerializer _serializer;

        public IntegrationPointSerializer(IAPILog logger)
        {
            _serializer = SerializerWithLogging.Create(logger);
        }

        public string Serialize(object o)
        {
            return _serializer.Serialize(o);
        }

        public object Deserialize(Type objectType, string serializedString)
        {
            return _serializer.Deserialize(objectType, serializedString);
        }

        public T Deserialize<T>(string serializedString)
        {
            return _serializer.Deserialize<T>(serializedString);
        }
    }
}
