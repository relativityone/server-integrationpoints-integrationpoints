using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class JobStopManagerTests : TestBase
	{
		private JobStopManager _instance;
		private IJobService _jobService;
		private IJobHistoryService _jobHistoryService;
		private Guid _guid;
		private int _jobId;
		private JobHistory _jobHistory;

		[SetUp]
		public override void SetUp()
		{
			_jobHistory = new JobHistory();
			_jobService = Substitute.For<IJobService>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			var helper = Substitute.For<IHelper>();
			_guid = Guid.NewGuid();
			_jobId = 123;
			_instance = new JobStopManager(_jobService, _jobHistoryService, helper, _guid, _jobId);
		}

		[Test]
		public void IsStopRequested_UnableToFindTheJob()
		{
			// act
			_instance.Callback.Invoke(null);
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		[TestCase(StopState.None)]
		[TestCase(StopState.Unstoppable)]
		public void IsStopRequested_NoStoppingJob(StopState stopState)
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(stopState);
			_jobService.GetJob(_jobId).Returns(job);

			// act
			_instance.Callback.Invoke(null);
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		private static Choice[] JobHistoryStatuses = new []
		{
			JobStatusChoices.JobHistoryPending,
			JobStatusChoices.JobHistoryProcessing
		};

		[TestCaseSource(nameof(JobHistoryStatuses))]
		public void IsStopRequested_StoppingJob(Choice status)
		{
			// arrange
			_jobHistory.JobStatus = status;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.GetJob(_jobId).Returns(job);
			_jobHistoryService.GetRdo(_guid).Returns(_jobHistory);

			// act
			_instance.Callback.Invoke(null);
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			_jobHistoryService.Received(1).UpdateRdo(Arg.Is<JobHistory>(history => history.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopping)));
			Assert.IsTrue(isStopRequested);
		}

		[Test]
		public void IsStopRequested_JobServiceFailsToGetTheJob()
		{
			// arrange
			_jobService.GetJob(_jobId).Throws(new Exception("something"));

			// act
			_instance.Callback.Invoke(null);
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		[Test]
		public void IsStopRequested_JobHistoryServiceFailsToGetTheJob()
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState (StopState.Stopping);
			_jobService.GetJob(_jobId).Returns(job);
			_jobHistoryService.GetRdo(_guid).Throws(new Exception("something"));

			// act
			_instance.Callback.Invoke(null);
			bool isStopRequested = _instance.IsStopRequested();

			// assert
			Assert.IsFalse(isStopRequested);
		}

		[Test]
		public void ThrowIfStopRequested_ThrowExceptionWhenStop()
		{
			// arrange
			_jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.GetJob(_jobId).Returns(job);
			_jobHistoryService.GetRdo(_guid).Returns(_jobHistory);
			_instance.Callback.Invoke(null);

			// act & assert
			Assert.Throws<OperationCanceledException>(() =>	_instance.ThrowIfStopRequested());
		}

		[Test]
		public void StopRequestedEvent_RaisesWhenStop()
		{
			// arrange
			var eventTriggered = false;
			_instance.StopRequestedEvent += (sender, args) => eventTriggered = true;
			_jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.Stopping);
			_jobService.GetJob(_jobId).Returns(job);
			_jobHistoryService.GetRdo(_guid).Returns(_jobHistory);
			_instance.Callback.Invoke(null);

			// act & assert
			Assert.True(eventTriggered);
		}

		[Test]
		public void ThrowIfStopRequested_DoNotThrowExceptionWhenRunning()
		{
			// arrange
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.None);
			_jobService.GetJob(_jobId).Returns(job);
			_jobHistoryService.GetRdo(_guid).Returns(_jobHistory);
			_instance.Callback.Invoke(null);

			// act & assert
			Assert.DoesNotThrow(() => _instance.ThrowIfStopRequested());
		}

		[Test]
		public void StopRequestedEvent_DoesntRaiseWhenRunning()
		{
			// arrange
			var eventTriggered = false;
			_instance.StopRequestedEvent += (sender, args) => eventTriggered = true;
			Job job = JobExtensions.CreateJob();
			job = job.CopyJobWithStopState(StopState.None);
			_jobService.GetJob(_jobId).Returns(job);
			_jobHistoryService.GetRdo(_guid).Returns(_jobHistory);
			_instance.Callback.Invoke(null);

			// act & assert
			Assert.False(eventTriggered);
		}

		[Test]
		public void Dispose_CorrectDisposePattern()
		{
			// arrange

			// act & assert
			Assert.DoesNotThrow(() => _instance.Dispose());
			Assert.DoesNotThrow(() => _instance.Dispose());
		}
	}
}