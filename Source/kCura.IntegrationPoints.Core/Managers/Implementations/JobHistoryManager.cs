using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

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
			_logger = logger.ForContext<JobHistoryManager>();
			_massUpdateHelper = massUpdateHelper;
		}

		public int GetLastJobHistoryArtifactId(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
			return jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId);
		}

		public StoppableJobCollection GetStoppableJobCollection(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
			IDictionary<Guid, int[]> stoppableJobStatusDictionary = jobHistoryRepository.GetStoppableJobHistoryArtifactIdsByStatus(integrationPointArtifactId);

			stoppableJobStatusDictionary.TryGetValue(JobStatusChoices.JobHistoryPendingGuid, out int[] pendingJobArtifactIDs);
			stoppableJobStatusDictionary.TryGetValue(JobStatusChoices.JobHistoryProcessingGuid, out int[] processingJobArtifactIDs);

			var stoppableJobCollection = new StoppableJobCollection
			{
				PendingJobArtifactIds = pendingJobArtifactIDs ?? new int[0],
				ProcessingJobArtifactIds = processingJobArtifactIDs ?? new int[0]
			};

			return stoppableJobCollection;
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

		private void LogSettingErrorStatusError(int workspaceArtifactID, int jobHistoryArtifactID, JobHistoryErrorDTO.Choices.ErrorType.Values errorType, Exception e)
		{
			_logger.LogError(e, "Failed to set error status ({ErrorType}) for JobHistory {JobHistoryId} in Workspace {WorkspaceId}.", errorType, jobHistoryArtifactID,
				workspaceArtifactID);
		}

		#endregion
	}
}