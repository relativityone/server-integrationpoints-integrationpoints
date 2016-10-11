using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportManager : SyncManager
	{
		#region Constructors

		public ExportManager(ICaseServiceContext caseServiceContext,
			IDataProviderFactory providerFactory,
			IJobManager jobManager,
			IJobService jobService,
			IHelper helper,
			IIntegrationPointService integrationPointService,
			ISerializer serializer, IGuidService guidService,
			IJobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			IScheduleRuleFactory scheduleRuleFactory,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainer,
			IEnumerable<IBatchStatus> batchStatuses,
			IRepositoryFactory repositoryFactory)
			: base(
				caseServiceContext, providerFactory, jobManager, jobService, helper, integrationPointService, serializer, guidService, jobHistoryService, jobHistoryErrorService,
				scheduleRuleFactory, managerFactory, contextContainer, batchStatuses)
		{
			_repositoryFactory = repositoryFactory;
			_logger = Helper.GetLoggerFactory().GetLogger().ForContext<ExportManager>();
		}

		#endregion //Constructors

		#region Properties

		public override int BatchSize
		{
			get
			{
				//Currently Export Shared library (kCura.WinEDDS) is making usage of batching internalLy
				//so for now we need to create only one worker job
				return int.MaxValue;
			}
		}

		#endregion //Properties

		#region Fields

		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAPILog _logger;

		#endregion //Private Fields

		#region Methods

		protected override TaskType GetTaskType()
		{
			return TaskType.ExportWorker;
		}

		/// <summary>
		///     This method returns record (batch) ids that should be processed by ExportWorker class
		/// </summary>
		/// <param name="job">Details of the export job</param>
		/// <returns>List of batch ids to be processed</returns>
		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			//Currently Export Shared library (kCura.WinEDDS) is making usage of batching internalLy
			//so for now we need to create only one worker job
			yield break;
		}

		public override int BatchTask(Job job, IEnumerable<string> batchIDs)
		{
			IIntegrationPointManager integrationPointManager = ManagerFactory
				.CreateIntegrationPointManager(ContextContainerFactory.CreateContextContainer(Helper));

			IntegrationPointDTO integrationPoint = integrationPointManager.Read(job.WorkspaceID, job.RelatedObjectArtifactID);

			if (integrationPoint == null)
			{
				LogUnableToRetrieveIntegrationPoint(job);
				throw new Exception("Failed to retrieved corresponding Integration Point.");
			}

			ExportUsingSavedSearchSettings sourceSettings = Serializer.Deserialize<ExportUsingSavedSearchSettings>(integrationPoint.SourceConfiguration);

			int totalCount = GetTotalExportItemsCount(sourceSettings, job);

			// This condition should be changed when we implement correct Total Items count for Production or Folders/Subfolders Export types
			if (totalCount >= 0)
			{
				CreateBatchJob(job, new List<string>());
			}
			return totalCount;
		}

		private int GetTotalExportItemsCount(ExportUsingSavedSearchSettings settings, Job job)
		{
			try
			{
				ISavedSearchRepository savedSearchRepo = _repositoryFactory.GetSavedSearchRepository(job.WorkspaceID,
					settings.SavedSearchArtifactId);

				int totalDocsCount = savedSearchRepo.GetTotalDocsCount();
				int extractedIndex = Math.Min(totalDocsCount, Math.Abs(settings.StartExportAtRecord - 1));

				return Math.Max(totalDocsCount - extractedIndex, 0);
			}
			catch (Exception ex)
			{
				LogRetrievingExportItemsCountError(job, ex);
				throw;
			}
		}

		#endregion //Methods

		#region Logging

		private void LogRetrievingExportItemsCountError(Job job, Exception ex)
		{
			_logger.LogError(ex, "Failed to retrieve total export items count for job {JobId}.", job.JobId);
		}

		private void LogUnableToRetrieveIntegrationPoint(Job job)
		{
			_logger.LogError("Failed to retrieved Integration Point object for job {JobId}.", job.JobId);
		}

		#endregion
	}
}