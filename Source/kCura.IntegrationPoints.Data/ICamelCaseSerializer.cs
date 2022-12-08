namespace kCura.IntegrationPoints.Data
{
    public interface ICamelCaseSerializer
    {
        string SerializeCamelCase(object @object);

        T Deserialize<T>(string serializedString);
    }
}
