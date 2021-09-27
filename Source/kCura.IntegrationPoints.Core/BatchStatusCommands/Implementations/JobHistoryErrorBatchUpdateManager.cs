using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryErrorBatchUpdateManager : IBatchStatus
	{
		private readonly IJobHistoryErrorManager _jobHistoryErrorManager;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IJobStopManager _jobStopManager;
		private readonly int _sourceWorkspaceArtifactID;
		private readonly JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private readonly IAPILog _logger;
		private readonly IMassUpdateHelper _massUpdateHelper;

		public JobHistoryErrorBatchUpdateManager(
			IJobHistoryErrorManager jobHistoryErrorManager,
			IAPILog logger,
			IRepositoryFactory repositoryFactory,
			IJobStopManager jobStopManager,
			int sourceWorkspaceArtifactID,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType,
			IMassUpdateHelper massUpdateHelper)
		{
			_jobHistoryErrorManager = jobHistoryErrorManager;
			_repositoryFactory = repositoryFactory;
			_jobStopManager = jobStopManager;
			_sourceWorkspaceArtifactID = sourceWorkspaceArtifactID;
			_updateStatusType = updateStatusType;
			_massUpdateHelper = massUpdateHelper;

			_logger = (logger ?? throw new ArgumentNullException(nameof(logger)))
				.ForContext<JobHistoryErrorBatchUpdateManager>();
		}

		private bool IsRetryErrorsJob =>
			_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;

		public void OnJobStart(Job job)
		{
			try
			{
				_logger.LogDebug("JobHistoryErrorBatchUpdateManager OnJobStart. Workspace: {workspaceId}, jobType: {jobType}, errorTypes: {errorType}",
					_sourceWorkspaceArtifactID, _updateStatusType.JobType, _updateStatusType.ErrorTypes);
				IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactID);

				if (IsRetryErrorsJob)
				{
					UpdateScratchTablesOnStart();
				}

				IEnumerable<UpdateErrorStatusData> updateErrorStatusesData = IsRetryErrorsJob
					? GetUpdateStatusesOnStartForRetryErrorsJob()
					: GetUpdateStatusesOnStartForNonRetryJob();
				UpdateStatusesAsync(updateErrorStatusesData, jobHistoryErrorRepository).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				throw GetAndLogJobStartSaveJobHistoryException(job, ex);
			}
		}

		public void OnJobComplete(Job job)
		{
			try
			{
				_logger.LogDebug("JobHistoryErrorBatchUpdateManager OnJobComplete. Workspace: {workspaceId}, jobType: {jobType}, errorTypes: {errorType}",
					_sourceWorkspaceArtifactID,
					_updateStatusType.JobType,
					_updateStatusType?.ErrorTypes);

				if (!IsRetryErrorsJob)
				{
					return;
				}

				IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactID);
				IEnumerable<UpdateErrorStatusData> updateErrorStatusesData = GetUpdateStatusDataForRetryErrorsJobComplete();
				UpdateStatusesAsync(updateErrorStatusesData, jobHistoryErrorRepository).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				throw GetAndLogJobCompleteSaveJobHistoryException(job, ex);
			}
		}

		private void UpdateScratchTablesOnStart()
		{
			switch (_updateStatusType.ErrorTypes)
			{
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
					_jobHistoryErrorManager.JobHistoryErrorJobComplete = _jobHistoryErrorManager.JobHistoryErrorJobStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE);
					break;
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
					_jobHistoryErrorManager.JobHistoryErrorJobComplete = _jobHistoryErrorManager.JobHistoryErrorJobStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE);
					break;
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
					_jobHistoryErrorManager.JobHistoryErrorItemComplete = _jobHistoryErrorManager.JobHistoryErrorItemStart.CopyTempTable(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE);
					break;
			}
		}

		private IEnumerable<UpdateErrorStatusData> GetUpdateStatusesOnStartForNonRetryJob()
		{
			switch (_updateStatusType.ErrorTypes)
			{
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorJobStart,
						ErrorStatusChoices.JobHistoryErrorExpiredGuid);
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorItemStart,
						ErrorStatusChoices.JobHistoryErrorExpiredGuid);
					break;
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorJobStart,
						ErrorStatusChoices.JobHistoryErrorExpiredGuid);
					break;
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorItemStart,
						ErrorStatusChoices.JobHistoryErrorExpiredGuid);
					break;
			}
		}

		private IEnumerable<UpdateErrorStatusData> GetUpdateStatusesOnStartForRetryErrorsJob()
		{
			switch (_updateStatusType.ErrorTypes)
			{
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorJobStart,
						ErrorStatusChoices.JobHistoryErrorInProgressGuid);
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorItemStart,
						ErrorStatusChoices.JobHistoryErrorExpiredGuid);
					break;
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorJobStart,
						ErrorStatusChoices.JobHistoryErrorInProgressGuid);
					break;
				case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorItemStart,
						ErrorStatusChoices.JobHistoryErrorInProgressGuid);
					yield return new UpdateErrorStatusData(
						_jobHistoryErrorManager.JobHistoryErrorItemStartExcluded,
						ErrorStatusChoices.JobHistoryErrorExpiredGuid);
					break;
			}
		}

		private IEnumerable<UpdateErrorStatusData> GetUpdateStatusDataForRetryErrorsJobComplete()
		{
			if (_jobStopManager.IsStopRequested())
			{
				yield return new UpdateErrorStatusData(
					_jobHistoryErrorManager.JobHistoryErrorItemComplete,
					ErrorStatusChoices.JobHistoryErrorExpiredGuid);
			}
			else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly)
			{
				yield return new UpdateErrorStatusData(
					_jobHistoryErrorManager.JobHistoryErrorItemComplete,
					ErrorStatusChoices.JobHistoryErrorRetriedGuid);
			}
			else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly ||
					_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem)
			{
				yield return new UpdateErrorStatusData(
					_jobHistoryErrorManager.JobHistoryErrorJobComplete,
					ErrorStatusChoices.JobHistoryErrorRetriedGuid);
			}
		}

		private async Task UpdateStatusesAsync(
			IEnumerable<UpdateErrorStatusData> updateErrorStatusesData,
			IRepositoryWithMassUpdate repositoryWithMassUpdate)
		{
			foreach (UpdateErrorStatusData updateErrorStatusData in updateErrorStatusesData)
			{
				await UpdateStatusesAsync(
						updateErrorStatusData.ScratchTableRepository,
						repositoryWithMassUpdate,
						updateErrorStatusData.ErrorStatusValue)
					.ConfigureAwait(false);
			}
		}

		private async Task UpdateStatusesAsync(
			IScratchTableRepository scratchTable,
			IRepositoryWithMassUpdate massUpdateRepository,
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

		private IntegrationPointsException GetAndLogJobCompleteSaveJobHistoryException(Job job, Exception exception)
		{
			const string message = "Error while updating Job History after job completion. Cannot perform save history information on job";
			return GetAndLogException(job, exception, message);
		}

		private IntegrationPointsException GetAndLogJobStartSaveJobHistoryException(Job job, Exception exception)
		{
			const string message = "Error while updating Job History before job start. Cannot perform save history information on job.";
			return GetAndLogException(job, exception, message);
		}

		private IntegrationPointsException GetAndLogException(
			Job job,
			Exception exception,
			string message)
		{
			_logger.LogError($"{message}: {{@job}}.", job);
			return new IntegrationPointsException(message, exception);
		}

		private struct UpdateErrorStatusData
		{
			public IScratchTableRepository ScratchTableRepository { get; }
			public Guid ErrorStatusValue { get; }

			public UpdateErrorStatusData(IScratchTableRepository scratchTableRepository, Guid errorStatusValue)
			{
				ScratchTableRepository = scratchTableRepository;
				ErrorStatusValue = errorStatusValue;
			}
		}
	}
}