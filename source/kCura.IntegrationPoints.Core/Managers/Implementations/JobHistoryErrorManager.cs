using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

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
	}
}
