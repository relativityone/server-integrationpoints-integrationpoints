using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter;
using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.RelativitySync;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Choice;
using Choice = Relativity.Services.Objects.DataContracts.ChoiceRef;

namespace kCura.IntegrationPoints.Agent.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class AgentTests
    {
        private const string _PROVIDER_NAME = "TestProvider";
        private readonly Guid _batchInstanceGuid = Guid.NewGuid();
        private Mock<IMessageService> _messageServiceMock;
        private Mock<IJobHistoryService> _jobHistoryServiceFake;
        private Mock<IMemoryUsageReporter> _memoryUsageReporter;
        private Mock<IHeartbeatReporter> _heartbeatReporter;
        private Mock<IAPILog> _loggerMock;
        private Mock<IIntegrationPointService> _integrationPointServiceMock;
        private IWindsorContainer _container;
        private Mock<IKubernetesMode> _kubernetesModeMock;

        [SetUp]
        public void SetUp()
        {
            _container = new WindsorContainer();

            _messageServiceMock = new Mock<IMessageService>();
            _jobHistoryServiceFake = new Mock<IJobHistoryService>();
            _memoryUsageReporter = new Mock<IMemoryUsageReporter>();
            _heartbeatReporter = new Mock<IHeartbeatReporter>();
            _loggerMock = new Mock<IAPILog>();
            _kubernetesModeMock = new Mock<IKubernetesMode>();
            _kubernetesModeMock.Setup(x => x.IsEnabled()).Returns(false);
        }

        [Test]
        public void ProcessJob_ShouldSendStartMetric_WhenJobHasBeenStarted()
        {
            // Arrange
            TestAgent sut = PrepareSut();

            Job job = JobExtensions.CreateJob();

            // Act
            sut.ProcessJob_Test(job);

            // Assert
            _messageServiceMock.Verify(x => x.Send(It.IsAny<JobStartedMessage>()), Times.Once);
        }

        [Test]
        public void ProcessJob_ShouldNotSendStartMetric_WhenJobWasResumed()
        {
            // Arrange
            TestAgent sut = PrepareSut();

            SetupJobHistoryStatus(JobStatusChoices.JobHistorySuspended);

            Job job = JobExtensions.CreateJob();

            // Act
            sut.ProcessJob_Test(job);

            // Assert
            _messageServiceMock.Verify(x => x.Send(It.IsAny<JobStartedMessage>()), Times.Never);
        }

        [Test]
        public void ProcessJob_ShouldActivateTimer_AtTheBeginningOfProcessing()
        {
            // Arrange
            TestAgent sut = PrepareSut();

            SetupJobHistoryStatus(JobStatusChoices.JobHistorySuspended);

            Job job = JobExtensions.CreateJob();

            // Act
            sut.ProcessJob_Test(job);

            // Assert
            _memoryUsageReporter.Verify(
                x =>
                    x.ActivateTimer(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.Is<string>(jobType => jobType == "ExportService")), Times.Once);
        }

        [Test]
        public void ProcessJob_ShouldNotThrow_WhenJobHistoryDoesNotExist()
        {
            // Arrange
            TestAgent sut = PrepareSut();

            _jobHistoryServiceFake.Setup(x => x.GetRdoWithoutDocuments(_batchInstanceGuid))
                .Returns((JobHistory)null);

            Job job = JobExtensions.CreateJob();

            // Act
            Action action = () => sut.ProcessJob_Test(job);

            // Assert
            action.ShouldNotThrow();
        }

        [Test]
        public void ProcessJob_ShouldBreakSchedule_WhenBreakScheduleFlagIsTrue()
        {
            // Arrange
            TestAgent sut = PrepareSut();

            const int integrationPointId = 200;
            Mock<ITaskFactoryJobHistoryService> jobHistoryServiceMock = new Mock<ITaskFactoryJobHistoryService>();

            Mock<ITaskFactoryJobHistoryServiceFactory> jobHistoryServiceFactoryFake = new Mock<ITaskFactoryJobHistoryServiceFactory>();
            jobHistoryServiceFactoryFake.Setup(x => x.CreateJobHistoryService(It.IsAny<IntegrationPointDto>())).Returns(jobHistoryServiceMock.Object);
            RegisterMock(jobHistoryServiceFactoryFake);

            IScheduleRule scheduleRule = new PeriodicScheduleRule(ScheduleInterval.Daily, DateTime.Now, TimeSpan.FromDays(2));
            IsJobFailed jobFailed = new IsJobFailed(new InvalidOperationException(), true, true);
            Job job = new JobBuilder()
                .WithRelatedObjectArtifactId(integrationPointId)
                .WithScheduleRule(scheduleRule)
                .WithJobFailed(jobFailed)
                .Build();

            // Act
            TaskResult result = sut.ProcessJob_Test(job);

            // Assert
            result.Status.Should().Be(TaskStatusEnum.Fail);
            result.Exceptions.Single().Should().BeOfType<InvalidOperationException>();
            _integrationPointServiceMock.Verify(x => x.DisableScheduler(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void Execute_ShouldReturn_WhenDidWork_And_KubernetesMode()
        {
            // Arrange
            _kubernetesModeMock.Setup(x => x.IsEnabled()).Returns(true);
            TestAgent sut = PrepareSut();
            sut.DidWork = true;

            // Act
            sut.Execute();

            // Assert
            _loggerMock.Verify(
                x =>
                    x.LogInformation(
                        "Attempting to initialize Config Settings factory in {TypeName}",
                        nameof(ScheduleQueueAgentBase)),
                Times.Never);
        }

        [Test]
        public void Execute_ShouldReturnFailAndMarkJobHistoryAsFailed_WhenJobWasMarkedAsFailedInQueue()
        {
            // Arrange
            Exception expectedException = new Exception("Test Exception");
            const long expectedJobId = 100;
            const int integrationPointId = 200;

            Mock<IExtendedJob> extendedJob = new Mock<IExtendedJob>();
            Mock<IJobHistorySyncService> jobHistoryService = new Mock<IJobHistorySyncService>();

            RegisterMock(extendedJob);
            RegisterMock(jobHistoryService);

            TestAgent sut = PrepareSut();

            Job job = new JobBuilder()
                .WithJobId(expectedJobId)
                .WithRelatedObjectArtifactId(integrationPointId)
                .Build();
            job.MarkJobAsFailed(expectedException, false, false);

            // Act
            TaskResult result = sut.ProcessJob_Test(job);

            // Assert
            result.Status.Should().Be(TaskStatusEnum.Fail);
            result.Exceptions.Should().Contain(expectedException);

            jobHistoryService.Verify(
                x => x.UpdateFinishedJobAsync(
                    extendedJob.Object,
                    It.Is<Choice>(c => c.Guid == JobStatusChoices.JobHistoryErrorJobFailedGuid),
                    true),
                Times.Once);

            jobHistoryService.Verify(
                x => x.AddJobHistoryErrorAsync(
                    extendedJob.Object,
                    expectedException),
                Times.Once);
        }

        private TestAgent PrepareSut()
        {
            Mock<IJobExecutor> jobExecutor = new Mock<IJobExecutor>();
            jobExecutor.Setup(x => x.ProcessJob(It.IsAny<Job>())).Returns(new TaskResult());

            Mock<IJobContextProvider> jobContextProvider = new Mock<IJobContextProvider>();

            Mock<ISerializer> serializer = new Mock<ISerializer>();
            serializer.Setup(x => x.Deserialize<TaskParameters>(It.IsAny<string>())).Returns(
                new TaskParameters
                {
                    BatchInstance = Guid.NewGuid()
                });

            Mock<IConfig> config = new Mock<IConfig>();

            Mock<IRipMetrics> ripMetrics = new Mock<IRipMetrics>();

            Mock<IRelativitySyncConstrainsChecker> relativitySyncConstrainsChecker = new Mock<IRelativitySyncConstrainsChecker>();
            relativitySyncConstrainsChecker.Setup(x => x.ShouldUseRelativitySync(It.IsAny<int>())).Returns(false);

            Mock<ITaskParameterHelper> taskParameterHelper = new Mock<ITaskParameterHelper>();
            taskParameterHelper.Setup(x => x.GetBatchInstance(It.IsAny<Job>())).Returns(_batchInstanceGuid);

            Mock<IRipToggleProvider> toggleProvider = new Mock<IRipToggleProvider>();

            _integrationPointServiceMock = new Mock<IIntegrationPointService>();
            _integrationPointServiceMock.Setup(x => x.Read(It.IsAny<int>()))
                .Returns(new IntegrationPointDto
                {
                    Name = "abc",
                    SourceProvider = It.IsAny<int>(),
                    DestinationProvider = It.IsAny<int>()
                });
            _integrationPointServiceMock.Setup(x => x.ReadSlim(It.IsAny<int>()))
                .Returns(new IntegrationPointSlimDto
                {
                    Name = "abc",
                    SourceProvider = It.IsAny<int>(),
                    DestinationProvider = It.IsAny<int>()
                });

            Mock<IProviderTypeService> providerTypeService = new Mock<IProviderTypeService>();
            providerTypeService.Setup(x => x.GetProviderName(It.IsAny<int>(), It.IsAny<int>())).Returns(_PROVIDER_NAME);

            RegisterMock(jobExecutor);
            RegisterMock(jobContextProvider);
            RegisterMock(serializer);
            RegisterMock(config);
            RegisterMock(ripMetrics);
            RegisterMock(relativitySyncConstrainsChecker);
            RegisterMock(taskParameterHelper);
            RegisterMock(_integrationPointServiceMock);
            RegisterMock(_jobHistoryServiceFake);
            RegisterMock(_memoryUsageReporter);
            RegisterMock(_heartbeatReporter);
            RegisterMock(providerTypeService);
            RegisterMock(_messageServiceMock);
            RegisterMock(_kubernetesModeMock);
            RegisterMock(toggleProvider);

            SetupJobHistoryStatus(JobStatusChoices.JobHistoryPending);

            return new TestAgent(_container, _loggerMock, _kubernetesModeMock.Object);
        }

        private void RegisterMock<T>(Mock<T> mock) where T : class
        {
            _container.Register(Component.For<T>().Instance(mock.Object));
        }

        private void SetupJobHistoryStatus(ChoiceRef jobStatus)
        {
            _jobHistoryServiceFake.Setup(x => x.GetRdoWithoutDocuments(_batchInstanceGuid))
                .Returns(new JobHistory
                {
                    JobStatus = jobStatus
                });
        }

        private class TestAgent : Agent
        {
            private readonly Mock<IAPILog> _logMock;

            public TestAgent(IWindsorContainer container, Mock<IAPILog> logger, IKubernetesMode kubernetesMode)
                : base(Guid.Empty, kubernetesMode: kubernetesMode, dateTime: new DateTimeWrapper(), jobService: Substitute.For<IJobService>())
            {
                Container = container;

                JobExecutor = Container.Resolve<IJobExecutor>();

                _logMock = logger;
            }

            protected override IAPILog Logger => _logMock.Object;

            public TaskResult ProcessJob_Test(Job job) => ProcessJob(job);

            protected override IWindsorContainer CreateAgentLevelContainer() => Container;
        }
    }
}
