using Castle.Windsor;
using Relativity.API;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.WinEDDS;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	public class ITaskFactoryTests : TestBase
	{
		private IAgentHelper _helper;
		private IHelper _targetHelper;
		private IIntegrationPointSerializer _serializer;
		private IContextContainerFactory _contextContainerFactory;
		private ICaseServiceContext _caseServiceContext;
		private IRSAPIClient _rsapiClient;
		private IJobHistoryService _jobHistoryService;
		private IAgentService _agentService;
		private IJobService _jobService;
		private IManagerFactory _managerFactory;
		private IServiceFactory _serviceFactory;
		private TaskFactory _instance;
		private IRelativityConfigurationFactory _relativityConfigurationFactory;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IContextContainer _contextContainer;
		private IQueueManager _queueManager;
		private IServiceManagerProvider _serviceManagerProvider;
		private IHelperFactory _helperFactory;

		[SetUp]
		public override void SetUp()
		{
			_relativityConfigurationFactory = Substitute.For<IRelativityConfigurationFactory>();

			_helper = Substitute.For<IAgentHelper>();
			_targetHelper = Substitute.For<IHelper>();
			_serializer = Substitute.For<IIntegrationPointSerializer>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_rsapiClient = Substitute.For<IRSAPIClient>();
			_agentService = Substitute.For<IAgentService>();
			_jobService = Substitute.For<IJobService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_serviceFactory = Substitute.For<IServiceFactory>();
			_serviceManagerProvider = Substitute.For<IServiceManagerProvider>();
			var apiLog = Substitute.For<IAPILog>();

			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_contextContainer = Substitute.For<IContextContainer>();
			_queueManager = Substitute.For<IQueueManager>();
			_helperFactory = Substitute.For<IHelperFactory>();
			_helperFactory.CreateTargetHelper(_helper, null, Arg.Any<string>()).Returns(_helper);
			_helperFactory.CreateTargetHelper(_helper, Arg.Any<int>(), Arg.Any<string>()).Returns(_targetHelper);

			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(),
				Arg.Any<Relativity.Client.DTOs.Choice>(),
				Arg.Any<DateTime?>()).Returns(new JobHistory());

			_serviceFactory.CreateJobHistoryService(_helper, _helper).Returns(_jobHistoryService);
			_serviceFactory.CreateJobHistoryService(_helper, _targetHelper).Returns(_jobHistoryService);

			_instance = new TaskFactory(_helper, _serializer, _contextContainerFactory, _caseServiceContext, _jobHistoryService, _agentService, _jobService,
				_managerFactory, apiLog);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void HasOtherJobsExecuting_GoldFlow(bool expectedHasJobsExecuting)
		{
			//Arrange
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);

			Job job = JobExtensions.CreateJob();
			DateTime now = DateTime.UtcNow;
			job.NextRunTime = now;

			_queueManager.HasJobsExecuting(job.WorkspaceID, job.RelatedObjectArtifactID, job.JobId, job.NextRunTime)
				.Returns(expectedHasJobsExecuting);

			//Act
			bool hasJobsExecuting = _instance.HasOtherJobsExecuting(job);

			//Assert
			Assert.AreEqual(expectedHasJobsExecuting, hasJobsExecuting);
			_contextContainerFactory.Received(1).CreateContextContainer(_helper);
			_managerFactory.Received(1).CreateQueueManager(_contextContainer);
			_queueManager.Received(1).HasJobsExecuting(job.WorkspaceID, job.RelatedObjectArtifactID, job.JobId, job.NextRunTime);
		}

		[Test]
		public void DropJobAndThrowException_ScheduledJob_AgentExceptionThrown()
		{
			//Arrange
			ScheduleQueueAgentBase agent = new Agent();
			Job job = JobExtensions.CreateJob();
			job.ScheduleRuleType = "not null";

			Data.IntegrationPoint integrationPointDto = new Data.IntegrationPoint();
			DateTime nextRunTime = DateTime.UtcNow;

			_jobService.GetJobNextUtcRunDateTime(job, Arg.Any<IScheduleRuleFactory>(), Arg.Any<TaskResult>())
				.Returns(nextRunTime);
			String exceptionMessage =
				$"Unable to execute Integration Point job: There is already a job currently running. Job is re-scheduled for {nextRunTime}.";

			//Act
			Assert.Throws<AgentDropJobException>(() => _instance.DropJobAndThrowException(job, integrationPointDto, agent),
				exceptionMessage);

			//Assert
			_jobService.Received(1).GetJobNextUtcRunDateTime(job, Arg.Any<IScheduleRuleFactory>(), Arg.Any<TaskResult>());
		}

		[Test]
		public void DropJobAndThrowException_NonScheduledJob_AgentExceptionThrown()
		{
			//Arrange
			String exceptionMessage = $"Unable to execute Integration Point job: There is already a job currently running.";
			TaskParameters taskParameters = new TaskParameters();
			string batchInstance = "A6E6BD34-3814-4C9D-AD98-8FC47F5E25D1";
			taskParameters.BatchInstance = new Guid(batchInstance);

			ScheduleQueueAgentBase agent = new Agent();
			Job job = JobExtensions.CreateJob();

			JobHistory jobHistory = new JobHistory();
			jobHistory.ArtifactId = 2;

			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(taskParameters);

			Data.IntegrationPoint integrationPointDto = new Data.IntegrationPoint();
			integrationPointDto.JobHistory = new[] { 1, 2, 3 };
			_jobHistoryService.CreateRdo(integrationPointDto, taskParameters.BatchInstance, JobTypeChoices.JobHistoryRun,
				Arg.Any<DateTime>()).Returns(jobHistory);

			//Act
			Assert.Throws<AgentDropJobException>(() => _instance.DropJobAndThrowException(job, integrationPointDto, agent),
				exceptionMessage);

			//Assert
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Received(1).Update(integrationPointDto);
			_jobHistoryService.Received(1).DeleteRdo(jobHistory.ArtifactId);
			_serializer.Received(1).Deserialize<TaskParameters>(job.JobDetails);
			_jobHistoryService.Received(1).UpdateRdo(jobHistory);
			_jobHistoryService.Received(1)
				.CreateRdo(Arg.Any<Data.IntegrationPoint>(), taskParameters.BatchInstance, JobTypeChoices.JobHistoryRun,
					Arg.Any<DateTime>());

			int[] expectedJobHistoryArray = new[] { 1, 3 };
			Assert.AreEqual(expectedJobHistoryArray, integrationPointDto.JobHistory);
		}


		[TestCase(null)]
		[TestCase(1000)]
		public void CreateTask_UnableToResolveTaskType_JobHistoryIsUpdated(int? federatedInstanceArtifactId)
		{
			// arrange
			int relatedId = 453245;
			int jobId = 342343;
			const string errorMessage = "WOAH WOAH WOAH EERRRROOOORRR!";
			TaskType taskType = TaskType.SyncManager;

			Job tempJob = JobExtensions.CreateJob(jobId, taskType, relatedId);
			_rsapiClient.APIOptions = new APIOptions(40234);


			TaskParameters paramerters = new TaskParameters();
			JobHistory jobHistory = new JobHistory() { ArtifactId = 1234 };

			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			IWindsorContainer container = Substitute.For<IWindsorContainer>();

			container.Resolve<ICaseServiceContext>().Returns(_caseServiceContext);
			container.Resolve<IRelativityConfigurationFactory>().Returns(_relativityConfigurationFactory);
			container.Resolve<IIntegrationPointSerializer>().Returns(_serializer);
			container.Resolve<IJobHistoryErrorService>().Returns(_jobHistoryErrorService);
			container.Resolve<IServiceFactory>().Returns(_serviceFactory);
			container.Resolve<IContextContainerFactory>().Returns(_contextContainerFactory);
			container.Resolve<IHelperFactory>().Returns(_helperFactory);

			container.Resolve<SyncManager>().Throws(new Exception(errorMessage));

			_helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(_rsapiClient);
			_relativityConfigurationFactory.GetConfiguration().Returns(new EmailConfiguration());
			_serializer.Deserialize<TaskParameters>(Arg.Any<String>()).Returns(paramerters);
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), JobTypeChoices.JobHistoryRun,
				Arg.Any<DateTime>()).Returns(jobHistory);

			DestinationConfiguration destinationConfiguration = new DestinationConfiguration()
			{
				FederatedInstanceArtifactId = federatedInstanceArtifactId
			};

			_caseServiceContext.RsapiService.IntegrationPointLibrary.Read(relatedId).Returns(new Data.IntegrationPoint()
			{
				DestinationConfiguration = JsonConvert.SerializeObject(destinationConfiguration),
				SecuredConfiguration = "{}"
			});

			_serializer.Deserialize<DestinationConfiguration>(Arg.Any<string>()).Returns(destinationConfiguration);

			TaskFactory taskFactory = new TaskFactory(_helper, container);

			// act
			Assert.Throws<Exception>(() => taskFactory.CreateTask(tempJob, agentBase), errorMessage);

			// assert
			_jobHistoryErrorService.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorJob, Arg.Any<Exception>());
			_jobHistoryErrorService.Received(1).CommitErrors();
			_jobHistoryService.UpdateRdo(Arg.Is<JobHistory>(
				x => x.ArtifactId == jobHistory.ArtifactId && x.JobStatus == JobStatusChoices.JobHistoryErrorJobFailed));
		}

		[Test]
		[TestCaseSource(nameof(CreateTask_CaseData))]
		public void CreateTask_AllTaskTypesAreResolvable(TaskType taskType)
		{
			// Arrange
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			IExportConfig exportConfig = Substitute.For<IExportConfig>();
			IRSAPIService rsapiService = Substitute.For<IRSAPIService>();
			IGenericLibrary<Data.IntegrationPoint> integrationPointLibrary = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			var sqlServiceFactory = Substitute.For<ISqlServiceFactory>();

			caseServiceContext.RsapiService.Returns(rsapiService);
			rsapiService.IntegrationPointLibrary.Returns(integrationPointLibrary);

			int relatedId = 453245;
			var integrationPoint = new Data.IntegrationPoint()
			{
				DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings()),
				SecuredConfiguration = "{}"
			};
			integrationPointLibrary.Read(relatedId).Returns(integrationPoint);
			IWindsorContainer windsorContainer = new WindsorContainer();
			windsorContainer.Register(Component.For<IRelativityConfigurationFactory>().Instance(_relativityConfigurationFactory));
			windsorContainer.Register(Component.For<ICaseServiceContext>().Instance(caseServiceContext));
			windsorContainer.Register(Component.For<ISqlServiceFactory>().Instance(sqlServiceFactory));
			windsorContainer.Register(Component.For<IServiceManagerProvider>().Instance(_serviceManagerProvider));
			windsorContainer.Register(Component.For<IExportConfig>().Instance(exportConfig));
			windsorContainer.Register(Component.For<IServiceFactory>().Instance(_serviceFactory));

			var taskFactory = new TaskFactory(_helper, windsorContainer);
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			_rsapiClient.APIOptions = new APIOptions(40234);
			_helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(_rsapiClient);
			_relativityConfigurationFactory.GetConfiguration().Returns(new EmailConfiguration());

			int jobId = 342343;

			Job job = JobExtensions.CreateJob(jobId, taskType, relatedId);
			try
			{
				// Act / Assert
				taskFactory.CreateTask(job, agentBase);
			}
			catch (Exception ex)
			{
				throw new Exception($"Unable to resolve the \"{taskType}\" task type.", ex);
			}
		}

		[Test]
		public void ItShouldUpdateJobHistoryJobId()
		{
			TaskType taskType = TaskType.SyncManager;

			// Arrange
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			IExportConfig exportConfig = Substitute.For<IExportConfig>();
			IRSAPIService rsapiService = Substitute.For<IRSAPIService>();
			IGenericLibrary<Data.IntegrationPoint> integrationPointLibrary = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			var sqlServiceFactory = Substitute.For<ISqlServiceFactory>();

			caseServiceContext.RsapiService.Returns(rsapiService);
			rsapiService.IntegrationPointLibrary.Returns(integrationPointLibrary);

			int relatedId = 453245;
			var integrationPoint = new Data.IntegrationPoint()
			{
				DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings()),
				SecuredConfiguration = "{}"
			};
			integrationPointLibrary.Read(relatedId).Returns(integrationPoint);
			IWindsorContainer windsorContainer = new WindsorContainer();
			windsorContainer.Register(Component.For<IRelativityConfigurationFactory>().Instance(_relativityConfigurationFactory));
			windsorContainer.Register(Component.For<ICaseServiceContext>().Instance(caseServiceContext));
			windsorContainer.Register(Component.For<ISqlServiceFactory>().Instance(sqlServiceFactory));
			windsorContainer.Register(Component.For<IServiceManagerProvider>().Instance(_serviceManagerProvider));
			windsorContainer.Register(Component.For<IExportConfig>().Instance(exportConfig));
			windsorContainer.Register(Component.For<IServiceFactory>().Instance(_serviceFactory));

			var taskFactory = new TaskFactory(_helper, windsorContainer);
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			_rsapiClient.APIOptions = new APIOptions(40234);
			_helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(_rsapiClient);
			_relativityConfigurationFactory.GetConfiguration().Returns(new EmailConfiguration());

			int jobId = 342343;

			Job job = JobExtensions.CreateJob(jobId, taskType, relatedId);

			// Act
			taskFactory.CreateTask(job, agentBase);

			// Assert
			_jobHistoryService.Received().UpdateRdo(Arg.Is<JobHistory>(x => x.JobID == jobId.ToString()));
		}

		[Test]
		public void CreateTask_FederatedInstanceExportServiceIsResolvable()
		{
			// Arrange
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			IRSAPIService rsapiService = Substitute.For<IRSAPIService>();
			IGenericLibrary<Data.IntegrationPoint> integrationPointLibrary = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			var sqlServiceFactory = Substitute.For<ISqlServiceFactory>();

			caseServiceContext.RsapiService.Returns(rsapiService);
			rsapiService.IntegrationPointLibrary.Returns(integrationPointLibrary);

			int relatedId = 453245;
			var integrationPoint = new Data.IntegrationPoint()
			{
				SourceConfiguration = JsonConvert.SerializeObject(new SourceConfiguration() { FederatedInstanceArtifactId = 1 }),
				DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = 1 }),
				SecuredConfiguration = JsonConvert.SerializeObject(new OAuthClientDto())
			};
			integrationPointLibrary.Read(relatedId).Returns(integrationPoint);
			IWindsorContainer windsorContainer = new WindsorContainer();
			windsorContainer.Register(Component.For<IRelativityConfigurationFactory>().Instance(_relativityConfigurationFactory));
			windsorContainer.Register(Component.For<ICaseServiceContext>().Instance(caseServiceContext));
			windsorContainer.Register(Component.For<ISqlServiceFactory>().Instance(sqlServiceFactory));
			windsorContainer.Register(Component.For<IServiceManagerProvider>().Instance(_serviceManagerProvider));
			windsorContainer.Register(Component.For<IHelperFactory>().Instance(_helperFactory));
			windsorContainer.Register(Component.For<IServiceFactory>().Instance(_serviceFactory));

			var taskFactory = new TaskFactory(_helper, windsorContainer);
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			_rsapiClient.APIOptions = new APIOptions(40234);
			_helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(_rsapiClient);
			_relativityConfigurationFactory.GetConfiguration().Returns(new EmailConfiguration());

			int jobId = 342343;

			Job job = JobExtensions.CreateJob(jobId, TaskType.ExportService, relatedId);
			try
			{
				// Act / Assert
				taskFactory.CreateTask(job, agentBase);
			}
			catch (Exception ex)
			{
				throw new Exception($"Unable to resolve the \"{TaskType.ExportService}\" task type.", ex);
			}
		}

		private static IEnumerable<TestCaseData> CreateTask_CaseData()
		{
			foreach (var taskType in Enum.GetValues(typeof(TaskType)))
			{
				TestCaseData testCaseData = new TestCaseData(taskType) { TestName = taskType.ToString() };
				yield return testCaseData;
			}
		}

		public class TestAgentBase : ScheduleQueueAgentBase
		{
			public TestAgentBase(Guid agentGuid, IAgentService agentService = null,
				IJobService jobService = null, IScheduleRuleFactory scheduleRuleFactory = null)
				: base(agentGuid, agentService, jobService, scheduleRuleFactory)
			{
			}

			public override string Name { get; }
			public override ITask GetTask(Job job)
			{
				throw new NotImplementedException();
			}
		}
	}
}