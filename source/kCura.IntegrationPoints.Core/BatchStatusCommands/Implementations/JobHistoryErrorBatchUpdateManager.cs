using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryErrorBatchUpdateManager : IBatchStatus
	{
		private readonly IJobHistoryErrorManager _jobHistoryErrorManager;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private readonly int _savedSearchArtifactId;
		private readonly int _jobHistoryErrorTypeId;

		public JobHistoryErrorBatchUpdateManager(IJobHistoryErrorManager jobHistoryErrorManager, IRepositoryFactory repositoryFactory, 
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, int sourceWorkspaceArtifactId, int submittedBy, 
			JobHistoryErrorDTO.UpdateStatusType updateStatusType, int savedSearchArtifactId)
		{
			_jobHistoryErrorManager = jobHistoryErrorManager;
			_repositoryFactory = repositoryFactory; 
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_updateStatusType = updateStatusType;
			_jobHistoryErrorTypeId = SetJobHistoryErrorArtifactTypeId(_sourceWorkspaceArtifactId);
			_savedSearchArtifactId = savedSearchArtifactId;
		}

		public void OnJobStart(Job job)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactId);

			if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
			{
				switch (_updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStartOther, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;
				}
			}
			else //This runs for Run Now or Scheduled jobs
			{
				switch (_updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;
				}
			}
		}

		public void OnJobComplete(Job job)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactId);

			if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
			{
				if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly)
				{
					UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemComplete, jobHistoryErrorRepository,
						ErrorStatusChoices.JobHistoryErrorRetried);
					jobHistoryErrorRepository.DeleteItemLevelErrorsSavedSearch(_sourceWorkspaceArtifactId, _savedSearchArtifactId, 0);
				}
				else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly ||
						_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem)
				{
					UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobComplete, jobHistoryErrorRepository,
						ErrorStatusChoices.JobHistoryErrorRetried);
				}
			}
		}

		private void UpdateStatuses(IScratchTableRepository scratchTable, IJobHistoryErrorRepository jobHistoryErrorRepository, Relativity.Client.Choice errorStatus)
		{
			try
			{
				int numberOfErrors = scratchTable.Count;
				IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_sourceWorkspaceArtifactId);
				int errorStatusChoiceArtifactId = artifactGuidRepository.GetArtifactIdsForGuids(errorStatus.ArtifactGuids)[errorStatus.ArtifactGuids[0]];

				jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, numberOfErrors, _jobHistoryErrorTypeId,
					_sourceWorkspaceArtifactId, errorStatusChoiceArtifactId, scratchTable.GetTempTableName());
			}
			finally
			{
				scratchTable.Dispose();
			}
		}

		private int SetJobHistoryErrorArtifactTypeId(int workspaceArtifactId)
		{
			IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(workspaceArtifactId);
			int? objectTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistoryError));

			if (!objectTypeId.HasValue)
			{
				throw new Exception(JobHistoryErrorErrors.JOB_HISTORY_ERROR_NO_ARTIFACT_TYPE_FOUND);
			}

			return objectTypeId.Value;
		}
	}
}