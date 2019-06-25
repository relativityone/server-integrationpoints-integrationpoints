using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryErrorBatchUpdateManager : IBatchStatus
	{
		private readonly IJobHistoryErrorManager _jobHistoryErrorManager;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IJobStopManager _jobStopManager;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private readonly IAPILog _logger;
		private readonly IMassUpdateHelper _massUpdateHelper;

		public JobHistoryErrorBatchUpdateManager(
			IJobHistoryErrorManager jobHistoryErrorManager,
			IAPILog logger,
			IRepositoryFactory repositoryFactory,
			IJobStopManager jobStopManager,
			int sourceWorkspaceArtifactId,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType,
			IMassUpdateHelper massUpdateHelper)
		{
			_jobHistoryErrorManager = jobHistoryErrorManager;
			_repositoryFactory = repositoryFactory;
			_jobStopManager = jobStopManager;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_updateStatusType = updateStatusType;
			_logger = logger.ForContext<JobHistoryErrorBatchUpdateManager>();
			_massUpdateHelper = massUpdateHelper;
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
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgressGuid);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpiredGuid);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
							_jobHistoryErrorManager.JobHistoryErrorJobComplete = _jobHistoryErrorManager.JobHistoryErrorJobStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgressGuid);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
							_jobHistoryErrorManager.JobHistoryErrorItemComplete = _jobHistoryErrorManager.JobHistoryErrorItemStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgressGuid);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStartExcluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpiredGuid);
							break;
					}
				}
				else //This runs for Run Now or Scheduled jobs
				{
					switch (_updateStatusType.ErrorTypes)
					{
						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpiredGuid);
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpiredGuid);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpiredGuid);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
							UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemStart, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpiredGuid);
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
						UpdateStatuses(
							_jobHistoryErrorManager.JobHistoryErrorItemComplete, 
							jobHistoryErrorRepository,
							ErrorStatusChoices.JobHistoryErrorExpiredGuid);
					}
					else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly)
					{
						UpdateStatuses(
							_jobHistoryErrorManager.JobHistoryErrorItemComplete, 
							jobHistoryErrorRepository,
							ErrorStatusChoices.JobHistoryErrorRetriedGuid);
					}
					else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly ||
							_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem)
					{
						UpdateStatuses(
							_jobHistoryErrorManager.JobHistoryErrorJobComplete, 
							jobHistoryErrorRepository,
							ErrorStatusChoices.JobHistoryErrorRetriedGuid);
					}
				}
			}
			catch (Exception ex)
			{
				throw LogJobCompleteSaveJobHistoryException(job, ex);
			}
		}

		private void UpdateStatuses(
			IScratchTableRepository scratchTable,
			IMassUpdateRepository massUpdateRepository,
			Guid errorStatusChoiceValueGuid)
		{
			TryUpdateStatusesAsync(
					scratchTable,
					massUpdateRepository,
					errorStatusChoiceValueGuid)
				.GetAwaiter()
				.GetResult();
		}

		private async Task TryUpdateStatusesAsync(
			IScratchTableRepository scratchTable,
			IMassUpdateRepository massUpdateRepository,
			Guid errorStatusChoiceValueGuid)
		{
			try
			{
				FieldUpdateRequestDto[] fieldsToUpdate =
				{
					new FieldUpdateRequestDto(
						JobHistoryErrorFieldGuids.ErrorStatusGuid,
						new SingleChoiceReferenceDto(errorStatusChoiceValueGuid))
				};

				await _massUpdateHelper
					.UpdateArtifactsAsync(
						scratchTable,
						fieldsToUpdate,
						massUpdateRepository)
					.ConfigureAwait(false);
			}
			finally
			{
				scratchTable.Dispose();
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
	}
}