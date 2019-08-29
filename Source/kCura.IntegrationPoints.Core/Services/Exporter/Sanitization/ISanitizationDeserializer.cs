namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal interface ISanitizationDeserializer
	{
		T DeserializeAndValidateExportFieldValue<T>(string itemIdentifier, string sanitizingSourceFieldName, object initialValue);
	}
}
