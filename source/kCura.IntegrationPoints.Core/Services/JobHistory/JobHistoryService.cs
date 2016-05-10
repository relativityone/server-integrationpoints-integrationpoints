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
				Fields = GetFields()
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
				Fields = GetFields()
			};

			IList<Data.JobHistory> jobHistories = _caseServiceContext.RsapiService.JobHistoryLibrary.Query(query);
			return jobHistories;
		}

		public Data.JobHistory GetLastJobHistory(IntegrationPoint integrationPoint)
		{
			Data.JobHistory lastCompletedJobHistory = GetLastJobHistory(integrationPoint.JobHistory.ToList());
			return lastCompletedJobHistory;
		}

		public Data.JobHistory GetLastJobHistory(IList<int> jobHistoryArtifactIds)
		{
			if (!jobHistoryArtifactIds.Any())
			{
				return null;
			}

			var jobHistoryArtifactIdCondition = new WholeNumberCondition("ArtifactID", NumericConditionEnum.In)
			{
				Value = jobHistoryArtifactIds as List<int>
			};
			var artifactIdSort = new Sort
			{
				Field = "ArtifactID",
				Direction = SortEnum.Descending
			};
			List<Sort> sorts = new List<Sort>(1) { artifactIdSort };

			var query = new Query<RDO>
			{
				ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.JobHistory),
				Condition = jobHistoryArtifactIdCondition,
				Fields = GetFields(),
				Sorts = sorts
			};

			IList<Data.JobHistory> jobHistories = _caseServiceContext.RsapiService.JobHistoryLibrary.Query(query);
			Data.JobHistory lastJobHistory = jobHistories.FirstOrDefault();
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
				Fields = GetJobHistoryErrorFields(),
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
					//Should not always be Run Now type job, but cannot find a high enough method call to be able to make a distinction between a Run Now, Scheduled, or Retry job
					JobType = JobTypeChoices.JobHistoryRunNow,
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

		protected List<FieldValue> GetFields()
		{
			return (from field in (BaseRdo.GetFieldMetadata(typeof(Data.JobHistory)).Values).ToList()
					select new FieldValue(field.FieldGuid)).ToList();
		}

		private List<FieldValue> GetJobHistoryErrorFields()
		{
			return (from field in (BaseRdo.GetFieldMetadata(typeof(JobHistoryError)).Values).ToList()
					select new FieldValue(field.FieldGuid)).ToList();
		}
	}
}
