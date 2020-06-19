namespace Relativity.Sync.Storage
{
	internal sealed class DataSourceSnapshotConfiguration : DataSourceSnapshotConfigurationBase
	{
		public DataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters) : base(cache, fieldMappings, syncJobParameters)
		{
		}
	}
}
