using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobHistoryManager : IJobHistoryManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		internal JobHistoryManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public int GetLastJobHistoryArtifactId(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
			return jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId);
		}
	}
}
