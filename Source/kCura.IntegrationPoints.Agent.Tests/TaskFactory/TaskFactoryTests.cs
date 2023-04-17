using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.Sync;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.Domain.Exceptions;
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
        private Mock<ICustomProviderFlowCheck> _newCustomProviderCheckFake;
        private Mock<IRelativitySyncConstrainsChecker> _relativitySyncCheckerFake;
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

            _newCustomProviderCheckFake = new Mock<ICustomProviderFlowCheck>();
            _newCustomProviderCheckFake.Setup(
                    x => x.ShouldBeUsedAsync(
                        It.IsAny<IntegrationPointDto>()))
                .ReturnsAsync(true);

            _relativitySyncCheckerFake = new Mock<IRelativitySyncConstrainsChecker>();

            _containerFake = new Mock<IWindsorContainer>();
            _containerFake.Setup(x => x.Resolve<ICustomProviderFlowCheck>()).Returns(_newCustomProviderCheckFake.Object);
            _containerFake.Setup(x => x.Resolve<IRelativitySyncConstrainsChecker>()).Returns(_relativitySyncCheckerFake.Object);

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

        [TestCaseSource(nameof(CreateTask_CaseDataWithoutNone))]
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

            // Act
            Func<ITask> action = () => _instance.CreateTask(job, agentBase);

            // Assert
            Assert.DoesNotThrow(() => action());

            _logger.DidNotReceiveWithAnyArgs().LogError(Arg.Any<string>());
        }

        [Test]
        public void CreateTask_ShouldThrow_WhenTaskTypeNone()
        {
            int relatedId = 453245;
            int jobId = 342343;
            ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());

            Job job = new JobBuilder()
                .WithJobId(jobId)
                .WithTaskType(TaskType.None)
                .WithRelatedObjectArtifactId(relatedId)
                .Build();

            // Act
            Func<ITask> action = () => _instance.CreateTask(job, agentBase);

            // Assert
            Assert.Throws<IntegrationPointsException>(() => action());
        }

        [Test]
        public void CreateTask_ShouldCreateNewCustomProviderTask_WhenCriteriaAreMet()
        {
            // Arrange
            CustomProviderTask expectedTask = _fxt.Create<CustomProviderTask>();

            Job job = _fxt.Build<Job>()
                .With(x => x.TaskType, TaskType.SyncManager.ToString())
                .Create();

            var agentBase = new TestAgentBase(Guid.NewGuid());

            _newCustomProviderCheckFake.Setup(x => x.ShouldBeUsedAsync(It.IsAny<IntegrationPointDto>()))
                .ReturnsAsync(true);

            _containerFake.Setup(x => x.Resolve<ICustomProviderTask>()).Returns(expectedTask);

            // Act
            ITask task = _instance.CreateTask(job, agentBase);

            // Assert
            task.Should().Be(expectedTask);
        }

        [Test]
        public void CreateTask_ShouldCreateScheduledSyncTask_WhenCriteriaAreMet()
        {
            // Arrange
            ScheduledSyncTask expectedTask = _fxt.Create<ScheduledSyncTask>();

            Job job = _fxt.Build<Job>()
                .With(x => x.TaskType, TaskType.ExportService.ToString())
                .Create();

            var agentBase = new TestAgentBase(Guid.NewGuid());

            _relativitySyncCheckerFake.Setup(x => x.ShouldUseRelativitySyncApp(It.IsAny<int>()))
                .Returns(true);

            _containerFake.Setup(x => x.Resolve<IScheduledSyncTask>()).Returns(expectedTask);

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

        private static IEnumerable<TestCaseData> CreateTask_CaseDataWithoutNone()
        {
            IEnumerable<TaskType> taskTypes = Enum.GetValues(typeof(TaskType)).Cast<TaskType>()
                .Except(new[] { TaskType.None });

            foreach (var taskType in taskTypes)
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
