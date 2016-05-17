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

		internal JobHistoryErrorManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public JobHistoryErrorDTO.UpdateStatusType StageForUpdatingErrors(Job job, Relativity.Client.Choice jobType, string uniqueJobId)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(job.WorkspaceID);
			List<int> jobLevelErrors = GetLastJobHistoryErrorArtifactIds(job.WorkspaceID, job.RelatedObjectArtifactID, ErrorTypeChoices.JobHistoryErrorJob);
			List<int> itemLevelErrors = GetLastJobHistoryErrorArtifactIds(job.WorkspaceID, job.RelatedObjectArtifactID, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistoryErrorDTO.UpdateStatusType updateStatusType = jobHistoryErrorRepository.DetermineUpdateStatusType(jobType, jobLevelErrors.Any(), itemLevelErrors.Any());

			CreateErrorListTempTables(jobHistoryErrorRepository, jobLevelErrors, itemLevelErrors, updateStatusType, uniqueJobId);

			return updateStatusType;
		}

		private void CreateErrorListTempTables(IJobHistoryErrorRepository jobHistoryErrorRepository, List<int> jobLevelErrors, List<int> itemLevelErrors, JobHistoryErrorDTO.UpdateStatusType updateStatusType, string uniqueJobId)
		{
			if (updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
			{
				switch (updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						jobHistoryErrorRepository.CreateErrorListTempTable(jobLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						jobHistoryErrorRepository.CreateErrorListTempTable(jobLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, uniqueJobId);
						jobHistoryErrorRepository.CreateErrorListTempTable(itemLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						jobHistoryErrorRepository.CreateErrorListTempTable(jobLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						jobHistoryErrorRepository.CreateErrorListTempTable(jobLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						jobHistoryErrorRepository.CreateErrorListTempTable(itemLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						//ToDo: Second CreateErrorListTempTable needed when logic to split item level errors between those being retried and those no longer included is written
						jobHistoryErrorRepository.CreateErrorListTempTable(itemLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE, uniqueJobId);
						break;
				}
			}
			else
			{
				switch (updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						jobHistoryErrorRepository.CreateErrorListTempTable(jobLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						jobHistoryErrorRepository.CreateErrorListTempTable(itemLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						jobHistoryErrorRepository.CreateErrorListTempTable(jobLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						jobHistoryErrorRepository.CreateErrorListTempTable(itemLevelErrors, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						break;
				}
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
	}
}
