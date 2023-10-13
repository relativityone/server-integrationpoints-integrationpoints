using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class JobHistoryManager : IJobHistoryManager
    {
        private readonly IAPILog _logger;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IMassUpdateHelper _massUpdateHelper;

        internal JobHistoryManager(
            IRepositoryFactory repositoryFactory,
            IAPILog logger,
            IMassUpdateHelper massUpdateHelper)
        {
            _repositoryFactory = repositoryFactory;
            _logger = (logger ?? throw new ArgumentNullException(nameof(logger)))
                .ForContext<JobHistoryManager>();
            _massUpdateHelper = massUpdateHelper;
        }

        public int GetLastJobHistoryArtifactId(int workspaceArtifactId, int integrationPointArtifactId)
        {
            IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
            return jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId);
        }

        public ChoiceRef GetLastJobHistoryStatus(int workspaceArtifactId, int integrationPointArtifactId)
        {
            IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
            return jobHistoryRepository.GetLastJobHistoryStatus(integrationPointArtifactId);
        }

        public JobHistory GetLastJobHistory(int workspaceArtifactId, int integrationPointArtifactId)
        {
            IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
            return jobHistoryRepository.GetLastJobHistory(integrationPointArtifactId);
        }

        public StoppableJobHistoryCollection GetStoppableJobHistory(int workspaceArtifactId, int integrationPointArtifactId)
        {
            IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
            IList<JobHistory> stoppableJobHistories = jobHistoryRepository.GetStoppableJobHistoriesForIntegrationPoint(integrationPointArtifactId);

            IDictionary<string, JobHistory[]> jobHistoriesByStatus = stoppableJobHistories
                .Where(x => x.JobStatus != null)
                .GroupBy(x => x.JobStatus?.Name)
                .Select(x => new { Key = x.Key, Values = x.ToArray() })
                .ToDictionary(x => x.Key, x => x.Values);

            List<JobHistory> processingJobHistory = new List<JobHistory>();
            if (jobHistoriesByStatus.ContainsKey(JobStatusChoices.JobHistoryProcessing.Name))
            {
                processingJobHistory.AddRange(jobHistoriesByStatus[JobStatusChoices.JobHistoryProcessing.Name]);
            }

            if (jobHistoriesByStatus.ContainsKey(JobStatusChoices.JobHistoryValidating.Name))
            {
                processingJobHistory.AddRange(jobHistoriesByStatus[JobStatusChoices.JobHistoryValidating.Name]);
            }

            return new StoppableJobHistoryCollection
            {
                PendingJobHistory = jobHistoriesByStatus.ContainsKey(JobStatusChoices.JobHistoryPending.Name)
                    ? jobHistoriesByStatus[JobStatusChoices.JobHistoryPending.Name]
                    : Array.Empty<JobHistory>(),
                ProcessingJobHistory = processingJobHistory.ToArray()
            };
        }

        public void SetErrorStatusesToExpired(int workspaceArtifactID, int jobHistoryArtifactID)
        {
            SetErrorStatusesToExpiredAsync(workspaceArtifactID, jobHistoryArtifactID).GetAwaiter().GetResult();
        }

        public async Task SetErrorStatusesToExpiredAsync(int workspaceArtifactID, int jobHistoryArtifactID)
        {
            JobHistoryErrorDTO.Choices.ErrorType.Values[] errorTypesToSetToExpired =
            {
                JobHistoryErrorDTO.Choices.ErrorType.Values.Item,
                JobHistoryErrorDTO.Choices.ErrorType.Values.Job
            };

            foreach (JobHistoryErrorDTO.Choices.ErrorType.Values errorTypeToSetToExpired in errorTypesToSetToExpired)
            {
                await SetErrorStatusesAsync(
                        workspaceArtifactID,
                        jobHistoryArtifactID,
                        ErrorStatusChoices.JobHistoryErrorExpiredGuid,
                        errorTypeToSetToExpired)
                    .ConfigureAwait(false);
            }
        }

        private async Task SetErrorStatusesAsync(
            int workspaceArtifactID,
            int jobHistoryArtifactID,
            Guid errorStatusChoiceValueGuid,
            JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
        {
            try
            {
                IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactID);
                ICollection<int> itemLevelErrorArtifactIds = jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryArtifactID, errorType);

                FieldUpdateRequestDto[] fieldsToUpdate =
                {
                    new FieldUpdateRequestDto(
                        JobHistoryErrorFieldGuids.ErrorStatusGuid,
                        new SingleChoiceReferenceDto(errorStatusChoiceValueGuid))
                };

                await _massUpdateHelper
                    .UpdateArtifactsAsync(
                        itemLevelErrorArtifactIds,
                        fieldsToUpdate,
                        jobHistoryErrorRepository)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogSettingErrorStatusError(workspaceArtifactID, jobHistoryArtifactID, errorType, e);
                // ignore failure
            }
        }

        #region Logging

        private void LogSettingErrorStatusError(
            int workspaceArtifactID,
            int jobHistoryArtifactID,
            JobHistoryErrorDTO.Choices.ErrorType.Values errorType,
            Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to set error status ({ErrorType}) for JobHistory {JobHistoryId} in Workspace {WorkspaceId}.",
                errorType,
                jobHistoryArtifactID,
                workspaceArtifactID);
        }

        #endregion
    }
}
