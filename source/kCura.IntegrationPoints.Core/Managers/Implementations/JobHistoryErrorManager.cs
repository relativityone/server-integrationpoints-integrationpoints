using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobHistoryErrorManager : IJobHistoryErrorManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		internal JobHistoryErrorManager(IRepositoryFactory repositoryFactory,
			ITempDocTableHelper helper)
		{
			JobHistoryErrorJobStart = new ScratchTableRepository(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, helper, false);
			JobHistoryErrorJobComplete = new ScratchTableRepository(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, helper, false);
			JobHistoryErrorItemStart = new ScratchTableRepository(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, helper, false);
			JobHistoryErrorItemComplete = new ScratchTableRepository(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE, helper, false);
			JobHistoryErrorItemStartOther = new ScratchTableRepository(Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START_OTHER, helper, false);

			_repositoryFactory = repositoryFactory;
		}

		public IScratchTableRepository JobHistoryErrorJobStart { get; }
		public IScratchTableRepository JobHistoryErrorJobComplete { get; }
		public IScratchTableRepository JobHistoryErrorItemStart { get; }
		public IScratchTableRepository JobHistoryErrorItemComplete { get; }
		public IScratchTableRepository JobHistoryErrorItemStartOther { get; }

		public JobHistoryErrorDTO.UpdateStatusType StageForUpdatingErrors(Job job, Relativity.Client.Choice jobType)
		{
			List<int> jobLevelErrors = GetLastJobHistoryErrorArtifactIds(job.WorkspaceID, job.RelatedObjectArtifactID, ErrorTypeChoices.JobHistoryErrorJob);
			List<int> itemLevelErrors = GetLastJobHistoryErrorArtifactIds(job.WorkspaceID, job.RelatedObjectArtifactID, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistoryErrorDTO.UpdateStatusType updateStatusType = DetermineUpdateStatusType(jobType, jobLevelErrors.Any(), itemLevelErrors.Any());

			CreateErrorListTempTables(jobLevelErrors, itemLevelErrors, updateStatusType);

			return updateStatusType;
		}

		private JobHistoryErrorDTO.UpdateStatusType DetermineUpdateStatusType(Relativity.Client.Choice jobType, bool hasJobLevelErrors, bool hasItemLevelErrors)
		{
			JobHistoryErrorDTO.UpdateStatusType updateStatusType = new JobHistoryErrorDTO.UpdateStatusType();

			if (jobType.Name == JobTypeChoices.JobHistoryRetryErrors.Name)
			{
				updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			}
			else
			{
				updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			}

			if (hasJobLevelErrors && hasItemLevelErrors)
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;
			}
			else if (hasJobLevelErrors)
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;
			}
			else if (hasItemLevelErrors)
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;
			}
			else
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;
			}

			return updateStatusType;
		}

		private void CreateErrorListTempTables(List<int> jobLevelErrors, List<int> itemLevelErrors, JobHistoryErrorDTO.UpdateStatusType updateStatusType)
		{
			try
			{
				if (updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
				{
					switch (updateStatusType.ErrorTypes)
					{
						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
							JobHistoryErrorJobStart.AddArtifactIdsIntoTempTable(jobLevelErrors);
							JobHistoryErrorJobComplete.AddArtifactIdsIntoTempTable(jobLevelErrors);
							JobHistoryErrorItemStart.AddArtifactIdsIntoTempTable(itemLevelErrors);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
							JobHistoryErrorJobStart.AddArtifactIdsIntoTempTable(jobLevelErrors);
							JobHistoryErrorJobComplete.AddArtifactIdsIntoTempTable(jobLevelErrors);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						// This action will be performed later on. Item level errors need to be cross referenced before staging can occur.
							break;
					}
				}
				else //Runs for Run Now or Scheduled jobs
				{
					switch (updateStatusType.ErrorTypes)
					{
						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
							JobHistoryErrorJobStart.AddArtifactIdsIntoTempTable(jobLevelErrors);
							JobHistoryErrorItemStart.AddArtifactIdsIntoTempTable(itemLevelErrors);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
							JobHistoryErrorJobStart.AddArtifactIdsIntoTempTable(jobLevelErrors);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
							JobHistoryErrorItemStart.AddArtifactIdsIntoTempTable(itemLevelErrors);
							break;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(JobHistoryErrorErrors.JOB_HISTORY_ERROR_TEMP_TABLE_CREATION_FAILURE, ex);
			}
		}

		public int CreateItemLevelErrorsSavedSearch(Job job, int originalSavedSearchArtifactId)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(job.WorkspaceID);
			int lastJobHistoryArtifactId = GetLastJobHistory(job.WorkspaceID, job.RelatedObjectArtifactID);

			return jobHistoryErrorRepository.CreateItemLevelErrorsSavedSearch(job.WorkspaceID, job.RelatedObjectArtifactID,
				originalSavedSearchArtifactId, lastJobHistoryArtifactId, job.SubmittedBy);
		}

		private List<int> GetLastJobHistoryErrorArtifactIds(int workspaceArtifactId, int integrationPointArtifactId, Relativity.Client.Choice errorType)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactId);
			int lastJobHistoryArtifactId = GetLastJobHistory(workspaceArtifactId, integrationPointArtifactId);

			return jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(lastJobHistoryArtifactId, errorType);
		}

		private int GetLastJobHistory(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);

			return jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId);
		}

		public void CreateErrorListTempTablesForItemLevelErrors(Job job, string uniqueJobId, int savedSearchIdForItemLevelError)
		{
			var currentItemLevelErrors = new List<int>();
			var expiredItemLevelErrors = new List<int>();

			List<ArtifactDTO> documentsFromSavedSearch = GetAllFromSavedSearch(job.WorkspaceID, savedSearchIdForItemLevelError);

			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(job.WorkspaceID);
			int lastJobHistoryId = GetLastJobHistory(job.WorkspaceID, job.RelatedObjectArtifactID);
			Dictionary<int, string> itemLevelErrorsAndSourceUniqueIds = jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, ErrorTypeChoices.JobHistoryErrorItem);

			foreach (var error in itemLevelErrorsAndSourceUniqueIds)
			{
				if (documentsFromSavedSearch.Exists(document => document.TextIdentifier == error.Value))
				{
					currentItemLevelErrors.Add(error.Key);
				}
				else
				{
					expiredItemLevelErrors.Add(error.Key);
				}
			}

			JobHistoryErrorItemStart.AddArtifactIdsIntoTempTable(currentItemLevelErrors);
			JobHistoryErrorItemComplete.AddArtifactIdsIntoTempTable(currentItemLevelErrors);

			if (expiredItemLevelErrors.Count > 0)
			{
				JobHistoryErrorItemStartOther.AddArtifactIdsIntoTempTable(expiredItemLevelErrors);
			}
		}

		private List<ArtifactDTO> GetAllFromSavedSearch(int workspaceArtifactId, int savedSearchId)
		{
			var allArtifacts = new List<ArtifactDTO>();
			ISavedSearchRepository savedSearchRepository = _repositoryFactory.GetSavedSearchRepository(workspaceArtifactId, savedSearchId);

			while (true)
			{
				ArtifactDTO[] artifactDtos = savedSearchRepository.RetrieveNextDocuments();
				if (artifactDtos != null && artifactDtos.Any())
				{
					allArtifacts.AddRange(artifactDtos);
				}
				else
				{
					break;
				}
			}

			return allArtifacts;
		}
	}
}
