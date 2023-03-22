using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class JobStopManagerTests : TestBase
    {
        private Mock<IJobService> _jobServiceMock;
        private Mock<IJobHistoryService> _jobHistoryServiceMock;
        private Mock<IRemovableAgent> _agentFake;
        private Mock<IHelper> _helperFake;
        private Guid _jobHistoryInstanceGuid;
        private int _jobId;
        private JobHistory _jobHistory;

        [SetUp]
        public override void SetUp()
        {
            _jobHistory = new JobHistory()
            {
                JobStatus = JobStatusChoices.JobHistoryProcessing
            };
            _jobServiceMock = new Mock<IJobService>();
            _jobHistoryServiceMock = new Mock<IJobHistoryService>();
            _agentFake = new Mock<IRemovableAgent>();
            _jobHistoryInstanceGuid = Guid.NewGuid();
            _jobId = 123;
            _helperFake = new Mock<IHelper>();
            _helperFake.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<JobStopManager>()).Returns(Mock.Of<IAPILog>());
        }

        private JobStopManager PrepareSut(bool supportsDrainStop, CancellationTokenSource stopToken = null, CancellationTokenSource drainStopToken = null)
        {
            return new JobStopManager(
                _jobServiceMock.Object,
                _jobHistoryServiceMock.Object,
                _helperFake.Object,
                _jobHistoryInstanceGuid,
                _jobId,
                _agentFake.Object,
                supportsDrainStop,
                stopToken ?? new CancellationTokenSource(),
                drainStopToken ?? new CancellationTokenSource(),
                new EmptyDiagnosticLog());
        }

        private Job PrepareJob(StopState stopState = StopState.None)
        {
            Job job = JobExtensions.CreateJob().CopyJobWithStopState(stopState).CopyJobWithJobId(_jobId);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);
            _jobHistoryServiceMock.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);
            return job;
        }

        [Test]
        public void TerminateIfRequested_ShouldNotStopJob_WhenDrainStopOccurrs()
        {
            // Arrange
            Job job = PrepareJob();
            CancellationTokenSource stopToken = new CancellationTokenSource();
            _agentFake.SetupGet(x => x.ToBeRemoved).Returns(true);

            JobStopManager sut = PrepareSut(true, stopToken);

            bool stopRequested = false;
            sut.StopRequestedEvent += (sender, args) => stopRequested = true;

            // Act
            sut.TerminateIfRequested();

            // Assert
            stopToken.IsCancellationRequested.Should().BeFalse();
            stopRequested.Should().BeFalse();
        }

        [Test]
        public void TerminateIfRequested_ShouldSignalDrainStopAndUpdateJobState_WhenAgentIsToBeRemoved()
        {
            // Arrange
            _agentFake.SetupGet(x => x.ToBeRemoved).Returns(true);
            Job job = PrepareJob();
            CancellationTokenSource drainStopToken = new CancellationTokenSource();
            JobStopManager sut = PrepareSut(true, new CancellationTokenSource(), drainStopToken);

            // Act
            sut.TerminateIfRequested();

            // Assert
            drainStopToken.IsCancellationRequested.Should().BeTrue();
            _jobServiceMock.Verify(x => x.UpdateStopState(
                It.Is<IList<long>>(jobs=> jobs.Single() == _jobId),
                It.Is<StopState>(stopState => stopState == StopState.DrainStopping)), Times.Once);
        }

        [Test]
        public void TerminateIfRequested_ShouldNotDoAnything_WhenJobDrainStopIsInProgress()
        {
            // Arrange
            _agentFake.SetupGet(x => x.ToBeRemoved).Returns(true);
            Job job = PrepareJob(StopState.DrainStopping);
            _jobHistory.JobStatus = JobStatusChoices.JobHistorySuspending;
            JobStopManager sut = PrepareSut(true);

            // Act
            sut.TerminateIfRequested();
            sut.TerminateIfRequested();
            sut.TerminateIfRequested();

            // Assert
            _jobServiceMock.Verify(x => x.UpdateStopState(It.IsAny<IList<long>>(), It.IsAny<StopState>()), Times.Never);
            _jobHistoryServiceMock.Verify(x => x.UpdateRdoWithoutDocuments(It.IsAny<JobHistory>()), Times.Never);
        }

        [Test]
        public void IsStopRequested_UnableToFindTheJob()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);

            // act
            sut.Execute();
            bool isStopRequested = sut.IsStopRequested();

            // assert
            Assert.IsFalse(isStopRequested);
        }

        [TestCase(StopState.None)]
        [TestCase(StopState.Unstoppable)]
        public void IsStopRequested_NoStoppingJob(StopState stopState)
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            Job job = JobExtensions.CreateJob();
            job = job.CopyJobWithStopState(stopState);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);

            // act
            sut.Execute();
            bool isStopRequested = sut.IsStopRequested();

            // assert
            Assert.IsFalse(isStopRequested);
        }

        [TestCaseSource(nameof(JobHistoryStatuses))]
        public void IsStopRequested_StoppingJob(ChoiceRef status)
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            _jobHistory.JobStatus = status;
            Job job = PrepareJob(StopState.Stopping);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);
            _jobHistoryServiceMock.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);

            // act
            sut.Execute();
            bool isStopRequested = sut.IsStopRequested();

            // assert
            _jobHistoryServiceMock.Verify(x => x.UpdateRdoWithoutDocuments(It.Is<JobHistory>(history => history.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopping))));
            Assert.IsTrue(isStopRequested);
        }

        [Test]
        public void IsStopRequested_JobServiceFailsToGetTheJob()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Throws(new Exception("something"));

            // act
            sut.Execute();
            bool isStopRequested = sut.IsStopRequested();

            // assert
            Assert.IsFalse(isStopRequested);
        }

        [Test]
        public void IsStopRequested_JobHistoryServiceFailsToGetTheJob()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            Job job = PrepareJob(StopState.Stopping);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);
            _jobHistoryServiceMock.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Throws(new Exception("something"));

            // act
            sut.Execute();
            bool isStopRequested = sut.IsStopRequested();

            // assert
            Assert.IsFalse(isStopRequested);
        }

        [Test]
        public void ThrowIfStopRequested_ThrowExceptionWhenStop()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            _jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
            Job job = PrepareJob(StopState.Stopping);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);
            _jobHistoryServiceMock.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);
            sut.Execute();

            // act & assert
            Assert.Throws<OperationCanceledException>(() =>    sut.ThrowIfStopRequested());
        }

        [Test]
        public void StopRequestedEvent_RaisesWhenStop()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            bool eventTriggered = false;
            sut.StopRequestedEvent += (sender, args) => eventTriggered = true;
            _jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
            Job job = PrepareJob(StopState.Stopping);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);
            _jobHistoryServiceMock.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);
            sut.Execute();

            // act & assert
            Assert.True(eventTriggered);
        }

        [Test]
        public void ThrowIfStopRequested_DoNotThrowExceptionWhenRunning()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            Job job = PrepareJob(StopState.None);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);
            _jobHistoryServiceMock.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);
            sut.Execute();

            // act & assert
            Assert.DoesNotThrow(() => sut.ThrowIfStopRequested());
        }

        [Test]
        public void StopRequestedEvent_DoesntRaiseWhenRunning()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);
            bool eventTriggered = false;
            sut.StopRequestedEvent += (sender, args) => eventTriggered = true;
            Job job = PrepareJob(StopState.None);
            _jobServiceMock.Setup(x => x.GetJob(_jobId)).Returns(job);
            _jobHistoryServiceMock.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);
            sut.Execute();

            // act & assert
            Assert.False(eventTriggered);
        }

        [Test]
        public void Dispose_CorrectDisposePattern()
        {
            // arrange
            JobStopManager sut = PrepareSut(false);

            // act & assert
            Assert.DoesNotThrow(() => sut.Dispose());
            Assert.DoesNotThrow(() => sut.Dispose());
        }

        private static ChoiceRef[] JobHistoryStatuses = new[]
        {
            JobStatusChoices.JobHistoryPending,
            JobStatusChoices.JobHistoryProcessing
        };

    }
}
