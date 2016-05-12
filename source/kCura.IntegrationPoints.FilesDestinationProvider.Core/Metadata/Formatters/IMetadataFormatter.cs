namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Metadata.Formatters
{
	public interface IMetadataFormatter
	{
		string GetHeaders(MetadataSettings settings);
	}
}
