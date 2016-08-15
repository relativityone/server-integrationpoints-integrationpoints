using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

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

		public void SetErrorStatusesToExpired(int workspaceArtifactId, int jobHistoryArtifactId)
		{
			IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(workspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(workspaceArtifactId);

			int objectTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistoryError));
			int errorStatusChoiceArtifactId = artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorExpired.ArtifactGuids)[ErrorStatusChoices.JobHistoryErrorExpired.ArtifactGuids[0]];

			SetErrorStatuses(workspaceArtifactId, jobHistoryArtifactId, objectTypeId, errorStatusChoiceArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);
			SetErrorStatuses(workspaceArtifactId, jobHistoryArtifactId, objectTypeId, errorStatusChoiceArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job);
		}

		private void SetErrorStatuses(int workspaceArtifactId, int jobHistoryArtifactId, int objectTypeId, int errorStatusChoiceArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			try
			{
				IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactId);
				using (IScratchTableRepository scratchTable = _repositoryFactory.GetScratchTableRepository(workspaceArtifactId, "StoppingRIPJob_", Guid.NewGuid().ToString()))
				{
					ICollection<int> itemLevelErrorArtifactIds = jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryArtifactId, errorType);
					scratchTable.AddArtifactIdsIntoTempTable(itemLevelErrorArtifactIds);
					jobHistoryErrorRepository.UpdateErrorStatuses(ClaimsPrincipal.Current, itemLevelErrorArtifactIds.Count, objectTypeId, errorStatusChoiceArtifactId, scratchTable.GetTempTableName());
				}
			}
			catch
			{
				// ignore failure
			}
		}
	}
}
