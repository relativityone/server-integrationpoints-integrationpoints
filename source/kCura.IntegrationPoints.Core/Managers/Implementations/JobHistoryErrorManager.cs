using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobHistoryErrorManager : IJobHistoryErrorManager
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private const int _batchSize = 1000;

		internal JobHistoryErrorManager(IRepositoryFactory repositoryFactory, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			JobHistoryErrorJobStart = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
			JobHistoryErrorJobComplete = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, uniqueJobId);
			JobHistoryErrorItemStart = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
			JobHistoryErrorItemComplete = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE, uniqueJobId);
			JobHistoryErrorItemStartExcluded = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START_EXCLUDED, uniqueJobId);

			_repositoryFactory = repositoryFactory;
		}

		public IScratchTableRepository JobHistoryErrorJobStart { get; }
		public IScratchTableRepository JobHistoryErrorJobComplete { get; }
		public IScratchTableRepository JobHistoryErrorItemStart { get; }
		public IScratchTableRepository JobHistoryErrorItemComplete { get; }
		public IScratchTableRepository JobHistoryErrorItemStartExcluded { get; }

		public JobHistoryErrorDTO.UpdateStatusType StageForUpdatingErrors(Job job, Relativity.Client.Choice jobType)
		{
			IList<int> jobLevelErrors = GetLastJobHistoryErrorArtifactIds(job.WorkspaceID, job.RelatedObjectArtifactID, JobHistoryErrorDTO.Choices.ErrorType.Values.Job);
			IList<int> itemLevelErrors = GetLastJobHistoryErrorArtifactIds(job.WorkspaceID, job.RelatedObjectArtifactID, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);

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

		private void CreateErrorListTempTables(IList<int> jobLevelErrors, IList<int> itemLevelErrors, JobHistoryErrorDTO.UpdateStatusType updateStatusType)
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
							JobHistoryErrorItemStart.BatchAddArtifactIdsIntoTempTable(itemLevelErrors, _batchSize);
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
							JobHistoryErrorItemStart.BatchAddArtifactIdsIntoTempTable(itemLevelErrors, _batchSize);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
							JobHistoryErrorJobStart.AddArtifactIdsIntoTempTable(jobLevelErrors);
							break;

						case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
							JobHistoryErrorItemStart.BatchAddArtifactIdsIntoTempTable(itemLevelErrors, _batchSize);
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

			return jobHistoryErrorRepository.CreateItemLevelErrorsSavedSearch(job.RelatedObjectArtifactID,
				originalSavedSearchArtifactId, lastJobHistoryArtifactId);
		}

		private IList<int> GetLastJobHistoryErrorArtifactIds(int workspaceArtifactId, int integrationPointArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
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

		public void CreateErrorListTempTablesForItemLevelErrors(Job job, int savedSearchIdForItemLevelError)
		{
			var currentItemLevelErrors = new HashSet<int>();
			var expiredItemLevelErrors = new HashSet<int>();

			IJobHistoryErrorRepository jobHistoryErrorRepository =
				_repositoryFactory.GetJobHistoryErrorRepository(job.WorkspaceID);
			int lastJobHistoryId = GetLastJobHistory(job.WorkspaceID, job.RelatedObjectArtifactID);
			IDictionary<int, string> itemLevelErrorsAndSourceUniqueIds =
				jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId,
					JobHistoryErrorDTO.Choices.ErrorType.Values.Item);

			ISavedSearchRepository savedSearchRepository = _repositoryFactory.GetSavedSearchRepository(job.WorkspaceID, savedSearchIdForItemLevelError);
			while (!savedSearchRepository.AllDocumentsRetrieved())
			{
				var documentIdentifiersFromSavedSearch = new HashSet<string>();
				ArtifactDTO[] artifactDtos = savedSearchRepository.RetrieveNextDocuments();

				if (artifactDtos != null && artifactDtos.Any())
				{
					documentIdentifiersFromSavedSearch = new HashSet<string>(artifactDtos.Select(x => x.TextIdentifier));
				}

				foreach (var error in itemLevelErrorsAndSourceUniqueIds)
				{
					if (documentIdentifiersFromSavedSearch.Contains(error.Value))
					{
						currentItemLevelErrors.Add(error.Key);
						expiredItemLevelErrors.Remove(error.Key);
					}
					else if (!currentItemLevelErrors.Contains(error.Key))
					{
						expiredItemLevelErrors.Add(error.Key);
					}
				}
			}

			if (currentItemLevelErrors.Any())
			{
				IList<int> currentItemLevelErrorList = currentItemLevelErrors.ToList();
				// TODO: if BatchAddArtifactIdsIntoTempTable took an ICollection instead of IList, 
				// we wouldn't need to convert from the HashTable. -- biedrzycki: 6/1/2016
				JobHistoryErrorItemStart.BatchAddArtifactIdsIntoTempTable(currentItemLevelErrorList, _batchSize);
				JobHistoryErrorItemComplete.BatchAddArtifactIdsIntoTempTable(currentItemLevelErrorList, _batchSize);
			}

			if (expiredItemLevelErrors.Any())
			{
				JobHistoryErrorItemStartExcluded.BatchAddArtifactIdsIntoTempTable(expiredItemLevelErrors.ToList(), _batchSize);
			}
		}
	}
}
