using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class JobEndMetricsConfiguration : SumReporterConfiguration, IJobEndMetricsConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;

		public JobEndMetricsConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters) : base(cache, syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SyncConfigurationArtifactId => _syncJobParameters.JobId;
	}
}