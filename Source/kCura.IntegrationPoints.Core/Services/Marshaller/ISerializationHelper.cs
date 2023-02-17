using System;

namespace kCura.IntegrationPoints.Core.Services.Marshaller
{
    public interface ISerializationHelper
    {
        /// <summary>
        /// Convert a byte array to an Object
        /// </summary>
        /// <typeparam name="T">Type of object to create from byte array</typeparam>
        /// <param name="byteArray">Array of bytes representing class</param>
        /// <returns>An instance of the request type</returns>
        T Deserialize<T>(byte[] byteArray) where T : class;

        /// <summary>
        /// Convert an object to a byte array
        /// </summary>
        /// <param name="obj">Object to convert to byte aray. Note: Object must be serializable</param>
        /// <returns>byte array representation of object</returns>
        byte[] Serialize(Object obj);
    }
}
