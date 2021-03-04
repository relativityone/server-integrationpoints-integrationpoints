using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class SourceWorkspaceObjectTypesCreationConfiguration : ISourceWorkspaceObjectTypesCreationConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;

		public SourceWorkspaceObjectTypesCreationConfiguration(SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
	}
}