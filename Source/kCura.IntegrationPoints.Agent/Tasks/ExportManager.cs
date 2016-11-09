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
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportManager : SyncManager
	{
		#region Fields

		private readonly IExportInitProcessService _exportInitProcessService;
		private readonly IAPILog _logger;

		#endregion //Private Fields

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

		#region Constructors

		public ExportManager(ICaseServiceContext caseServiceContext,
			IDataProviderFactory providerFactory,
			IJobManager jobManager,
			IJobService jobService,
			IHelper helper,
			IIntegrationPointService integrationPointService,
			ISerializer serializer, IGuidService guidService,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IScheduleRuleFactory scheduleRuleFactory,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainer,
			IEnumerable<IBatchStatus> batchStatuses,
			IExportInitProcessService exportInitProcessService)
			: base(
				caseServiceContext, providerFactory, jobManager, jobService, helper, integrationPointService, serializer, guidService, jobHistoryService, jobHistoryErrorService,
				scheduleRuleFactory, managerFactory, contextContainer, batchStatuses)
		{
			_exportInitProcessService = exportInitProcessService;
			_logger = Helper.GetLoggerFactory().GetLogger().ForContext<ExportManager>();
		}

		#endregion //Constructors

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
				throw new Exception("Failed to retrieve corresponding Integration Point.");
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
				return _exportInitProcessService.CalculateDocumentCountToTransfer(settings);
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
			_logger.LogError("Failed to retrieve Integration Point object for job {JobId}.", job.JobId);
		}

		#endregion
	}
}