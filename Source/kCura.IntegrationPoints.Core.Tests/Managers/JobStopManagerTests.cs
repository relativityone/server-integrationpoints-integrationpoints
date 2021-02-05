using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture, Category("Unit")]
	public class JobStopManagerTests : TestBase
	{
		private Mock<IJobService> _jobService;
		private Mock<IJobHistoryService> _jobHistoryService;
		private Mock<IJobServiceDataProvider> _jobServiceDataProvider;
		private Mock<IRemovableAgent> _agent;
		private Mock<IHelper> _helper;
		private Guid _jobHistoryInstanceGuid;
		private int _jobId;
		private JobHistory _jobHistory;

		[SetUp]
		public override void SetUp()
		{
			_jobHistory = new JobHistory();
			_jobService = new Mock<IJobService>();
			_jobHistoryService = new Mock<IJobHistoryService>();
			_jobServiceDataProvider = new Mock<IJobServiceDataProvider>();
			_agent = new Mock<IRemovableAgent>();
			_jobHistoryInstanceGuid = Guid.NewGuid();
			_jobId = 123;
			_helper = new Mock<IHelper>();
			_helper.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<JobStopManager>()).Returns(Mock.Of<IAPILog>());
		}

		private JobStopManager PrepareSut(bool supportsDrainStop, CancellationTokenSource stopToken = null, CancellationTokenSource drainStopToken = null)
		{
			return new JobStopManager(_jobService.Object, _jobHistoryService.Object, _jobServiceDataProvider.Object,
				_helper.Object, _jobHistoryInstanceGuid, _jobId, _agent.Object, supportsDrainStop,
				stopToken ?? new CancellationTokenSource(), drainStopToken ?? new CancellationTokenSource());
		}

		private Job PrepareJob(StopState stopState = StopState.None)
		{
			Job job = JobExtensions.CreateJob().CopyJobWithStopState(stopState).CopyJobWithJobId(_jobId);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);
			return job;
		}

		[Test]
		public void ProcessJob_ShouldNotStopJob_WhenDrainStopOccurrs()
		{
			// Arrange
			Job job = PrepareJob();
			CancellationTokenSource stopToken = new CancellationTokenSource();
			_agent.SetupGet(x => x.ToBeRemoved).Returns(true);

			JobStopManager sut = PrepareSut(true, stopToken);

			bool stopRequested = false;
			sut.StopRequestedEvent += (sender, args) => stopRequested = true;

			// Act
			sut.ProcessJob(job);

			// Assert
			stopToken.IsCancellationRequested.Should().BeFalse();
			stopRequested.Should().BeFalse();
		}

		[Test]
		public void ProcessJob_ShouldSignalDrainStopAndUpdateJobState_WhenAgentIsToBeRemoved()
		{
			// Arrange
			_agent.SetupGet(x => x.ToBeRemoved).Returns(true);
			Job job = PrepareJob();
			CancellationTokenSource drainStopToken = new CancellationTokenSource();
			JobStopManager sut = PrepareSut(true, new CancellationTokenSource(), drainStopToken);

			// Act
			sut.ProcessJob(job);

			// Assert
			drainStopToken.IsCancellationRequested.Should().BeTrue();
			_jobService.Verify(x => x.UpdateStopState(
				It.Is<IList<long>>(jobs=> jobs.Single() == _jobId),
				It.Is<StopState>(stopState => stopState == StopState.DrainStopping)), Times.Once);
			_jobHistoryService.Verify(x => x.UpdateRdoWithoutDocuments(It.Is<JobHistory>(jobHistory =>
				jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistorySuspending))), Times.Once);
		}

		[Test]
		public void ProcessJob_ShouldNotDoAnything_WhenJobDrainStopIsInProgress()
		{
			// Arrange
			_agent.SetupGet(x => x.ToBeRemoved).Returns(true);
			Job job = PrepareJob(StopState.DrainStopping);
			_jobHistory.JobStatus = JobStatusChoices.JobHistorySuspending;
			JobStopManager sut = PrepareSut(true);

			// Act
			sut.ProcessJob(job);
			sut.ProcessJob(job);
			sut.ProcessJob(job);

			// Assert
			_jobService.Verify(x => x.UpdateStopState(It.IsAny<IList<long>>(), It.IsAny<StopState>()), Times.Never);
			_jobHistoryService.Verify(x => x.UpdateRdoWithoutDocuments(It.IsAny<JobHistory>()), Times.Never);
		}

		[Test]
		public void ProcessJob_ShouldUpdateStopStateAndUnlockJob_WhenJobIsDrainStopped()
		{
			// Arrange
			_agent.SetupGet(x => x.ToBeRemoved).Returns(true);
			Job job = PrepareJob(StopState.DrainStopping);
			_jobHistory.JobStatus = JobStatusChoices.JobHistorySuspended;
			JobStopManager sut = PrepareSut(true);

			// Act
			sut.ProcessJob(job);

			// Assert
			_jobService.Verify(x => x.UpdateStopState(
				It.Is<IList<long>>(jobs => jobs.Single() == _jobId),
				It.Is<StopState>(stopState => stopState == StopState.DrainStopped)));
			_jobServiceDataProvider.Verify(x => x.UnlockJob(_jobId), Times.Once);
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
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);

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
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Returns(_jobHistory);

			// act
			sut.Execute();
			bool isStopRequested = sut.IsStopRequested();

			// assert
			_jobHistoryService.Verify(x => x.UpdateRdoWithoutDocuments(It.Is<JobHistory>(history => history.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopping))));
			Assert.IsTrue(isStopRequested);
		}

		[Test]
		public void IsStopRequested_JobServiceFailsToGetTheJob()
		{
			// arrange
			JobStopManager sut = PrepareSut(false);
			_jobService.Setup(x => x.GetJob(_jobId)).Throws(new Exception("something"));

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
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState (StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdoWithoutDocuments(_jobHistoryInstanceGuid)).Throws(new Exception("something"));

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
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
			sut.Execute();

			// act & assert
			Assert.Throws<OperationCanceledException>(() =>	sut.ThrowIfStopRequested());
		}

		[Test]
		public void StopRequestedEvent_RaisesWhenStop()
		{
			// arrange
			JobStopManager sut = PrepareSut(false);
			bool eventTriggered = false;
			sut.StopRequestedEvent += (sender, args) => eventTriggered = true;
			_jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
			sut.Execute();

			// act & assert
			Assert.True(eventTriggered);
		}

		[Test]
		public void ThrowIfStopRequested_DoNotThrowExceptionWhenRunning()
		{
			// arrange
			JobStopManager sut = PrepareSut(false);
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.None);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
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
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.None);
			_jobService.Setup(x => x.GetJob(_jobId)).Returns(job);
			_jobHistoryService.Setup(x => x.GetRdo(_jobHistoryInstanceGuid)).Returns(_jobHistory);
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