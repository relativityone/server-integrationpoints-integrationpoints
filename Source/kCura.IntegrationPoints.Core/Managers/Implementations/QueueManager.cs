using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class QueueManager : IQueueManager
	{
		private readonly IQueueRepository _queueRepository;

		internal QueueManager(IRepositoryFactory repositoryFactory)
		{
			_queueRepository = repositoryFactory.GetQueueRepository();
		}

		public bool HasJobsExecutingOrInQueue(int workspaceId, int integrationPointId)
		{
			int numberOfJobs = _queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceId, integrationPointId);

			return numberOfJobs > 0;
		}

		public bool HasJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime)
		{
			int numberOfJobs = _queueRepository.GetNumberOfJobsExecuting(workspaceId, integrationPointId, jobId, runTime);

			return numberOfJobs > 0;
		}

        public bool HasJobsExecuting(int workspaceId, int integrationPointId)
        {
            int numberOfJobs = _queueRepository.GetNumberOfJobsLockedByAgentForIntegrationPoint(workspaceId, integrationPointId);

			return numberOfJobs > 0;
        }
    }
}
