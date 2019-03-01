using kCura.IntegrationPoints.Core.Tagging;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
	// We want to use tags created before, this is why we're creating TagsContainer with values obtained earlier and stored in SyncConfiguration
	internal sealed class SourceWorkspaceTagInjector : ISourceWorkspaceTagCreator
	{
		private readonly SyncConfiguration _syncConfiguration;

		public SourceWorkspaceTagInjector(SyncConfiguration syncConfiguration)
		{
			_syncConfiguration = syncConfiguration;
		}

		public int CreateDestinationWorkspaceTag(int destinationWorkspaceId, int jobHistoryInstanceId, int? federatedInstanceId)
		{
			return _syncConfiguration.DestinationWorkspaceTagArtifactId;
		}
	}
}