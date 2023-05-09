using System;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
    public class TargetDocumentsTaggingManager : IConsumeScratchTableBatchStatus
    {
        private readonly int _destinationWorkspaceArtifactId;
        private readonly int? _federatedInstanceArtifactId;
        private readonly int _jobHistoryArtifactId;
        private readonly IAPILog _logger;
        private readonly int _sourceWorkspaceArtifactId;
        private readonly ITagsCreator _tagsCreator;
        private readonly ITagger _tagger;
        private readonly ITagSavedSearchManager _tagSavedSearchManager;
        private readonly ImportSettings _importSettings;
        private bool _errorOccurDuringJobStart;
        private TagsContainer _tagsContainer;

        public TargetDocumentsTaggingManager(
            IRepositoryFactory repositoryFactory,
            ITagsCreator tagsCreator,
            ITagger tagger,
            ITagSavedSearchManager tagSavedSearchManager,
            IHelper helper,
            ImportSettings importSettings,
            int sourceWorkspaceArtifactId,
            int destinationWorkspaceArtifactId,
            int? federatedInstanceArtifactId,
            int jobHistoryArtifactId,
            string uniqueJobId)
        {
            ScratchTableRepository = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId,
                Data.Constants.TEMPORARY_DOC_TABLE_SOURCEWORKSPACE, uniqueJobId);
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<TargetDocumentsTaggingManager>();

            _sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
            _destinationWorkspaceArtifactId = destinationWorkspaceArtifactId;
            _federatedInstanceArtifactId = federatedInstanceArtifactId;
            _jobHistoryArtifactId = jobHistoryArtifactId;
            _tagger = tagger;
            _tagsCreator = tagsCreator;
            _tagSavedSearchManager = tagSavedSearchManager;
            _importSettings = importSettings;
        }

        public IScratchTableRepository ScratchTableRepository { get; }

        public void OnJobStart(Job job)
        {
            try
            {
                _tagsContainer = _tagsCreator.CreateTags(_sourceWorkspaceArtifactId, _destinationWorkspaceArtifactId, _jobHistoryArtifactId, _federatedInstanceArtifactId);
            }
            catch (Exception e)
            {
                LogErrorDuringJobStart(e);
                _errorOccurDuringJobStart = true;
                throw;
            }
        }

        public void OnJobComplete(Job job)
        {
            try
            {
                if (!_errorOccurDuringJobStart)
                {
                    _tagger.TagDocuments(_tagsContainer, ScratchTableRepository);
                    _tagSavedSearchManager.CreateSavedSearchForTagging(_destinationWorkspaceArtifactId, _importSettings.DestinationConfiguration, _tagsContainer);
                }
            }
            catch (Exception e)
            {
                LogErrorDuringJobComplete(e);
                throw;
            }
            finally
            {
                ScratchTableRepository.Dispose();
            }
        }

        #region Logging

        private void LogErrorDuringJobStart(Exception e)
        {
            _logger.LogError(e, $"Error occurred during job starting in {nameof(TargetDocumentsTaggingManager)}");
        }

        private void LogErrorDuringJobComplete(Exception e)
        {
            _logger.LogError(e, $"Error occurred during job completion in {nameof(TargetDocumentsTaggingManager)}");
        }

        #endregion
    }
}
