using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Constants = kCura.IntegrationPoint.Tests.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Category(Constants.INTEGRATION_CATEGORY)]
	public class ExportServiceManagerTests : RelativityProviderTemplate
	{
		private ExportServiceManager _exportManager;
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobServiceManager;
		private IJobService _jobService;
		private ICaseServiceContext _caseContext;
		private Relativity.Client.DTOs.Workspace _sourceWorkspaceDto;
		private int _agentTypeId;
		private Query _integrationPointAgentsQuery;

		public ExportServiceManagerTests() : base("ExportServiceManagerTests", null)
		{ }

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_agentTypeId = IntegrationPoint.Tests.Core.Agent.GetAgentTypeByName("Integration Points Agent").ArtifactID;
			_integrationPointAgentsQuery = new Query()
			{
				Condition = $"'AgentTypeArtifactID' == {_agentTypeId}"
			};
			IntegrationPoint.Tests.Core.Agent.DisableAgents(_integrationPointAgentsQuery);
		}

		public override void SuiteTeardown()
		{
			IntegrationPoint.Tests.Core.Agent.EnableAgents(_integrationPointAgentsQuery);
			base.SuiteTeardown();
		}

		protected override void Install()
		{
			base.Install();
			Container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifeStyle.Transient);
			Container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>()
				.ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>()
				.LifestyleTransient());
			Container.Register(
				Component.For<IRSAPIService>()
					.Instance(new RSAPIService(Container.Resolve<IHelper>(), SourceWorkspaceArtifactId))
					.LifestyleTransient());
		}

		public override void TestSetup()
		{
			_caseContext = Container.Resolve<ICaseServiceContext>();
			IContextContainerFactory contextContainerFactory = Container.Resolve<IContextContainerFactory>();
			ISynchronizerFactory synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			IExporterFactory exporterFactory = Container.Resolve<IExporterFactory>();
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory = Container.Resolve<IOnBehalfOfUserClaimsPrincipalFactory>();
			ISourceWorkspaceManager sourceWorkspaceManager = Container.Resolve<ISourceWorkspaceManager>();
			ISourceJobManager sourceJobManager = Container.Resolve<ISourceJobManager>();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IManagerFactory managerFactory = Container.Resolve<IManagerFactory>();
			ISerializer serializer = Container.Resolve<ISerializer>();
			_jobService = Container.Resolve<IJobService>();
			IScheduleRuleFactory scheduleRuleFactory = new DefaultScheduleRuleFactory();
			IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			JobHistoryErrorService jobHistoryErrorManager = Container.Resolve<JobHistoryErrorService>();
			JobStatisticsService jobStatisticsService = Container.Resolve<JobStatisticsService>();

			IJobStatusUpdater jobStatusUpdater = Container.Resolve<IJobStatusUpdater>();
			JobHistoryBatchUpdateStatus jobHistoryUpdater = new JobHistoryBatchUpdateStatus(jobStatusUpdater, jobHistoryService, _jobService, serializer);

			_exportManager = new ExportServiceManager(Helper,
				_caseContext, contextContainerFactory,
				synchronizerFactory,
				exporterFactory,
				onBehalfOfUserClaimsPrincipalFactory,
				sourceWorkspaceManager,
				sourceJobManager,
				repositoryFactory,
				managerFactory,
				new[] { jobHistoryUpdater },
				serializer,
				_jobService,
				scheduleRuleFactory,
				jobHistoryService,
				jobHistoryErrorManager,
				jobStatisticsService
				);

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobServiceManager = Container.Resolve<IJobService>();
			_sourceWorkspaceDto = IntegrationPoint.Tests.Core.Workspace.GetWorkspaceDto(SourceWorkspaceArtifactId);
		}

		[Test]
		public void RunRelativityProviderAlone()
		{
			// arrange
			ISerializer serializer = Container.Resolve<ISerializer>();
			IntegrationModel model = new IntegrationModel()
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
			};
			model = CreateOrUpdateIntegrationPoint(model); // create integration point

			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, model.ArtifactID, 9); // run now
			Job job = null;
			try
			{
				job = GetNextJobInScheduleQueue(new[] { _sourceWorkspaceDto.ResourcePoolID.Value }, model.ArtifactID); // pick up job

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

		[Test]
		public void AgentPickUpRunNowJobWhenScheduledJobIsRunning()
		{
			Job scheduledJob = null;
			Job job = null;
			try
			{
				// arrange
				const int fakeAgentId = 78945;
				ISerializer serializer = Container.Resolve<ISerializer>();
				IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
				IntegrationModel model = new IntegrationModel()
				{
					SourceProvider = RelativityProvider.ArtifactId,
					Name = "ARRRRRRRGGGHHHHH - AgentPickUpRunNowJobWhenScheduledJobIsRunning",
					DestinationProvider = DestinationProvider.ArtifactId,
					SourceConfiguration = CreateDefaultSourceConfig(),
					Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
					Map = CreateDefaultFieldMap(),
					Scheduler = new Scheduler()
					{
						EnableScheduler = true,
						StartDate = DateTime.UtcNow.AddDays(-10).ToString(),
						ScheduledTime = DateTime.UtcNow.AddDays(-10).AddMinutes(30).TimeOfDay.ToString(),
						Reoccur = 10,
						SelectedFrequency = ScheduleInterval.Daily.ToString()
					},
					SelectedOverwrite = "Append Only",
				};
				model = CreateOrUpdateIntegrationPoint(model); // create integration point
				int jobId = GetLastScheduledJobId(SourceWorkspaceArtifactId, model.ArtifactID);
				scheduledJob = _jobService.GetJob(jobId);

				// run now; job created
				_jobService.CreateJob(SourceWorkspaceArtifactId, model.ArtifactID, TaskType.ExportService.ToString(),
					DateTime.UtcNow, serializer.Serialize(new TaskParameters() { BatchInstance = Guid.NewGuid() }), 9,
					null, null);

				AssignJobToAgent(fakeAgentId, scheduledJob.JobId); // agent pick up scheduled job
				job = GetNextJobInScheduleQueue(new[] { _sourceWorkspaceDto.ResourcePoolID.Value }, model.ArtifactID); // agent pick up job

				TaskParameters runNowParameters = serializer.Deserialize<TaskParameters>(job.JobDetails);

				// act
				Assert.IsNotNull(job, "There is no job to execute");
				AgentDropJobException ex = Assert.Throws<AgentDropJobException>(() => _exportManager.Execute(job)); // run the job
				Assert.That("Unable to execute Integration Point job: There is already a job currently running.", Is.EqualTo(ex.Message));

				// assert
				model = RefreshIntegrationModel(model);
				JobHistory runNowJobhistory = jobHistoryService.GetRdo(runNowParameters.BatchInstance);

				Assert.IsNull(runNowJobhistory); // job history is deleted
				Assert.IsNotNull(model); // ip object does not get deleted
				Assert.IsFalse(model.HasErrors ?? false);

			}
			finally
			{
				if (scheduledJob != null)
				{
					_jobService.DeleteJob(scheduledJob.JobId);
				}
				if (job != null)
				{
					_jobService.DeleteJob(job.JobId);
				}
			}
		}

		[Test]
		public void AgentPickUpScheduledJobJobWhenRunNowJobIsRunning()
		{
			Job fakeScheduledJob = null;
			Job job = null;

			try
			{
				// arrange
				const int fakeAgentId = 78945;
				ISerializer serializer = Container.Resolve<ISerializer>();
				IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
				IntegrationModel model = new IntegrationModel()
				{
					SourceProvider = RelativityProvider.ArtifactId,
					Name = "ARRRRRRRGGGHHHHH - AgentPickUpScheduledJobJobWhenRunNowJobIsRunning",
					DestinationProvider = DestinationProvider.ArtifactId,
					SourceConfiguration = CreateDefaultSourceConfig(),
					Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
					Map = CreateDefaultFieldMap(),
					Scheduler = new Scheduler()
					{
						EnableScheduler = true,
						StartDate = DateTime.UtcNow.AddDays(300).ToString(),
						ScheduledTime = DateTime.UtcNow.AddDays(300).AddMinutes(30).TimeOfDay.ToString(),
						Reoccur = 10,
						SelectedFrequency = ScheduleInterval.Daily.ToString()
					},
					SelectedOverwrite = "Append Only",
				};
				model = CreateOrUpdateIntegrationPoint(model); // create integration point
				int jobId = GetLastScheduledJobId(SourceWorkspaceArtifactId, model.ArtifactID);
				var integrationPointId = model.ArtifactID;

				fakeScheduledJob = _jobService.GetJob(jobId);

				// a person click run now
				_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointId, 9);

				// run now picked up by an agent
				job = GetNextJobInScheduleQueue(new[] { _sourceWorkspaceDto.ResourcePoolID.Value }, model.ArtifactID);

				AssignJobToAgent(fakeAgentId, fakeScheduledJob.JobId); // agent pick up the scheduled job
				TaskParameters scheduledJobParameters = serializer.Deserialize<TaskParameters>(fakeScheduledJob.JobDetails);
				TaskParameters runNowJobParam = serializer.Deserialize<TaskParameters>(job.JobDetails);

				// act
				Assert.IsNotNull(job, "There is no job to execute");
				AgentDropJobException ex = Assert.Throws<AgentDropJobException>(() => _exportManager.Execute(fakeScheduledJob)); // run the job
				string exceptionMessage = String.Format("Unable to execute Integration Point job: There is already a job currently running. Job is re-scheduled for {0}.", fakeScheduledJob.NextRunTime);
				Assert.That(exceptionMessage, Is.EqualTo(ex.Message));

				// assert
				model = RefreshIntegrationModel(model);
				JobHistory scheduledJobhistory = jobHistoryService.GetRdo(scheduledJobParameters.BatchInstance);
				JobHistory history = jobHistoryService.GetRdo(runNowJobParam.BatchInstance);

				Assert.IsNotNull(history); // job history is deleted
				Assert.IsNotNull(model); // ip object does not get deleted
				Assert.IsFalse(model.HasErrors ?? false); // expect the error being logged
			}
			finally
			{
				if (fakeScheduledJob != null)
				{
					_jobService.DeleteJob(fakeScheduledJob.JobId);
				}
				if (job != null)
				{
					_jobService.DeleteJob(job.JobId);
				}
			}
		}
	}
}