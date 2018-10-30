using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryErrorBatchUpdateManager : IBatchStatus
	{
		private readonly IJobHistoryErrorManager _jobHistoryErrorManager;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IJobStopManager _jobStopManager;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private readonly int _jobHistoryErrorTypeId;
		private readonly IAPILog _logger;

		public JobHistoryErrorBatchUpdateManager(IJobHistoryErrorManager jobHistoryErrorManager, 
			IHelper helper,
			IRepositoryFactory repositoryFactory, 
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, IJobStopManager jobStopManager, int sourceWorkspaceArtifactId, int submittedBy, 
			JobHistoryErrorDTO.UpdateStatusType updateStatusType)
		{
			_jobHistoryErrorManager = jobHistoryErrorManager;
			_repositoryFactory = repositoryFactory;
			_jobStopManager = jobStopManager;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_updateStatusType = updateStatusType;
			_jobHistoryErrorTypeId = SetJobHistoryErrorArtifactTypeId(_sourceWorkspaceArtifactId);
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<JobHistoryErrorBatchUpdateManager>();

		}

		public void OnJobStart(Job job)
		{
			try
			{
				_logger.LogDebug("JobHistoryErrorBatchUpdateManager OnJobStart. Workspace: {workspaceId}, jobType: {jobType}, errorTypes: {errorType}",
					_sourceWorkspaceArtifactId, _updateStatusType.JobType, _updateStatusType.ErrorTypes);
				IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactId);
				
				if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
				{

					switch (_updateStatusType.ErrorTypes)
					{
						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
							_jobHistoryErrorManager.JobHistoryErrorJobComplete = _jobHistoryErrorManager.JobHistoryErrorJobStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
							_jobHistoryErrorManager.JobHistoryErrorJobComplete = _jobHistoryErrorManager.JobHistoryErrorJobStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
							_jobHistoryErrorManager.JobHistoryErrorItemComplete = _jobHistoryErrorManager.JobHistoryErrorItemStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStartExcluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
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
			catch (Exception ex)
			{
				throw LogJobStartSaveJobHistoryException(job, ex);
			}
		}

		public void OnJobComplete(Job job)
		{
			try
			{
				_logger.LogDebug("JobHistoryErrorBatchUpdateManager OnJobComplete. Workspace: {workspaceId}, jobType: {jobType}, errorTypes: {errorType}",
					_sourceWorkspaceArtifactId, _updateStatusType.JobType, _updateStatusType?.ErrorTypes);
				IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactId);

				if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
				{
					if (_jobStopManager.IsStopRequested())
					{
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemComplete, jobHistoryErrorRepository,
							ErrorStatusChoices.JobHistoryErrorExpired);
					}
					else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly)
					{
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemComplete, jobHistoryErrorRepository,
							ErrorStatusChoices.JobHistoryErrorRetried);
					}
					else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly ||
							_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem)
					{
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobComplete, jobHistoryErrorRepository,
							ErrorStatusChoices.JobHistoryErrorRetried);
					}
				}

			}
			catch (Exception ex)
			{
				throw LogJobCompleteSaveJobHistoryException(job, ex);
			}
		}
		private IntegrationPointsException LogJobCompleteSaveJobHistoryException(Job job, Exception exception)
		{
			string message = "Error while updating Job History after job completion. Cannot perform save history information on job";
			_logger.LogError("Error while updating Job History after job completion. Cannot perform save history information on job: {@job}.", job);
			return new IntegrationPointsException(message, exception);
		}


		private IntegrationPointsException LogJobStartSaveJobHistoryException(Job job, Exception exception)
		{
			string message = "Error while updating Job History before job start. Cannot perform save history information on job.";
			_logger.LogError("Error while updating Job History before job start. Cannot perform save history information on job: {@job}.", job);
			return new IntegrationPointsException(message, exception);
		}

		private void UpdateStatuses(IScratchTableRepository scratchTable, IJobHistoryErrorRepository jobHistoryErrorRepository, Relativity.Client.DTOs.Choice errorStatus)
		{
			try
			{
				int numberOfErrors = scratchTable.Count;
				IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_sourceWorkspaceArtifactId);
				int errorStatusChoiceArtifactId = artifactGuidRepository.GetArtifactIdsForGuids(errorStatus.Guids)[errorStatus.Guids[0]];

				jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, numberOfErrors, _jobHistoryErrorTypeId, 
					errorStatusChoiceArtifactId, scratchTable.GetTempTableName());
			}
			finally
			{
				scratchTable.Dispose();
			}
		}

		private int SetJobHistoryErrorArtifactTypeId(int workspaceArtifactId)
		{
			IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(workspaceArtifactId);
			int objectTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistoryError));

			return objectTypeId;
		}
	}
}