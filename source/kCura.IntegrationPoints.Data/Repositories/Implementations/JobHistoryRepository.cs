using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryRepository : RelativityMassEditBase, IJobHistoryRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		internal JobHistoryRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public int GetLastJobHistoryArtifactId(int integrationPointArtifactId)
		{
			ObjectsCondition integrationPointCondition = new ObjectsCondition(new Guid(JobHistoryFieldGuids.IntegrationPoint), ObjectsConditionEnum.AnyOfThese, new List<int>() {integrationPointArtifactId});
			DateTimeCondition notRunningCondition = new DateTimeCondition(new Guid(JobHistoryFieldGuids.EndTimeUTC), DateTimeConditionEnum.IsSet);

			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistory),
				Condition = new CompositeCondition(integrationPointCondition, CompositeConditionEnum.And, notRunningCondition),
				Fields = new List<FieldValue>()
				{
					new FieldValue(new Guid(JobHistoryFieldGuids.IntegrationPoint))
				},
				Sorts = new List<Sort>()
				{
					new Sort()
					{
						Field = JobHistoryFields.EndTimeUTC,
						Direction = SortEnum.Descending
					}
				}
			};

			QueryResultSet<RDO> results = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				results = rsapiClient.Repositories.RDO.Query(query, 1);
			}

			if (!results.Success)
			{
				throw new Exception($"Unable to retrieve Job History: {results.Message}");
			}

			int lastJobHistoryArtifactId = results.Results.Select(result => result.Artifact.ArtifactID).FirstOrDefault();
			return lastJobHistoryArtifactId;
		}

		public int[] GetStoppableJobHistoryArtifactIds(int integrationPointArtifactId)
		{
			var integrationPointCondition = new ObjectsCondition(new Guid(JobHistoryFieldGuids.IntegrationPoint), ObjectsConditionEnum.AnyOfThese, new List<int>() { integrationPointArtifactId });
			var cancelableCondition = new SingleChoiceCondition(JobHistoryFieldGuids.JobStatus, SingleChoiceConditionEnum.AnyOfThese, new [] { JobStatusChoices.JobHistoryPending.ArtifactGuids.First(), JobStatusChoices.JobHistoryProcessing.ArtifactGuids.First()});

			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistory),
				Condition = new CompositeCondition(integrationPointCondition, CompositeConditionEnum.And, cancelableCondition),
				Fields = new List<FieldValue>()
				{
					new FieldValue(new Guid(JobHistoryFieldGuids.IntegrationPoint))
				}
			};

			QueryResultSet<RDO> results = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				results = rsapiClient.Repositories.RDO.Query(query);
			}

			if (!results.Success)
			{
				throw new Exception($"Unable to retrieve Job History: {results.Message}");
			}

			int[] cancelableJobHistoryArtifactIds = results.Results.Select(result => result.Artifact.ArtifactID).ToArray();

			return cancelableJobHistoryArtifactIds;
		}
	}
}