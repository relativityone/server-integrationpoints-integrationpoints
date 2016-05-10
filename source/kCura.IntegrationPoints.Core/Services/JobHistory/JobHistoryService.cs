using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobHistoryService : IJobHistoryService
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IWorkspaceRepository _workspaceRepository;

		public JobHistoryService(ICaseServiceContext caseServiceContext, IWorkspaceRepository workspaceRepository)
		{
			_caseServiceContext = caseServiceContext;
			_workspaceRepository = workspaceRepository;
		}

		public Data.JobHistory GetRdo(Guid batchInstance)
		{
			var query = new Query<RDO>
			{
				ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.JobHistory),
				Condition =
					new TextCondition(Guid.Parse(JobHistoryFieldGuids.BatchInstance), TextConditionEnum.EqualTo,
						batchInstance.ToString()),
				Fields = GetFields<Data.JobHistory>()
			};
			Data.JobHistory jobHistory = _caseServiceContext.RsapiService.JobHistoryLibrary.Query(query).SingleOrDefault(); //there should only be one!

			return jobHistory;
		}

		public IList<Data.JobHistory> GetJobHistory(IList<int> jobHistoryArtifactIds)
		{
			var condition = new WholeNumberCondition("ArtifactID", NumericConditionEnum.In)
			{
				Value = jobHistoryArtifactIds.ToList()
			};

			var query = new Query<RDO>
			{
				ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.JobHistory),
				Condition = condition,
				Fields = GetFields<Data.JobHistory>()
			};

			IList<Data.JobHistory> jobHistories = _caseServiceContext.RsapiService.JobHistoryLibrary.Query(query);
			return jobHistories;
		}

		public Data.JobHistory GetLastJobHistory(List<int> jobHistoryArtifactIds)
		{
			if (!jobHistoryArtifactIds.Any())
			{
				return null;
			}

			int highestArtifactId = jobHistoryArtifactIds.Max();
			Data.JobHistory lastJobHistory = _caseServiceContext.RsapiService.JobHistoryLibrary.Read(highestArtifactId);
			return lastJobHistory;
		}

		public void UpdateJobHistoryOnRetry(Data.JobHistory jobHistory)
		{
			// TODO: update the status appropriately
			//jobHistory.Status = JobStatusChoices.Expired;

			Guid errorTypeJob = ErrorTypeChoices.JobHistoryErrorJob.ArtifactGuids.First();
			Guid errorTypeItem = ErrorTypeChoices.JobHistoryErrorItem.ArtifactGuids.First();
			var errorTypes = new List<Guid>(2) { errorTypeJob, errorTypeItem };
			var errorStatusCondtion = new SingleChoiceCondition(Guid.Parse(JobHistoryErrorFieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese, errorTypes);
			var jobHistoryArtifactIdCondition = new ObjectCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), ObjectConditionEnum.EqualTo, jobHistory.ArtifactId);
			var conditions = new CompositeCondition(jobHistoryArtifactIdCondition, CompositeConditionEnum.And, errorStatusCondtion);

			var query = new Query<RDO>
			{
				ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.JobHistoryError),
				Condition = conditions,
				Fields = GetFields<JobHistoryError>(),
			};

			IList<JobHistoryError> jobHistoryErrors = _caseServiceContext.RsapiService.JobHistoryErrorLibrary.Query(query);
			foreach (JobHistoryError jobHistoryError in jobHistoryErrors)
			{
				// TODO: update the errors appropriately
				//jobHistoryError.ErrorType = ErrorTypeChoices.JobHistoryErrorExpired;
				//jobHistoryError.ErrorStatus = ErrorTypeChoices.JobHistoryErrorExpired;
			}

			_caseServiceContext.RsapiService.JobHistoryLibrary.Update(jobHistory);
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Update(jobHistoryErrors);
		}

		public Data.JobHistory CreateRdo(IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUtc)
		{
			Data.JobHistory jobHistory = null;

			try
			{
				jobHistory = GetRdo(batchInstance);
			}
			catch (Exception)
			{
				// ignored
			}

			if (jobHistory == null)
			{
				jobHistory = CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryScheduledRun, startTimeUtc);
			}

			return jobHistory;
		}

		public Data.JobHistory CreateRdo(IntegrationPoint integrationPoint, Guid batchInstance, Relativity.Client.Choice jobType, DateTime? startTimeUtc)
		{
			Data.JobHistory jobHistory = null;

			try
			{
				jobHistory = GetRdo(batchInstance);
			}
			catch
			{
				// ignored
			}

			if (jobHistory == null)
			{
				jobHistory = new Data.JobHistory
				{
					Name = integrationPoint.Name,
					IntegrationPoint = new[] { integrationPoint.ArtifactId },
					BatchInstance = batchInstance.ToString(),
					JobType = jobType,
					JobStatus = JobStatusChoices.JobHistoryPending,
					ItemsImported = 0,
					ItemsWithErrors = 0
				};

				ImportSettings setting = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);
				IntegrationPoints.Contracts.Models.WorkspaceDTO workspaceDto = _workspaceRepository.Retrieve(setting.CaseArtifactId);
				jobHistory.DestinationWorkspace = Utils.GetFormatForWorkspaceOrJobDisplay(workspaceDto.Name, setting.CaseArtifactId);

				if (startTimeUtc.HasValue)
				{
					jobHistory.StartTimeUTC = startTimeUtc.Value;
				}

				int artifactId = _caseServiceContext.RsapiService.JobHistoryLibrary.Create(jobHistory);
				jobHistory.ArtifactId = artifactId;
			}

			return jobHistory;
		}

		public void UpdateRdo(Data.JobHistory jobHistory)
		{
			_caseServiceContext.RsapiService.JobHistoryLibrary.Update(jobHistory);
		}

		public void DeleteRdo(int jobHistoryId)
		{
			_caseServiceContext.RsapiService.JobHistoryLibrary.Delete(jobHistoryId);
		}

		protected List<FieldValue> GetFields<T>()
		{
			return (from field in (BaseRdo.GetFieldMetadata(typeof(Data.JobHistory)).Values).ToList()
					select new FieldValue(field.FieldGuid)).ToList();
		}
	}
}
