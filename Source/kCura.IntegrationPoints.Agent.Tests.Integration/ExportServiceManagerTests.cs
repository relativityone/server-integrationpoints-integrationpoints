using System;
using System.Collections.Generic;
using System.Data;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Agent.Exceptions;
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
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoints.Agent.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using Constants = kCura.IntegrationPoint.Tests.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Ignore("TODO: Broken test needs to be fixed!")]
	public class ExportServiceManagerTests : RelativityProviderTemplate
	{
		private ExportServiceManager _exportManager;
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobService;
		private ICaseServiceContext _caseContext;
		private IQueueDBContext _queueContext;
		private Relativity.Client.DTOs.Workspace _sourceWorkspaceDto;

		public ExportServiceManagerTests() : base("ExportServiceManagerTests", 
			"ExportServiceManagerTests_Destination")
		{ }

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			ControlIntegrationPointAgents(false);
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		protected override void Install()
		{
			base.Install();
			Container.Register(Component.For<IAgentValidator>().ImplementedBy<AgentValidator>().LifestyleTransient());
		}

		public override void SuiteTeardown()
		{
			ControlIntegrationPointAgents(true);
			base.SuiteTeardown();
		}

		public override void TestSetup()
		{
			_caseContext = Container.Resolve<ICaseServiceContext>();
			var helperFactory = Container.Resolve<IHelperFactory>();
			var contextContainerFactory = Container.Resolve<IContextContainerFactory>();
			var synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			var exporterFactory = Container.Resolve<IExporterFactory>();
			var onBehalfOfUserClaimsPrincipalFactory = Container.Resolve<IOnBehalfOfUserClaimsPrincipalFactory>();
			var repositoryFactory = Container.Resolve<IRepositoryFactory>();
			var managerFactory = Container.Resolve<IManagerFactory>();
			var serializer = Container.Resolve<ISerializer>();
			_jobService = Container.Resolve<IJobService>();
			IScheduleRuleFactory scheduleRuleFactory = new DefaultScheduleRuleFactory();
			var jobHistoryService = Container.Resolve<IJobHistoryService>();
			var jobHistoryErrorService = Container.Resolve<IJobHistoryErrorService>();
			var jobStatisticsService = Container.Resolve<JobStatisticsService>();
			var agentValidator = Container.Resolve<IAgentValidator>();
			var jobStatusUpdater = Container.Resolve<IJobStatusUpdater>();
			var jobHistoryUpdater = new JobHistoryBatchUpdateStatus(jobStatusUpdater, jobHistoryService, _jobService, serializer);
			

			_exportManager = new ExportServiceManager(Helper, helperFactory,
				_caseContext, contextContainerFactory,
				synchronizerFactory,
				exporterFactory,
				onBehalfOfUserClaimsPrincipalFactory,
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
				agentValidator
				);

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_sourceWorkspaceDto = Workspace.GetWorkspaceDto(SourceWorkspaceArtifactId);
		}

		[Test]
		[Category(Constants.SMOKE_TEST)]
		[Ignore("TODO: Broken test needs to be fixed!")]
		public void RunRelativityProviderAlone()
		{
			// arrange
			var serializer = Container.Resolve<ISerializer>();
			var model = new IntegrationPointModel()
			{
				SourceProvider = RelativityProvider.ArtifactId,
				Name = "ARRRRRRRGGGHHHHH - RunRelativityProviderAlone",
				DestinationProvider = DestinationProvider.ArtifactId,
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
				
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, model.ArtifactID, 9); // run now
			Job job = null;
			try
			{
				job = GetNextJobInScheduleQueue(new[] { _sourceWorkspaceDto.ResourcePoolID.Value }, model.ArtifactID); // pick up job

				var parameters = serializer.Deserialize<TaskParameters>(job.JobDetails);

				// act
				Assert.IsNotNull(job, "There is no job to execute");
				_exportManager.Execute(job); // run the job
				
				// assert
				model = RefreshIntegrationModel(model);
				var jobHistoryService = Container.Resolve<IJobHistoryService>();
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

		[Test]
		[Category(Constants.SMOKE_TEST)]
		public void StopStateCannotBeUpdatedWhileExportServiceObservers()
		{
			Job job = null;
			try
			{
				DataTable dataTable = Import.GetImportTable("DocId", 5);
				Import.ImportNewDocuments(SourceWorkspaceArtifactId, dataTable);

				IntegrationPointModel model = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay,
					"StopStateCannotBeUpdatedWhileFinalizingExportServiceObservers", "Append/Overlay");
				model = CreateOrUpdateIntegrationPoint(model); // create integration point

				_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, model.ArtifactID, 9); // run now
				string query = $"SELECT * FROM [{GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME}]";
				var context = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
				DataTable result = context.EddsDBContext.ExecuteSqlStatementAsDataTable(query);
				foreach (DataRow row in result.Rows)
				{
					Console.WriteLine(string.Join(", ", row.ItemArray));
				}
				Console.WriteLine();
				Console.WriteLine($"Resource pool ID: {_sourceWorkspaceDto.ResourcePoolID.Value}");
				Console.WriteLine($"IntegrationPointModel: {JsonConvert.SerializeObject(model)}");
				Console.WriteLine();
				job = GetNextJobInScheduleQueue(new[] { _sourceWorkspaceDto.ResourcePoolID.Value }, model.ArtifactID); // pick up job

				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				job = JobExtensions.Execute(
					qDBContext: _queueContext,
					workspaceID: SourceWorkspaceArtifactId,
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