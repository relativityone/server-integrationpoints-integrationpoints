using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
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

		public Models.StoppableJobCollection GetStoppableJobCollection(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
			IDictionary<Guid, int[]> stoppableJobStatusDictionary = jobHistoryRepository.GetStoppableJobHistoryArtifactIdsByStatus(integrationPointArtifactId);
			Guid pendingGuid = JobStatusChoices.JobHistoryPending.ArtifactGuids.First();
			Guid processingGuid = JobStatusChoices.JobHistoryProcessing.ArtifactGuids.First();

			int[] pendingJobArtifactIds;
			int[] processingJobArtifactIds;
			stoppableJobStatusDictionary.TryGetValue(pendingGuid, out pendingJobArtifactIds);
			stoppableJobStatusDictionary.TryGetValue(processingGuid, out processingJobArtifactIds);

			var stoppableJobCollection = new Models.StoppableJobCollection()
			{
				PendingJobArtifactIds = pendingJobArtifactIds ?? new int[0],
				ProcessingJobArtifactIds = processingJobArtifactIds ?? new int[0]
			};

			return stoppableJobCollection;
		}
	}
}
