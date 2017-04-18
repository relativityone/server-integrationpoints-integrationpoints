using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tagging
{
	public class TagsCreator : ITagsCreator
	{
		private readonly ISourceJobManager _sourceJobManager;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly IRelativitySourceJobRdoInitializer _sourceJobRdoInitializer;
		private readonly IRelativitySourceWorkspaceRdoInitializer _sourceWorkspaceRdoInitializer;
		private readonly IAPILog _logger;

		public TagsCreator(ISourceJobManager sourceJobManager, ISourceWorkspaceManager sourceWorkspaceManager,
			IRelativitySourceJobRdoInitializer sourceJobRdoInitializer, IRelativitySourceWorkspaceRdoInitializer sourceWorkspaceRdoInitializer, IHelper helper)
		{
			_sourceJobManager = sourceJobManager;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_sourceJobRdoInitializer = sourceJobRdoInitializer;
			_sourceWorkspaceRdoInitializer = sourceWorkspaceRdoInitializer;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<TagsCreator>();
		}

		public TagsContainer CreateTags(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int jobHistoryArtifactId, int? federatedInstanceArtifactId)
		{
			try
			{
				var sourceWorkspaceDescriptorArtifactTypeId =
					_sourceWorkspaceRdoInitializer.InitializeWorkspaceWithSourceWorkspaceRdo(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);

				var sourceWorkspaceDto = _sourceWorkspaceManager.CreateSourceWorkspaceDto(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, federatedInstanceArtifactId,
					sourceWorkspaceDescriptorArtifactTypeId);

				var sourceJobDescriptorArtifactTypeId = _sourceJobRdoInitializer.InitializeWorkspaceWithSourceJobRdo(destinationWorkspaceArtifactId, sourceWorkspaceDto.ArtifactTypeId);

				var sourceJobDto = _sourceJobManager.CreateSourceJobDto(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, jobHistoryArtifactId, sourceWorkspaceDto.ArtifactId,
					sourceJobDescriptorArtifactTypeId);

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