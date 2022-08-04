namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    internal interface ISanitizationDeserializer
    {
        T DeserializeAndValidateExportFieldValue<T>(object initialValue);
    }
}