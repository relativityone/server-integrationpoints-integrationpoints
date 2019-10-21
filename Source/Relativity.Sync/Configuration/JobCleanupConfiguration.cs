namespace Relativity.Sync.Configuration
{
	internal sealed class JobCleanupConfiguration : IJobCleanupConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;

		public JobCleanupConfiguration(SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
	}
}