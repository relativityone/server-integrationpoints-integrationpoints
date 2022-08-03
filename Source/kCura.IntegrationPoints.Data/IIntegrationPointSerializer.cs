using System;

namespace kCura.IntegrationPoints.Data
{
    public interface IIntegrationPointSerializer
    {
        string Serialize(object o);

        object Deserialize(Type objectType, string serializedString);

        T Deserialize<T>(string serializedString);
    }
}
