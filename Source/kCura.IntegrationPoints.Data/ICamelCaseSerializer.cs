using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Data
{
    /// <summary>
    /// Represents the serializer having an extra ability to serialize the object with camel-case property naming.
    /// </summary>
    public interface ICamelCaseSerializer
    {
        /// <summary>
        /// Serializes the object using default contract resolver, respects the <see cref="JsonPropertyAttribute"/>  over the properties.
        /// </summary>
        /// <param name="object">The object to serialize.</param>
        /// <exception cref="RipSerializationException">Thrown when serialization fails.</exception>
        string Serialize(object @object);

        /// <summary>
        /// Serializes the object with the camel-case property names mode.
        /// </summary>
        /// <param name="object">The object to serialize.</param>
        /// <exception cref="RipSerializationException">Thrown when serialization fails.</exception>
        string SerializeCamelCase(object @object);

        /// <summary>
        /// Deserializes the string to desired type of T.
        /// </summary>
        /// <param name="serializedString">The string to deserialize.</param>
        /// <typeparam name="T">The desired type of result.</typeparam>
        /// <exception cref="RipSerializationException">Thrown when deserialization fails.</exception>
        T Deserialize<T>(string serializedString);
    }
}
