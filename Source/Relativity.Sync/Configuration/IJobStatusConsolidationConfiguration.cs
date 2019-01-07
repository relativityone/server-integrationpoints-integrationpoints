namespace Relativity.Sync.Configuration
{
	internal interface IJobStatusConsolidationConfiguration : IConfiguration
	{
		int StorageId { get; }

		bool IsJobStatusArtifactIdSet { get; }

		int JobStatusArtifactId { get; set; }
	}
}