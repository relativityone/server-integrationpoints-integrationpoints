namespace Relativity.Sync.Configuration
{
	internal interface IPreviousRunCleanupConfiguration : IConfiguration
	{
		bool IsPreviousRunArtifactIdSet { get; }

		int PreviousRunArtifactId { get; }

		bool Retrying { get; }
	}
}