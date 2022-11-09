using System;
using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using Moq;
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
            _sut = new JobExecutor(_taskProviderFake.Object, agentNotifier.Object, _jobServiceFake.Object, _logMock.Object);
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
    }
}
