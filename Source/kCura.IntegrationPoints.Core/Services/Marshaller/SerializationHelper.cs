using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace kCura.IntegrationPoints.Core.Services.Marshaller
{
    // kudos to : http://stackoverflow.com/questions/4865104/convert-any-object-to-a-byte
    public class SerializationHelper : ISerializationHelper
    {
        public T Deserialize<T>(byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return null;
            }

            T obj = null;
            using (var stream = new MemoryStream())
            {
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var formatter = new BinaryFormatter();
                try
                {
                    obj = formatter.Deserialize(stream) as T;
                }
                catch (Exception ex)
                {
                    throw new SerializationException("Could not deserialize data", ex);
                }
            }

            return obj;
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            byte[] streamBytes = { };
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                streamBytes = stream.ToArray();
            }

            return streamBytes;
        }
    }
}