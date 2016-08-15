using Castle.Windsor;
using global::Relativity.API;
using kCura.Apps.Common.Utils.Serializers;
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
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	public class ITaskFactoryTests
	{
		private IAgentHelper _helper;
		private ISerializer _serializer;
		private IContextContainerFactory _contextContainerFactory;
		private ICaseServiceContext _caseServiceContext;
		private IRSAPIClient _rsapiClient;
		private IWorkspaceDBContext _workspaceDbContext;
		private IEddsServiceContext _eddsServiceContext;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryService _jobHistoryService;
		private IAgentService _agentService;
		private IJobService _jobService;
		private IManagerFactory _managerFactory;
		private TaskFactory _instance;
		private int _workspaceId = 12345;
		private int _integrationPointId = 54321;

		[SetUp]
		public void Setup()
		{
			_helper = Substitute.For<IAgentHelper>();
			_serializer = Substitute.For<ISerializer>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>(); 
			_caseServiceContext = Substitute.For<ICaseServiceContext>(); 
			_rsapiClient = Substitute.For<IRSAPIClient>(); 
			_workspaceDbContext = Substitute.For<IWorkspaceDBContext>(); 
			_eddsServiceContext = Substitute.For<IEddsServiceContext>(); 
			_repositoryFactory = Substitute.For<IRepositoryFactory>(); 
			_jobHistoryService = Substitute.For<IJobHistoryService>(); 
			_agentService = Substitute.For<IAgentService>(); 
			_jobService = Substitute.For<IJobService>();
			_managerFactory = Substitute.For<IManagerFactory>(); 

			_instance = new TaskFactory(_helper, _serializer, _contextContainerFactory, _caseServiceContext, _rsapiClient, _workspaceDbContext, _eddsServiceContext, _repositoryFactory, _jobHistoryService, _agentService, _jobService, _managerFactory);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void HasOtherJobsExecuting_GoldFlow(bool expectedHasJobsExecuting)
		{
			//Arrange
			IContextContainer contextContainer = Substitute.For<IContextContainer>();
			IQueueManager queueManager = Substitute.For<IQueueManager>();

			_contextContainerFactory.CreateContextContainer(_helper).Returns(contextContainer);
			_managerFactory.CreateQueueManager(contextContainer).Returns(queueManager);
			
			Job job = JobExtensions.CreateJob();
			DateTime now = DateTime.UtcNow;
			job.NextRunTime = now;

			queueManager.HasJobsExecuting(job.WorkspaceID, job.RelatedObjectArtifactID, job.JobId, job.NextRunTime).Returns(expectedHasJobsExecuting);

			//Act
			bool hasJobsExecuting = _instance.HasOtherJobsExecuting(job);

			//Assert
			Assert.AreEqual(expectedHasJobsExecuting, hasJobsExecuting);
			_contextContainerFactory.Received(1).CreateContextContainer(_helper);
			_managerFactory.Received(1).CreateQueueManager(contextContainer);
			queueManager.Received(1).HasJobsExecuting(job.WorkspaceID, job.RelatedObjectArtifactID, job.JobId, job.NextRunTime);
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
			Assert.Throws<AgentDropJobException>(() => _instance.DropJobAndThrowException(job, integrationPointDto, agent), exceptionMessage);

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
			integrationPointDto.JobHistory = new[] {1, 2, 3};
			_jobHistoryService.CreateRdo(integrationPointDto, taskParameters.BatchInstance, JobTypeChoices.JobHistoryRun, Arg.Any<DateTime>()).Returns(jobHistory);

			//Act
			Assert.Throws<AgentDropJobException>(() => _instance.DropJobAndThrowException(job, integrationPointDto, agent), exceptionMessage);

			//Assert
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Received(1).Update(integrationPointDto);
			_jobHistoryService.Received(1).DeleteRdo(jobHistory.ArtifactId);
			_serializer.Received(1).Deserialize<TaskParameters>(job.JobDetails);
			_jobHistoryService.Received(1).UpdateRdo(jobHistory);
			_jobHistoryService.Received(1).CreateRdo(Arg.Any<Data.IntegrationPoint>(), taskParameters.BatchInstance, JobTypeChoices.JobHistoryRun, Arg.Any<DateTime>());

			int[] expectedJobHistoryArray = new[] {1, 3};
			Assert.AreEqual(expectedJobHistoryArray, integrationPointDto.JobHistory);
		}


		[Test]
		public void UpdateJobHistory()
		{
			// arrange
			Job tempJob = JobExtensions.CreateJob();
			IAgentHelper helper = Substitute.For<IAgentHelper>();
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint();
			TaskParameters paramerters = new TaskParameters();
			JobHistory jobHistory = new JobHistory() { ArtifactId = 1234 };

			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());
			IWindsorContainer container = Substitute.For<IWindsorContainer>();
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			ISerializer serializer = Substitute.For<ISerializer>();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();

			caseServiceContext.RsapiService.IntegrationPointLibrary.Read(Arg.Any<int>()).Returns(integrationPoint);
			serializer.Deserialize<TaskParameters>(Arg.Any<String>()).Returns(paramerters);
			jobHistoryService.CreateRdo(integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRun, Arg.Any<DateTime>()).Returns(jobHistory);

			container.Resolve<SyncManager>().Throws(new Exception("Error message."));
			container.Resolve<ISerializer>().Returns(serializer);
			container.Resolve<ICaseServiceContext>().Returns(caseServiceContext);
			container.Resolve<IJobHistoryService>().Returns(jobHistoryService);

			TaskFactory taskFactory = new TaskFactory(helper, container);

			// act
			Assert.Throws<Exception>(() => taskFactory.CreateTask(tempJob, agentBase), "Error message.");

			// assert
			caseServiceContext.RsapiService.JobHistoryErrorLibrary.Received(1).Create(Arg.Any<List<JobHistoryError>>());
		}

		public class TestAgentBase : ScheduleQueueAgentBase
		{
			public TestAgentBase(Guid agentGuid, IDBContext dbContext = null, IAgentService agentService = null, IJobService jobService = null, IScheduleRuleFactory scheduleRuleFactory = null)
				: base(agentGuid, dbContext, agentService, jobService, scheduleRuleFactory)
			{
			}

			public override string Name { get; }
		}
	}
}