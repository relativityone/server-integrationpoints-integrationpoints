namespace Relativity.Sync.Configuration
{
	internal interface IDataSourceSnapshotConfiguration : IConfiguration
	{
		int DataSourceArtifactId { get; }

		int SnapshotId { get; set; }
	}
}