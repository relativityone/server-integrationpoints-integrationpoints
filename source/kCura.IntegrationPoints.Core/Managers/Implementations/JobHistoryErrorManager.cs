using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobHistoryErrorManager : IJobHistoryErrorManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		internal JobHistoryErrorManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		/// <summary>
		/// Necessary for JobHistoryErrorManager for Batch Status changes
		/// </summary>
		internal JobHistoryErrorManager()
		{
		}

		public List<JobHistoryError> GetLastJobHistoryErrors(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactId);
			int jobHistoryArtifactId = jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId);

			return jobHistoryErrorRepository.RetreiveJobHistoryErrors(jobHistoryArtifactId);
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
