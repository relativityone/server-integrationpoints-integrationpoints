using System;
using kCura.IntegrationPoints.Core.Managers;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tagging
{
	public class TagsCreator : ITagsCreator
	{
		private readonly ISourceJobManager _sourceJobManager;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly IAPILog _logger;

		public TagsCreator(ISourceJobManager sourceJobManager, ISourceWorkspaceManager sourceWorkspaceManager, IHelper helper)
		{
			_sourceJobManager = sourceJobManager;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<TagsCreator>();
		}

		public TagsContainer CreateTags(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int jobHistoryArtifactId, int? federatedInstanceArtifactId)
		{
			try
			{
				var sourceWorkspaceDto = _sourceWorkspaceManager.InitializeWorkspace(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, federatedInstanceArtifactId);
				var sourceJobDto = _sourceJobManager.InitializeWorkspace(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, sourceWorkspaceDto.ArtifactTypeId,
					sourceWorkspaceDto.ArtifactId, jobHistoryArtifactId);
				return new TagsContainer(sourceJobDto, sourceWorkspaceDto);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to create tags in destination workspace {workspaceId}.", destinationWorkspaceArtifactId);
				throw;
			}
		}
	}
}