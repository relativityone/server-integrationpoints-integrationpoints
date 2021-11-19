using System;
using System.Collections.Generic;
using System.Data;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ResourcePool;
using Relativity.Testing.Identification;
using WorkspaceRef = Relativity.Services.Workspace.WorkspaceRef;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ExportServiceManagerTests : RelativityProviderTemplate
	{
		private ExportServiceManager _exportManager;
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobService;
		private ICaseServiceContext _caseContext;
		private IQueueDBContext _queueContext;
		private ResourcePool _workspaceResourcePool;

		public ExportServiceManagerTests()
			: base("ExportServiceManagerTests", "ExportServiceManagerTests_Destination")
		{ }

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		protected override void InitializeIocContainer()
		{
			base.InitializeIocContainer();
			Container.Register(Component.For<IAgentValidator>().ImplementedBy<AgentValidator>().LifestyleTransient());
		}

		public override void TestSetup()
		{
			_caseContext = Container.Resolve<ICaseServiceContext>();
			ISynchronizerFactory synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			IExporterFactory exporterFactory = Container.Resolve<IExporterFactory>();
			IExportServiceObserversFactory exportServiceObserversFactory= Container.Resolve<IExportServiceObserversFactory>();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IManagerFactory managerFactory = Container.Resolve<IManagerFactory>();
			ISerializer serializer = Container.Resolve<ISerializer>();
			_jobService = Container.Resolve<IJobService>();
			IScheduleRuleFactory scheduleRuleFactory = new DefaultScheduleRuleFactory();
			IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			IJobHistoryErrorService jobHistoryErrorService = Container.Resolve<IJobHistoryErrorService>();
			JobStatisticsService jobStatisticsService = Container.Resolve<JobStatisticsService>();
			IAgentValidator agentValidator = Container.Resolve<IAgentValidator>();
			IJobStatusUpdater jobStatusUpdater = Container.Resolve<IJobStatusUpdater>();
			IAPILog logger = Container.Resolve<IAPILog>();
			IDateTimeHelper dateTimeHelper = Container.Resolve<IDateTimeHelper>();
			IIntegrationPointRepository integrationPointRepository = Container.Resolve<IIntegrationPointRepository>();
			IDocumentRepository documentRepository = Container.Resolve<IDocumentRepository>();
			IExportDataSanitizer exportDataSanitizer = Container.Resolve<IExportDataSanitizer>();
			var jobHistoryUpdater = new JobHistoryBatchUpdateStatus(
				jobStatusUpdater,
				jobHistoryService,
				_jobService,
				serializer,
				logger,
				dateTimeHelper);


			_exportManager = new ExportServiceManager(Helper,
				_caseContext,
				synchronizerFactory,
				exporterFactory,
				exportServiceObserversFactory,
				repositoryFactory,
				managerFactory,
				new[] { jobHistoryUpdater },
				serializer,
				_jobService,
				scheduleRuleFactory,
				jobHistoryService,
				jobHistoryErrorService,
				jobStatisticsService,
				null,
				agentValidator,
				integrationPointRepository,
				documentRepository,
				exportDataSanitizer);

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_workspaceResourcePool = Workspace.GetWorkspaceResourcePoolAsync(SourceWorkspaceArtifactID).GetAwaiter().GetResult();
		}

		[IdentifiedTest("b09c8436-23e8-45d7-a57c-bbe214335433")]
		[SmokeTest]
		[Ignore("")]
		public void RunRelativityProviderAlone()
		{
			// arrange
			ISerializer serializer = Container.Resolve<ISerializer>();
			var model = new IntegrationPointModel()
			{
				SourceProvider = RelativityProvider.ArtifactId,
				Name = "ARRRRRRRGGGHHHHH - RunRelativityProviderAlone",
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				Map = CreateDefaultFieldMap(),
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				SelectedOverwrite = "Append Only",
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
			model = CreateOrUpdateIntegrationPoint(model); // create integration point

			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, model.ArtifactID, 9); // add job to schedule queue
			Job job = null;
			try
			{
				int[] resourcePoolsIDs = { _workspaceResourcePool.ArtifactID };
				job = GetNextJobInScheduleQueue(resourcePoolsIDs, model.ArtifactID, SourceWorkspaceArtifactID);

				TaskParameters parameters = serializer.Deserialize<TaskParameters>(job.JobDetails);

				// act
				Assert.IsNotNull(job, "There is no job to execute");
				_exportManager.Execute(job); // run the job

				// assert
				model = RefreshIntegrationModel(model);
				IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
				JobHistory history = jobHistoryService.GetRdo(parameters.BatchInstance);

				Assert.IsNotNull(model);
				Assert.IsFalse(model.HasErrors ?? false);
				Assert.AreEqual(0, history.ItemsWithErrors);
				Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, history.JobStatus.Name);
				Assert.IsFalse(model.HasErrors ?? false);
			}
			finally
			{
				if (job != null)
				{
					_jobService.DeleteJob(job.JobId);
				}
			}
		}

		[IdentifiedTest("6a5a30fb-ffd1-40b2-bab5-876c215eca09")]
		[SmokeTest]
		public void StopStateCannotBeUpdatedWhileExportServiceObservers()
		{
			Job job = null;
			try
			{
				DataTable dataTable = Import.GetImportTable("DocId", 5);
				Import.ImportNewDocuments(SourceWorkspaceArtifactID, dataTable);

				IntegrationPointModel model = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay,
					"StopStateCannotBeUpdatedWhileFinalizingExportServiceObservers", "Append/Overlay");
				model = CreateOrUpdateIntegrationPoint(model); // create integration point

				_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, model.ArtifactID, 9); // run now
				string query = $"SELECT * FROM [{GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME}]";
				var context = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
				DataTable result = context.EddsDBContext.ExecuteSqlStatementAsDataTable(query);
				foreach (DataRow row in result.Rows)
				{
					Console.WriteLine(string.Join(", ", row.ItemArray));
				}
				Console.WriteLine();
				Console.WriteLine($"Resource pool ID: {_workspaceResourcePool.ArtifactID}");
				Console.WriteLine($"IntegrationPointModel: {JsonConvert.SerializeObject(model)}");
				Console.WriteLine();
				int[] resourcePoolsIDs = { _workspaceResourcePool.ArtifactID };
				job = GetNextJobInScheduleQueue(resourcePoolsIDs, model.ArtifactID, SourceWorkspaceArtifactID);

				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				job = JobExtensions.Execute(
					qDBContext: _queueContext,
					workspaceID: SourceWorkspaceArtifactID,
					relatedObjectArtifactID: model.ArtifactID,
					taskType: TaskType.ExportService.ToString(),
					nextRunTime: DateTime.MaxValue,
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					locked: AgentArtifactId,
					rootJobID: 1,
					parentJobID: 1);

				_exportManager.Execute(job);

				var exception = Assert.Throws<ExecuteSQLStatementFailedException>(() => _jobService.UpdateStopState(new List<long> { job.JobId }, StopState.Stopping));
				const string exceptionMessage = "ERROR : Invalid operation. Attempted to stop an unstoppable job.";
				Assert.NotNull(exception.InnerException);
				Assert.That(exceptionMessage, Is.EqualTo(exception.InnerException.Message));
			}
			finally
			{
				if (job != null)
				{
					_jobService.DeleteJob(job.JobId);
				}
			}
		}
	}
}