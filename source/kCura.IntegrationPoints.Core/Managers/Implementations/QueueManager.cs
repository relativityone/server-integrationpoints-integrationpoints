using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class QueueManager : IQueueManager
	{
		private readonly IQueueRepository _queueRepository;

		public QueueManager(IContextContainer contextContainer)
			: this(new RepositoryFactory(contextContainer.Helper))
		{
		}

		/// <summary>
		/// Only consumed by unit tests
		/// </summary>
		internal QueueManager(IRepositoryFactory repositoryFactory)
		{
			_queueRepository = repositoryFactory.GetQueueRepository();
		}

		public bool HasJobsExecutingOrInQueue(int workspaceId, int integrationPointId)
		{
			int numberOfJobs = _queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceId, integrationPointId);

			if (numberOfJobs > 0)
			{
				return true;
			}
			return false;
		}

		public bool HasJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime)
		{
			int numberOfJobs = _queueRepository.GetNumberOfJobsExecuting(workspaceId, integrationPointId, jobId, runTime);

			if (numberOfJobs > 0)
			{
				return true;
			}
			return false;
		}
	}
}
