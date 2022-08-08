using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
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
                int sourceWorkspaceDescriptorArtifactTypeId =
                    _sourceWorkspaceRdoInitializer.InitializeWorkspaceWithSourceWorkspaceRdo(destinationWorkspaceArtifactId);

                SourceWorkspaceDTO sourceWorkspaceDto = _sourceWorkspaceManager.CreateSourceWorkspaceDto(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, federatedInstanceArtifactId);

                _sourceJobRdoInitializer.InitializeWorkspaceWithSourceJobRdo(destinationWorkspaceArtifactId, sourceWorkspaceDescriptorArtifactTypeId);

                SourceJobDTO sourceJobDto = _sourceJobManager.CreateSourceJobDto(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, jobHistoryArtifactId, sourceWorkspaceDto.ArtifactId);

                _logger.LogInformation("Created TagsContainer for workspace {workspaceArtifactId} and job: {jobArtifactId}. Destination {destinationWorkspaceArtifactId}, instance: {federatedInstanceArtifactId}",
                    sourceWorkspaceDto.ArtifactId, sourceJobDto.ArtifactId, destinationWorkspaceArtifactId, federatedInstanceArtifactId);
                return new TagsContainer(sourceJobDto, sourceWorkspaceDto);
            }
            catch (Exception e)
            {
                throw LogAndWrapException(e, destinationWorkspaceArtifactId);
            }
        }

        private IntegrationPointsException LogAndWrapException(Exception e, int destinationWorkspaceArtifactId)
        {
            string message = $"Failed to create tags in destination workspace {destinationWorkspaceArtifactId}.";
            _logger.LogError(e, "Failed to create tags in destination workspace {workspaceId}.", destinationWorkspaceArtifactId);
            return
                new IntegrationPointsException(message, e);
        }
    }
}