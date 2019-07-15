﻿using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;
using Sort = Relativity.Services.Objects.DataContracts.Sort;
using SortEnum = Relativity.Services.Objects.DataContracts.SortEnum;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		private readonly IRelativityObjectManager _relativityObjectManager;

		internal JobHistoryRepository(IRelativityObjectManager relativityObjectManager)
		{
			_relativityObjectManager = relativityObjectManager;
		}

		public int GetLastJobHistoryArtifactId(int integrationPointArtifactId)
		{
			string integrationPointCondition = CreateIntegrationPointCondition(integrationPointArtifactId);
			string notRunningCondition = $"('{JobHistoryFields.EndTimeUTC}' ISSET)";
			string condition = $"{integrationPointCondition} AND {notRunningCondition}";

			var queryRequest = new QueryRequest
			{
				Condition = condition,
				Fields = new[] { new FieldRef { Guid = Guid.Parse(JobHistoryFieldGuids.IntegrationPoint) } },
				Sorts = new List<Sort>
				{
					new Sort
					{
						Direction = SortEnum.Descending,
						FieldIdentifier = new FieldRef { Guid = Guid.Parse(JobHistoryFieldGuids.EndTimeUTC)}
					}
				}
			};

			IEnumerable<JobHistory> result = _relativityObjectManager.Query<JobHistory>(queryRequest, 0, 1).Items;
			return result.Select(x => x.ArtifactId).FirstOrDefault();
		}

		public IDictionary<Guid, int[]> GetStoppableJobHistoryArtifactIdsByStatus(int integrationPointArtifactId)
		{
			List<JobHistory> jobHistories = GetStoppableJobHistoriesForIntegrationPoint(integrationPointArtifactId);

			var pendingJobArtifactIds = new List<int>();
			var processingJobArtifactIds = new List<int>();

			foreach (JobHistory jobHistory in jobHistories)
			{
				string statusName = jobHistory.JobStatus?.Name;
				if (statusName == JobStatusChoices.JobHistoryPending.Name)
				{
					pendingJobArtifactIds.Add(jobHistory.ArtifactId);
				}
				else if (statusName == JobStatusChoices.JobHistoryProcessing.Name)
				{
					processingJobArtifactIds.Add(jobHistory.ArtifactId);
				}
			}

			return new Dictionary<Guid, int[]>
			{
				[JobStatusChoices.JobHistoryPending.Guids.First()] = pendingJobArtifactIds.ToArray(),
				[JobStatusChoices.JobHistoryProcessing.Guids.First()] = processingJobArtifactIds.ToArray()
			};
		}

		public string GetJobHistoryName(int jobHistoryArtifactId)
		{
			IEnumerable<Guid> fieldsToRetrieve = new[] { Guid.Parse(JobHistoryFieldGuids.Name) };
			JobHistory jobHistory = _relativityObjectManager.Read<JobHistory>(jobHistoryArtifactId, fieldsToRetrieve);
			return jobHistory.Name;
		}

		private List<JobHistory> GetStoppableJobHistoriesForIntegrationPoint(int integrationPointArtifactId)
		{
			string integrationPointCondition = CreateIntegrationPointCondition(integrationPointArtifactId);
			string stoppableCondition = CreateStoppableCondition();

			var queryRequest = new QueryRequest
			{
				Condition = $"{integrationPointCondition} AND {stoppableCondition}",
				Fields = new[] { new FieldRef { Guid = Guid.Parse(JobHistoryFieldGuids.JobStatus) } }
			};

			return _relativityObjectManager.Query<JobHistory>(queryRequest);
		}

		private string CreateIntegrationPointCondition(int integrationPointArtifactId)
		{
			return $"('{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{integrationPointArtifactId}])";
		}

		private string CreateStoppableCondition()
		{
			Guid pendingGuid = JobStatusChoices.JobHistoryPending.Guids.First();
			Guid processingGuid = JobStatusChoices.JobHistoryProcessing.Guids.First();
			return $"('{JobHistoryFields.JobStatus}' IN CHOICE [{pendingGuid}, {processingGuid}])";
		}
	}
}