using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
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
		private readonly int _jobHistoryErrorTypeId;

		public JobHistoryErrorBatchUpdateManager(IJobHistoryErrorManager jobHistoryErrorManager, IRepositoryFactory repositoryFactory, 
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, int sourceWorkspaceArtifactId, int submittedBy, 
			JobHistoryErrorDTO.UpdateStatusType updateStatusType)
		{
			_jobHistoryErrorManager = jobHistoryErrorManager;
			_repositoryFactory = repositoryFactory; 
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_updateStatusType = updateStatusType;
			_jobHistoryErrorTypeId = SetJobHistoryErrorArtifactTypeId(_sourceWorkspaceArtifactId);
		}

		public void OnJobStart(Job job)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactId);

			kCura.Method.Injection.InjectionManager.Instance.Evaluate("A876A7F9-A9F8-445C-9A01-FCB0C7FD4E8B");

			if (_updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
			{
				switch (_updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobError, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress, false);
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemErrorsIncluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobError, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress, false);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemErrorsIncluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorInProgress, false);
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemErrorsExcluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;
				}
			}
			else //This runs for Run Now or Scheduled jobs
			{
				switch (_updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobError, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemErrorsIncluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobError, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemErrorsIncluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorExpired);
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
					UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorItemErrorsIncluded, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorRetried);
				}
				else if (_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly ||
						_updateStatusType.ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem)
				{
					UpdateStatuses(_jobHistoryErrorManager.JobHistoryErrorJobError, jobHistoryErrorRepository, ErrorStatusChoices.JobHistoryErrorRetried);
				}
			}
		}

		private void UpdateStatuses(IScratchTableRepository scratchTable, IJobHistoryErrorRepository jobHistoryErrorRepository, Relativity.Client.Choice errorStatus, bool disposeTempTable = true)
		{
			try
			{
				int numberOfErrors = scratchTable.Count;
				IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_sourceWorkspaceArtifactId);
				int errorStatusChoiceArtifactId = artifactGuidRepository.GetArtifactIdsForGuids(errorStatus.ArtifactGuids)[errorStatus.ArtifactGuids[0]];

				jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, numberOfErrors, _jobHistoryErrorTypeId, 
					errorStatusChoiceArtifactId, scratchTable.GetTempTableName());
			}
			finally
			{
				kCura.Method.Injection.InjectionManager.Instance.Evaluate("C2B46E70-20EF-4A08-8BCF-9A15274ECC55");

				if (disposeTempTable)
				{
					scratchTable.Dispose();
				}
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