using System;
using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture, Category("Unit")]
	public class JobExecutorTests
	{
		private IJobExecutor _sut;

		private Mock<IAPILog> _logMock;
		private Mock<ITaskProvider> _taskProviderFake;
		private Mock<IJobService> _jobServiceFake;
		private Mock<ITaskParameterHelper> _taskParameterFake;

		private const string _RIP_PREFIX = "RIP.";

		[SetUp]
		public void SetUp()
		{
			_logMock = new Mock<IAPILog>();

			Mock<ITask> task = new Mock<ITask>();
			
			_taskProviderFake = new Mock<ITaskProvider>();
			_taskProviderFake.Setup(x => x.GetTask(It.IsAny<Job>()))
				.Returns(task.Object);

			Mock<IAgentNotifier> agentNotifier = new Mock<IAgentNotifier>();
			_jobServiceFake = new Mock<IJobService>();
			_taskParameterFake = new Mock<ITaskParameterHelper>();
			_sut = new JobExecutor(_taskProviderFake.Object, agentNotifier.Object, _jobServiceFake.Object, _taskParameterFake.Object, _logMock.Object);
		}
		
		[Test]
		public void ProcessJob_ShouldPushEmptyRootJobIdToLogContext_WhenRootJobIdIsNull()
		{
			// Arrange
			Job job = new JobBuilder()
				.WithRootJobId(null)
				.Build();

			// Act
			_sut.ProcessJob(job);

			// Assert
			_logMock.Verify(x => x.LogContextPushProperty($"{_RIP_PREFIX}{nameof(AgentCorrelationContext.RootJobId)}", string.Empty));
		}

		[Test]
		public void ProcessJob_ShouldRegisterProperLogContext()
		{
			// Arrange
			string expectedVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
			const long expectedJobId = 123;
			const long expectedRootJobId = 879;
			const int expectedUserId = 9;
			const int expectedWorkspaceId = 11;

			Job job = new JobBuilder()
				.WithJobId(expectedJobId)
				.WithRootJobId(expectedRootJobId)
				.WithSubmittedBy(expectedUserId)
				.WithWorkspaceId(expectedWorkspaceId)
				.Build();

			// Act
			_sut.ProcessJob(job);

			// Assert
			VerifyLoggerJobContextProperty(nameof(AgentCorrelationContext.JobId), expectedJobId);
			VerifyLoggerJobContextProperty(nameof(AgentCorrelationContext.ApplicationBuildVersion), expectedVersion);
			VerifyLoggerJobContextProperty(nameof(AgentCorrelationContext.RootJobId), expectedRootJobId);
			VerifyLoggerJobContextProperty(nameof(AgentCorrelationContext.UserId), expectedUserId);
			VerifyLoggerJobContextProperty(nameof(AgentCorrelationContext.WorkspaceId), expectedWorkspaceId);
		}

		[Test]
		public void ProcessJob_ShouldReturnedDrainStoppedStatus_WhenJobStopStateIsDrainStopping()
		{
			// Arrange
			Job job = new JobBuilder().Build();

			_jobServiceFake.Setup(x => x.GetJob(job.JobId))
			.Returns(job.CopyJobWithStopState(StopState.DrainStopping));

			// Act
			TaskResult result = _sut.ProcessJob(job);

			// Assert
			result.Status.Should().Be(TaskStatusEnum.DrainStopped);
		}

		[Test]
		public void ProcessJob_ShouldReturnedDrainStoppedStatus_WhenJobStopStateIsDrainStopped()
		{
			// Arrange
			Job job = new JobBuilder().Build();

			_jobServiceFake.Setup(x => x.GetJob(job.JobId)).Returns(job.CopyJobWithStopState(StopState.DrainStopped));

			// Act
			TaskResult result = _sut.ProcessJob(job);

			// Assert
			result.Status.Should().Be(TaskStatusEnum.DrainStopped);
		}

		[Test]
		public void ProcessJob_ShouldReturnFailedStatus_WhenGetTaskThrowsException()
		{
			// Arrange
			Job job = new JobBuilder().Build();

			_taskProviderFake.Setup(x => x.GetTask(job))
				.Throws<Exception>();

			// Act
			TaskResult result = _sut.ProcessJob(job);

			// Assert
			result.Status.Should().Be(TaskStatusEnum.Fail);
		}

		[Test]
		public void ProcessJob_ShouldReturnFailedStatus_WhenTaskExecuteThrowsException()
		{
			// Arrange
			Job job = new JobBuilder().Build();

			Mock<ITask> task = new Mock<ITask>();
			task.Setup(x => x.Execute(job))
				.Throws<Exception>();

			_taskProviderFake.Setup(x => x.GetTask(job))
				.Returns(task.Object);

			// Act
			TaskResult result = _sut.ProcessJob(job);

			// Assert
			result.Status.Should().Be(TaskStatusEnum.Fail);
		}

		private void VerifyLoggerJobContextProperty(string name, object value)
		{
			string contextFormat = $"{_RIP_PREFIX}{name}";
			_logMock.Verify(x => x.LogContextPushProperty(contextFormat, value.ToString()));
		}
	}
}
