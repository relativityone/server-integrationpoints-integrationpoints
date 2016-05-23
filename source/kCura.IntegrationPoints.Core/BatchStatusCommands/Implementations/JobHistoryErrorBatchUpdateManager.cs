using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryErrorBatchUpdateManager : IBatchStatus
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly string _uniqueJobId;
		private readonly ITempDocTableHelper _tempTableHelper;
		private ScratchTableRepository _scratchTable;
		private readonly JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private readonly int _savedSearchArtifactId;
		private readonly int _jobHistoryErrorTypeId;

		public JobHistoryErrorBatchUpdateManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory, IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory,
			int sourceWorkspaceArtifactId, string uniqueJobId, int submittedBy, JobHistoryErrorDTO.UpdateStatusType updateStatusType, int savedSearchArtifactId)
		{
			_repositoryFactory = repositoryFactory; 
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_uniqueJobId = uniqueJobId;
			_tempTableHelper = tempDocumentTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceArtifactId);
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
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START_OTHER, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;
				}
			}
			else //This runs for Run Now or Scheduled jobs
			{
				switch (_updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
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
					UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE, jobHistoryErrorRepository,
						ErrorStatusChoices.JobHistoryErrorRetried);
					jobHistoryErrorRepository.DeleteItemLevelErrorsSavedSearch(_sourceWorkspaceArtifactId, _savedSearchArtifactId, 0);
				}
				else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly ||
						_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem)
				{
					UpdateStatuses(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, jobHistoryErrorRepository,
						ErrorStatusChoices.JobHistoryErrorRetried);
				}
			}
		}

		private void UpdateStatuses(string tempTablePrefix, IJobHistoryErrorRepository jobHistoryErrorRepository, Relativity.Client.Choice errorStatus)
		{
			try
			{
				int numberOfErrors = _tempTableHelper.GetTempTableCount(tempTablePrefix);
				IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_sourceWorkspaceArtifactId);
				int errorStatusChoiceArtifactId = artifactGuidRepository.GetArtifactIdsForGuids(errorStatus.ArtifactGuids)[errorStatus.ArtifactGuids[0]];

				jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, numberOfErrors, _jobHistoryErrorTypeId,
					_sourceWorkspaceArtifactId, errorStatusChoiceArtifactId, tempTablePrefix + "_" + _uniqueJobId);
			}
			finally
			{
				_tempTableHelper.DeleteTable(tempTablePrefix);
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