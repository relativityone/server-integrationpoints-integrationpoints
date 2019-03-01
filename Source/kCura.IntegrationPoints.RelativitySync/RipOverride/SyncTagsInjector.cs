using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
	// We want to use tags created before, this is why we're creating TagsContainer with values obtained earlier and stored in SyncConfiguration
	internal sealed class SyncTagsInjector : ITagsCreator
	{
		private readonly SyncConfiguration _config;

		public SyncTagsInjector(SyncConfiguration config)
		{
			_config = config;
		}

		public TagsContainer CreateTags(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int jobHistoryArtifactId, int? federatedInstanceArtifactId)
		{
			SourceWorkspaceDTO sourceWorkspaceTag = GetSourceWorkspaceTag();
			SourceJobDTO sourceJobTag = GetSourceJobTag();
			return new TagsContainer(sourceJobTag, sourceWorkspaceTag);
		}

		private SourceJobDTO GetSourceJobTag()
		{
			return new SourceJobDTO
			{
				ArtifactId = _config.SourceJobTagArtifactId,
				Name = _config.SourceJobTagName
			};
		}

		private SourceWorkspaceDTO GetSourceWorkspaceTag()
		{
			return new SourceWorkspaceDTO
			{
				ArtifactId = _config.SourceWorkspaceTagArtifactId,
				Name = _config.SourceWorkspaceTagName
			};
		}
	}
}