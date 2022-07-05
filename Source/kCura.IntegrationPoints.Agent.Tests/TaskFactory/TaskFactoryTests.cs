using System;
using System.Collections.Generic;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.TaskFactory
{
	[TestFixture, Category("Unit")]
	public class TaskFactoryTests : TestBase
	{
		private IAPILog _logger;
		private IJobSynchronizationChecker _jobSynchronizationChecker;
		private ITaskFactoryJobHistoryService _jobHistoryService;
		private ITaskFactory _instance;

		[SetUp]
		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_logger.ForContext<IntegrationPoints.Agent.TaskFactory.TaskFactory>().Returns(_logger);

			ILogFactory loggerFactory = Substitute.For<ILogFactory>();
			loggerFactory.GetLogger().Returns(_logger);

			IAgentHelper helper = Substitute.For<IAgentHelper>();
			helper.GetLoggerFactory().Returns(loggerFactory);


			IIntegrationPointRepository integrationPointRepository = CreateIntegrationPointRepositoryMock();
			ITaskExceptionMediator taskExceptionMediator = Substitute.For<ITaskExceptionMediator>();


			_jobSynchronizationChecker = Substitute.For<IJobSynchronizationChecker>();
			_jobHistoryService = Substitute.For<ITaskFactoryJobHistoryService>();
			ITaskFactoryJobHistoryServiceFactory jobHistoryServiceFactory = Substitute.For<ITaskFactoryJobHistoryServiceFactory>();
			jobHistoryServiceFactory.CreateJobHistoryService(Arg.Any<Data.IntegrationPoint>()).Returns(_jobHistoryService);

			IWindsorContainer container = Substitute.For<IWindsorContainer>();

			_instance = new IntegrationPoints.Agent.TaskFactory.TaskFactory(helper, taskExceptionMediator,
				_jobSynchronizationChecker, jobHistoryServiceFactory, container, integrationPointRepository);
		}

		[Test]
		public void ItShouldSetJobIdOnJobHistory()
		{
			int jobId = 342343;
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			Job job = new JobBuilder().WithJobId(jobId).Build();

			_instance.CreateTask(job, agentBase);

			_jobHistoryService.Received().SetJobIdOnJobHistory(job);
		}

		[Test]
		public void ItShouldCheckForSynchronization()
		{
			TaskType taskType = TaskType.SendEmailWorker;
			int relatedId = 453245;
			int jobId = 342343;
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			Job job = new JobBuilder()
				.WithJobId(jobId)
				.WithTaskType(taskType)
				.WithRelatedObjectArtifactId(relatedId)
				.Build();

			_instance.CreateTask(job, agentBase);

			_jobSynchronizationChecker.Received().CheckForSynchronization(typeof(SendEmailWorker), job, Arg.Any<Data.IntegrationPoint>(), agentBase);
		}

		[Test]
		public void ItShouldRethrowExceptions()
		{
			_jobHistoryService.When(x => x.SetJobIdOnJobHistory(Arg.Any<Job>())).Throw<ArgumentNullException>();

			Job job = JobExtensions.CreateJob();
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			Assert.Throws<ArgumentNullException>(() => _instance.CreateTask(job, agentBase));
		}

		[Test]
		public void ItShouldUpdateJobHistoryOnFailure()
		{
			_jobHistoryService.When(x => x.SetJobIdOnJobHistory(Arg.Any<Job>())).Throw<ArgumentNullException>();

			Job job = JobExtensions.CreateJob();
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			try
			{
				_instance.CreateTask(job, agentBase);
			}
			catch (Exception) { }

			_jobHistoryService.Received().UpdateJobHistoryOnFailure(job, Arg.Any<ArgumentNullException>());
		}

		[Test]
		public void ItShouldNotUpdateJobHistoryForAgentDropJobException()
		{
			_jobHistoryService.When(x => x.SetJobIdOnJobHistory(Arg.Any<Job>())).Throw<AgentDropJobException>();

			Job job = JobExtensions.CreateJob();
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			try
			{
				_instance.CreateTask(job, agentBase);
			}
			catch (AgentDropJobException) { }

			_jobHistoryService.DidNotReceiveWithAnyArgs().UpdateJobHistoryOnFailure(Arg.Any<Job>(), Arg.Any<ArgumentNullException>());
		}

		[Test]
		[TestCaseSource(nameof(CreateTask_CaseData))]
		public void CreateTask_AllTaskTypesAreResolvable(TaskType taskType)
		{
			int relatedId = 453245;
			int jobId = 342343;
			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

			Job job = new JobBuilder()
				.WithJobId(jobId)
				.WithTaskType(taskType)
				.WithRelatedObjectArtifactId(relatedId)
				.Build();
			try
			{
				// Act
				_instance.CreateTask(job, agentBase);

				// Assert
				if (taskType == TaskType.None)
				{
					_logger.Received().LogError("Unable to create task. Unknown task type ({TaskType})", taskType);
				}
				else
				{
					_logger.DidNotReceiveWithAnyArgs().LogError(Arg.Any<string>());
				}

			}
			catch (Exception ex)
			{
				throw new Exception($"Unable to resolve the \"{taskType}\" task type.", ex);
			}
		}

		private IIntegrationPointRepository CreateIntegrationPointRepositoryMock()
		{
			var integrationPoint = new Data.IntegrationPoint();

			IIntegrationPointRepository integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			integrationPointRepository.ReadWithFieldMappingAsync(Arg.Any<int>()).Returns(integrationPoint);
			return integrationPointRepository;
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
				: base(agentGuid,Substitute.For<IKubernetesMode>(), agentService, jobService, scheduleRuleFactory)
			{
			}

			public override string Name { get; }
			protected override TaskResult ProcessJob(Job job)
			{
				throw new NotImplementedException();
			}

			protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
			{
			}
		}
	}
}