using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	using Apps.Common.Utils.Serializers;

	public class ExportManager : SyncManager
	{
		#region Fields

		private readonly IRepositoryFactory _repositoryFactory;

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
			JobHistoryErrorService jobHistoryErrorService,
			IScheduleRuleFactory scheduleRuleFactory,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainer,
			IEnumerable<IBatchStatus> batchStatuses,
			IRepositoryFactory repositoryFactory)
			: base(caseServiceContext, providerFactory, jobManager, jobService, helper, integrationPointService, serializer, guidService, jobHistoryService, jobHistoryErrorService, scheduleRuleFactory, managerFactory, contextContainer, batchStatuses)
		{
			_repositoryFactory = repositoryFactory;
		}

		#endregion //Constructors

		#region Methods

		protected override TaskType GetTaskType()
		{
			return TaskType.ExportWorker;
		}

		/// <summary>
		/// This method returns record (batch) ids that should be processed by ExportWorker class
		/// </summary>
		/// <param name="job">Details of the export job</param>
		/// <returns>List of batch ids to be processed</returns>
		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			//Currently Export Shared library (kCura.WinEDDS) is making usage of batching internalLy
			//so for now we need to create only one worker job
			yield return null;
		}

		public override int BatchTask(Job job, IEnumerable<string> batchIDs)
		{
			int integrationPointId = job.RelatedObjectArtifactID;

			var integrationPoint = ManagerFactory
				.CreateIntegrationPointManager(ContextContainerFactory.CreateContextContainer(Helper))
				.Read(job.WorkspaceID, integrationPointId);

			if (integrationPoint == null)
			{
				throw new Exception("Failed to retrieved corresponding Integration Point.");
			}
			var sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration);

			var savedSearchRepo = _repositoryFactory.GetSavedSearchRepository(job.WorkspaceID,
				sourceConfiguration.SavedSearchArtifactId);

			int totalCount = savedSearchRepo.GetTotalDocsCount();
			if (totalCount > 0)
			{
				CreateBatchJob(job, new List<string>());
			}
			return totalCount;
		}

		#endregion //Methods
	}
}
