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

			jobHistoryErrorRepository.CreateErrorListTempTables(jobLevelErrors, itemLevelErrors, updateStatusType, uniqueJobId);

			return updateStatusType;
		}

		private List<int> GetLastJobHistoryErrorArtifactIds(int workspaceArtifactId, int integrationPointArtifactId, Relativity.Client.Choice errorType)
		{
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactId);

			int jobHistoryArtifactId = jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId);

			return jobHistoryErrorRepository.RetreiveJobHistoryErrorArtifactIds(jobHistoryArtifactId, errorType);
		}

		public int CreateItemLevelErrorsSavedSearch(int workspaceArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactId);
			int itemLevelSavedSearch = jobHistoryErrorRepository.CreateItemLevelErrorsSavedSearch(workspaceArtifactId,
				savedSearchArtifactId, jobHistoryArtifactId);
			return itemLevelSavedSearch;
		}
	}
}
