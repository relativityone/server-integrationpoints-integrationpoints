using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class JobHistoryService : IJobHistoryService
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IFederatedInstanceManager _federatedInstanceManager;
		private readonly IWorkspaceManager _workspaceManager;
		private readonly IAPILog _logger;
		private readonly IIntegrationPointSerializer _serializer;

		public JobHistoryService(ICaseServiceContext caseServiceContext, IFederatedInstanceManager federatedInstanceManager, IWorkspaceManager workspaceManager, IHelper helper, IIntegrationPointSerializer serializer)
		{
			_caseServiceContext = caseServiceContext;
			_federatedInstanceManager = federatedInstanceManager;
			_workspaceManager = workspaceManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<JobHistoryService>();
			_serializer = serializer;
		}

		public Data.JobHistory GetRdo(Guid batchInstance)
		{
			var request = new QueryRequest
			{
				Condition = $"'{JobHistoryFields.BatchInstance}' == '{batchInstance}'"
			};
			List<Data.JobHistory> jobHistories = _caseServiceContext.RsapiService.RelativityObjectManager.Query<Data.JobHistory>(request);
			if (jobHistories.Count > 1)
			{
				LogMoreThanOneHistoryInstanceWarning(batchInstance);
			}
			Data.JobHistory jobHistory = jobHistories.SingleOrDefault(); //there should only be one!

			return jobHistory;
		}

		public IList<Data.JobHistory> GetJobHistory(IList<int> jobHistoryArtifactIds)
		{
			var request = new QueryRequest
			{
				Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", jobHistoryArtifactIds.ToList())}]",
				Fields = new Data.JobHistory().ToFieldList()
			};

			IList<Data.JobHistory> jobHistories = _caseServiceContext.RsapiService.RelativityObjectManager.Query<Data.JobHistory>(request);
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

				integrationPoint.JobHistory = integrationPoint?.JobHistory.Concat(new[] { jobHistory.ArtifactId }).ToArray();
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
			catch (Exception e)
			{
				LogCreatingHistoryRdoError(e);
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
					ItemsTransferred = 0,
					ItemsWithErrors = 0,
					Overwrite = integrationPoint.OverwriteFields.Name
				};

				var importSettings = _serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

				WorkspaceDTO workspaceDto = _workspaceManager.RetrieveWorkspace(importSettings.CaseArtifactId);

				FederatedInstanceDto federatedInstanceDto =
					_federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(importSettings.FederatedInstanceArtifactId);

				jobHistory.DestinationWorkspace = Utils.GetFormatForWorkspaceOrJobDisplay(workspaceDto.Name, importSettings.CaseArtifactId);
				jobHistory.DestinationInstance = Utils.GetFormatForWorkspaceOrJobDisplay(federatedInstanceDto.Name, federatedInstanceDto.ArtifactId);

				if (startTimeUtc.HasValue)
				{
					jobHistory.StartTimeUTC = startTimeUtc.Value;
				}

				int artifactId = _caseServiceContext.RsapiService.RelativityObjectManager.Create(jobHistory);
				jobHistory.ArtifactId = artifactId;
			}

			return jobHistory;
		}

		public void UpdateRdo(Data.JobHistory jobHistory)
		{
			_caseServiceContext.RsapiService.RelativityObjectManager.Update(jobHistory);
		}

		public void DeleteRdo(int jobHistoryId)
		{
			_caseServiceContext.RsapiService.RelativityObjectManager.Delete(jobHistoryId);
		}

		public IList<Data.JobHistory> GetAll()
		{
			return _caseServiceContext.RsapiService.RelativityObjectManager.Query<Data.JobHistory>(new QueryRequest()
			{
				Fields = new Data.JobHistory().ToFieldList()
			});
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