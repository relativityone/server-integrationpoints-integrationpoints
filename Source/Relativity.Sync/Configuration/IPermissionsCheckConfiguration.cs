namespace Relativity.Sync.Configuration
{
	internal interface IPermissionsCheckConfiguration : IConfiguration
	{
		int DataSourceArtifactId { get; }

		int DataDestinationArtifactId { get; }
	}
}