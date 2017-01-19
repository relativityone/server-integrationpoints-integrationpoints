using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class JobHistoryService : IJobHistoryService
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IRepositoryFactory _targetRepositoryFactory;
		private readonly IAPILog _logger;
		private readonly ISerializer _serializer;

		public JobHistoryService(ICaseServiceContext caseServiceContext, IRepositoryFactory targetRepositoryFactory, IHelper helper, ISerializer serializer){
			_caseServiceContext = caseServiceContext;
			_targetRepositoryFactory = targetRepositoryFactory;
			_serializer = serializer;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<JobHistoryService>();
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
			var jobHistories = _caseServiceContext.RsapiService.JobHistoryLibrary.Query(query);
			if (jobHistories.Count > 1)
			{
				LogMoreThanOneHistoryInstanceWarning(batchInstance);
			}
			Data.JobHistory jobHistory = jobHistories.SingleOrDefault(); //there should only be one!

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

		public Data.JobHistory GetOrCreateScheduledRunHistoryRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUtc)
		{
			Data.JobHistory jobHistory = null;

			try
			{
				jobHistory = GetRdo(batchInstance);
			}
			catch (Exception e)
			{
				LogHistoryNotFoundError(integrationPoint, e);
				// ignored
			}

			if (jobHistory == null)
			{
				jobHistory = CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryScheduledRun, startTimeUtc);
			}

			return jobHistory;
		}

		public Data.JobHistory CreateRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, Choice jobType, DateTime? startTimeUtc)
		{
			Data.JobHistory jobHistory = null;

			try
			{
				jobHistory = GetRdo(batchInstance);
			}
			catch(Exception e)
			{
				LogCreatingHistoryRdoError(e);
				// ignored
			}

			if (jobHistory == null)
			{
				jobHistory = new Data.JobHistory
				{
					Name = integrationPoint.Name,
					IntegrationPoint = new[] {integrationPoint.ArtifactId},
					BatchInstance = batchInstance.ToString(),
					JobType = jobType,
					JobStatus = JobStatusChoices.JobHistoryPending,
					ItemsTransferred = 0,
					ItemsWithErrors = 0
				};

				ImportSettings setting = _serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);
				IWorkspaceRepository workspaceRepository = _targetRepositoryFactory.GetWorkspaceRepository();
				WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(setting.CaseArtifactId);
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
			return (from field in BaseRdo.GetFieldMetadata(typeof(Data.JobHistory)).Values.ToList()
				select new FieldValue(field.FieldGuid)).ToList();
		}

		#region Logging

		private void LogMoreThanOneHistoryInstanceWarning(Guid batchInstance)
		{
			_logger.LogWarning("More than one job history instance found for {BatchInstance}.", batchInstance.ToString());
		}

		private void LogHistoryNotFoundError(Data.IntegrationPoint integrationPoint, Exception e)
		{
			_logger.LogError(e, "Job history for Integration Point {IntegrationPointId} not found.", integrationPoint.ArtifactId);
		}

		private void LogCreatingHistoryRdoError(Exception e)
		{
			_logger.LogError(e, "Failed to create History RDO.");
		}

		#endregion
	}
}