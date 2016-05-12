namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Metadata
{
	internal interface IMetadaGenerator
	{
		void Create(MetadataSettings settings);
		void WriteHerader(MetadataSettings settings);
	}
}
