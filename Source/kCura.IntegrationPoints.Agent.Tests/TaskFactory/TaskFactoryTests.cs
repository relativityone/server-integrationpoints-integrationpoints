using System;
using System.Collections.Generic;
using AutoFixture;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Moq;
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
        private Mock<IWindsorContainer> _containerFake;
        private ITaskFactory _instance;

        private IFixture _fxt;

        [SetUp]
        public override void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _logger = Substitute.For<IAPILog>();
            _logger.ForContext<IntegrationPoints.Agent.TaskFactory.TaskFactory>().Returns(_logger);

            ILogFactory loggerFactory = Substitute.For<ILogFactory>();
            loggerFactory.GetLogger().Returns(_logger);

            IAgentHelper helper = Substitute.For<IAgentHelper>();
            helper.GetLoggerFactory().Returns(loggerFactory);

            IIntegrationPointService integrationPointService = CreateIntegrationPointServiceMock();
            ITaskExceptionMediator taskExceptionMediator = Substitute.For<ITaskExceptionMediator>();

            _jobSynchronizationChecker = Substitute.For<IJobSynchronizationChecker>();
            _jobHistoryService = Substitute.For<ITaskFactoryJobHistoryService>();
            ITaskFactoryJobHistoryServiceFactory jobHistoryServiceFactory = Substitute.For<ITaskFactoryJobHistoryServiceFactory>();
            jobHistoryServiceFactory.CreateJobHistoryService(Arg.Any<IntegrationPointDto>()).Returns(_jobHistoryService);

            _containerFake = new Mock<IWindsorContainer>();

            _instance = new IntegrationPoints.Agent.TaskFactory.TaskFactory(
                helper,
                taskExceptionMediator,
                _jobSynchronizationChecker,
                jobHistoryServiceFactory,
                _containerFake.Object,
                integrationPointService);
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

            _jobSynchronizationChecker.Received().CheckForSynchronization(typeof(SendEmailWorker), job, Arg.Any<IntegrationPointDto>(), agentBase);
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

        [Test]
        public void CreateTask_ShouldCreateNewCustomProviderTask_WhenCriteriaAreMet()
        {
            // Arrange
            var expectedTask = _fxt.Create<CustomProviderTask>();

            Job job = _fxt.Build<Job>()
                .With(x => x.TaskType, TaskType.SyncManager.ToString())
                .Create();

            var agentBase = new TestAgentBase(Guid.NewGuid());

            var customProviderCheck = new Mock<INewCustomProviderFlowCheck>();
            customProviderCheck.Setup(x => x.ShouldBeUsedAsync(It.IsAny<IntegrationPointDto>()))
                .ReturnsAsync(true);

            _containerFake.Setup(x => x.Resolve<INewCustomProviderFlowCheck>()).Returns(customProviderCheck.Object);
            _containerFake.Setup(x => x.Resolve<ICustomProviderTask>()).Returns(expectedTask);

            // Act
            ITask task = _instance.CreateTask(job, agentBase);

            // Assert
            task.Should().Be(expectedTask);
        }

        private IIntegrationPointService CreateIntegrationPointServiceMock()
        {
            var integrationPoint = new IntegrationPointDto();

            IIntegrationPointService integrationPointService = Substitute.For<IIntegrationPointService>();
            integrationPointService.Read(Arg.Any<int>()).Returns(integrationPoint);
            return integrationPointService;
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
                : base(agentGuid, Substitute.For<IKubernetesMode>(), agentService, jobService, scheduleRuleFactory)
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
