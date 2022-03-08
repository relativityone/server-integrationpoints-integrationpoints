using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Choice;
using System;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.AgentBase;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture, Category("Unit")]
	public class AgentTests
	{
		private Mock<IMessageService> _messageServiceMock;
		private Mock<IJobHistoryService> _jobHistoryServiceFake;
		private Mock<IMemoryUsageReporter> _memoryUsageReporter;
		private Mock<IAPILog> _loggerMock;

		private IWindsorContainer _container;

		private const string _PROVIDER_NAME = "TestProvider";
		private readonly Guid _BATCH_INSTANCE_GUID = Guid.NewGuid();
		private Mock<IKubernetesMode> _kubernetesModeMock;

		[SetUp]
		public void SetUp()
		{
			_messageServiceMock = new Mock<IMessageService>();
			_jobHistoryServiceFake = new Mock<IJobHistoryService>();
			_memoryUsageReporter = new Mock<IMemoryUsageReporter>();
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
			_memoryUsageReporter.Verify(x => x.ActivateTimer(
				It.Is<int>(timeInterval => timeInterval == 30*1000), 
				It.IsAny<long>(), 
				It.IsAny<string>(), 
				It.Is<string>(jobType => jobType =="ExportService")), Times.Once);
		}

		[Test]
		public void ProcessJob_ShouldNotThrow_WhenJobHistoryDoesNotExist()
		{
			// Arrange
			TestAgent sut = PrepareSut();

			_jobHistoryServiceFake.Setup(x => x.GetRdoWithoutDocuments(_BATCH_INSTANCE_GUID))
				.Returns((JobHistory)null);

			Job job = JobExtensions.CreateJob();

			// Act
			Action action = () => sut.ProcessJob_Test(job);

			// Assert
			action.ShouldNotThrow();
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
			_loggerMock.Verify(x => x.LogDebug("Attempting to initialize Config Settings factory in {TypeName}",
				nameof(ScheduleQueueAgentBase)), Times.Never);
		}

		private TestAgent PrepareSut()
		{
			_container = new WindsorContainer();

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
			config.SetupGet(x => x.RelativityWebApiTimeout).Returns((TimeSpan?)null);

			Mock<IRelativitySyncConstrainsChecker> relativitySyncConstrainsChecker = new Mock<IRelativitySyncConstrainsChecker>();
			relativitySyncConstrainsChecker.Setup(x => x.ShouldUseRelativitySync(It.IsAny<Job>())).Returns(false);

			Mock<ITaskParameterHelper> taskParameterHelper = new Mock<ITaskParameterHelper>();
			taskParameterHelper.Setup(x => x.GetBatchInstance(It.IsAny<Job>())).Returns(_BATCH_INSTANCE_GUID);

			Mock<IIntegrationPointService> integrationPointService = new Mock<IIntegrationPointService>();
			integrationPointService.Setup(x => x.ReadIntegrationPoint(It.IsAny<int>()))
				.Returns(new Data.IntegrationPoint
				{
					SourceProvider = It.IsAny<int>(),
					DestinationProvider = It.IsAny<int>()
				});

			Mock<IProviderTypeService> providerTypeService = new Mock<IProviderTypeService>();
			providerTypeService.Setup(x => x.GetProviderName(It.IsAny<int>(), It.IsAny<int>())).Returns(_PROVIDER_NAME);

			RegisterMock(jobExecutor);
			RegisterMock(jobContextProvider);
			RegisterMock(serializer);
			RegisterMock(config);
			RegisterMock(relativitySyncConstrainsChecker);
			RegisterMock(taskParameterHelper);
			RegisterMock(integrationPointService);
			RegisterMock(_jobHistoryServiceFake);
			RegisterMock(_memoryUsageReporter);
			RegisterMock(providerTypeService);
			RegisterMock(_messageServiceMock);
			RegisterMock(_kubernetesModeMock);

			SetupJobHistoryStatus(JobStatusChoices.JobHistoryPending);

			return new TestAgent(_container, _loggerMock, _kubernetesModeMock.Object);
		}

		private void RegisterMock<T>(Mock<T> mock) where T: class
		{
			_container.Register(Component.For<T>().Instance(mock.Object));
		}

		private void SetupJobHistoryStatus(ChoiceRef jobStatus)
		{
			_jobHistoryServiceFake.Setup(x => x.GetRdoWithoutDocuments(_BATCH_INSTANCE_GUID))
				.Returns(new JobHistory
				{
					JobStatus = jobStatus
				});
		}

		private class TestAgent : Agent
		{
			private readonly Mock<IAPILog> _logMock;

			public IWindsorContainer Container { get; }

			public TestAgent(IWindsorContainer container, Mock<IAPILog> logger, IKubernetesMode kubernetesMode) : base(Guid.Empty, kubernetesMode: kubernetesMode)
			{
				Container = container;
				
				JobExecutor = Container.Resolve<IJobExecutor>();

				_logMock = logger;
			}

			public TaskResult ProcessJob_Test(Job job) => ProcessJob(job);

			protected override IWindsorContainer CreateAgentLevelContainer() => Container;

			protected override IAPILog Logger => _logMock.Object;

		}
	}
}
