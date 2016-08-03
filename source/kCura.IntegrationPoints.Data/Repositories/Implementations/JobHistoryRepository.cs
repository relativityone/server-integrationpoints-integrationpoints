using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryRepository : RelativityMassEditBase, IJobHistoryRepository
	{
		private readonly IHelper _helper;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly int _workspaceArtifactId;
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IArtifactGuidRepository _artifactGuidRepository;

		internal JobHistoryRepository(IHelper helper, IRepositoryFactory repositoryFactory, int workspaceArtifactId)
		{
			_helper = helper;
			_repositoryFactory = repositoryFactory;
			_workspaceArtifactId = workspaceArtifactId;
			_jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(_workspaceArtifactId);
			_objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(_workspaceArtifactId);
			_artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(_workspaceArtifactId);
		}

		public int GetLastJobHistoryArtifactId(int integrationPointArtifactId)
		{
			ObjectsCondition integrationPointCondition = new ObjectsCondition(new Guid(JobHistoryFieldGuids.IntegrationPoint), ObjectsConditionEnum.AnyOfThese, new List<int>() { integrationPointArtifactId });
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

		public IDictionary<Guid, int[]> GetStoppableJobHistoryArtifactIdsByStatus(int integrationPointArtifactId)
		{
			var integrationPointCondition = new ObjectsCondition(new Guid(JobHistoryFieldGuids.IntegrationPoint), ObjectsConditionEnum.AnyOfThese, new List<int>() { integrationPointArtifactId });
			Guid pendingGuid = JobStatusChoices.JobHistoryPending.ArtifactGuids.First();
			Guid processingGuid = JobStatusChoices.JobHistoryProcessing.ArtifactGuids.First();
			var stoppableCondition = new SingleChoiceCondition(new Guid(JobHistoryFieldGuids.JobStatus), SingleChoiceConditionEnum.AnyOfThese, new[] { pendingGuid, processingGuid });

			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistory),
				Condition = new CompositeCondition(integrationPointCondition, CompositeConditionEnum.And, stoppableCondition),
				Fields = new List<FieldValue>()
				{
					new FieldValue(new Guid(JobHistoryFieldGuids.JobStatus))
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

			var pendingJobArtifactIds = new List<int>();
			var processingJobArtifactIds = new List<int>();
			foreach (Result<RDO> result in results.Results)
			{
				kCura.Relativity.Client.DTOs.Choice status = null;
				try
				{
					status = result.Artifact.Fields.First().Value as kCura.Relativity.Client.DTOs.Choice;
				}
				catch
				{
					// surpress
				}

				if (status != null)
				{
					if (status.Name == JobStatusChoices.JobHistoryPending.Name)
					{
						pendingJobArtifactIds.Add(result.Artifact.ArtifactID);
					}
					else if (status.Name == JobStatusChoices.JobHistoryProcessing.Name)
					{
						processingJobArtifactIds.Add(result.Artifact.ArtifactID);
					}
				}
			}

			var stoppableJobHistoryArtifactIds = new Dictionary<Guid, int[]>
			{
				{pendingGuid, pendingJobArtifactIds.ToArray()},
				{processingGuid, processingJobArtifactIds.ToArray()}
			};

			return stoppableJobHistoryArtifactIds;
		}

		public void SetErrorStatusesToExpired(int jobHistoryArtifactId)
		{
			int objectTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistoryError));
			int errorStatusChoiceArtifactId = _artifactGuidRepository.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorExpired.ArtifactGuids)[ErrorStatusChoices.JobHistoryErrorExpired.ArtifactGuids[0]];

			SetErrorStatuses(jobHistoryArtifactId, objectTypeId, errorStatusChoiceArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);
			SetErrorStatuses(jobHistoryArtifactId, objectTypeId, errorStatusChoiceArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job);
		}

		private void SetErrorStatuses(int jobHistoryArtifactId, int objectTypeId, int errorStatusChoiceArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			try
			{
				using (IScratchTableRepository scratchTable = _repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, "StoppingRIPJob_", Guid.NewGuid().ToString()))
				{
					ICollection<int> itemLevelErrorArtifactIds = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryArtifactId, errorType);
					scratchTable.AddArtifactIdsIntoTempTable(itemLevelErrorArtifactIds);
					_jobHistoryErrorRepository.UpdateErrorStatuses(ClaimsPrincipal.Current, jobHistoryArtifactId, objectTypeId, errorStatusChoiceArtifactId, scratchTable.GetTempTableName());
				}
			}
			catch
			{
				// ignore failure
			}
		}
	}
}