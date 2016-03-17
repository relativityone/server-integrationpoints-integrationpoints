namespace kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata.Formatters
{
	public interface IMetadataFormatter
	{
		string GetHeaders(MetadataSettings settings);
	}
}
