using System;
using kCura.Apps.Common.Utils.Serializers;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    internal class SanitizationDeserializer : ISanitizationDeserializer
    {
        private readonly ISerializer _serializer;

        public SanitizationDeserializer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        // We have to re-serialize and deserialize the value from Export API due to REL-250554.
        public T DeserializeAndValidateExportFieldValue<T>(object initialValue)
        {
            T fieldValue;
            try
            {
                fieldValue = _serializer.Deserialize<T>(initialValue.ToString());
            }
            catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
            {
                throw new InvalidExportFieldValueException($"Expected value to be deserializable to {typeof(T)}, but instead type was {initialValue.GetType()}.", ex);
            }

            return fieldValue;
        }
    }
}
