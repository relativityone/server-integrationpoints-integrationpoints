using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class JobHistoryBatchUpdateStatusTests : TestBase
	{
		private IJobStatusUpdater _updater;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private ISerializer _serializer;
		private IAPILog _logger;
		private JobHistoryBatchUpdateStatus _instance;
		private int _workspaceID = 100001;
		private long _jobID = 10;
		private int _jobHistoryArtifactId = 123456;
		private string _jobHistoryServiceErrorMessage = "JobHistoryService failed";

		[SetUp]
		public override void SetUp()
		{
			_updater = Substitute.For<IJobStatusUpdater>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobService = Substitute.For<IJobService>();
			_serializer = Substitute.For<ISerializer>();
			_logger = Substitute.For<IAPILog>();

			_instance = new JobHistoryBatchUpdateStatus(_updater, _jobHistoryService, _jobService, _serializer, _logger);
		}

		[Test]
		public void OnJobStart_DoNotUpdateOnStoppingJob()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);
			_jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(StopState.Stopping));

			// ACT
			_instance.OnJobStart(job);

			// ASSERT
			_jobHistoryService.DidNotReceive().UpdateRdo(Arg.Any<JobHistory>());
		}

		[TestCase(StopState.Unstoppable)]
		[TestCase(StopState.None)]
		public void OnJobStart_DoNotUpdateOnNonStoppingJob(StopState state)
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);
			TaskParameters parameters = new TaskParameters() {BatchInstance = Guid.NewGuid()};
			_jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(state));
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
			_jobHistoryService.GetRdo(parameters.BatchInstance).Returns(new JobHistory());

			// ACT
			_instance.OnJobStart(job);

			// ASSERT
			_jobHistoryService.Received(1).UpdateRdo(Arg.Is<JobHistory>(obj => obj.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing)));
		}


		[Test]
		public void OnJobComplete_UpdateTheJobStatus()
		{
			// ARRANGE
			Choice expectedStatus = JobStatusChoices.JobHistoryCompleted;
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);
			ArrangeJobComplete(expectedStatus, job);

			// ACT
			_instance.OnJobComplete(job);

			// ASSERT
			_jobHistoryService.Received(1).UpdateRdo(Arg.Is<JobHistory>(obj => obj.JobStatus.EqualsToChoice(expectedStatus)));
			_logger.DidNotReceive().LogError(Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void OnJobComplete_LogErrorWhenJobServiceFails()
		{
			// ARRANGE
			Choice expectedStatus = JobStatusChoices.JobHistoryCompleted;
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);
			ArrangeJobComplete(expectedStatus, job);
			InvalidOperationException exception = new InvalidOperationException(_jobHistoryServiceErrorMessage);
			_jobHistoryService.When(x => x.UpdateRdo(Arg.Any<JobHistory>())).Do(x =>
			{
				throw exception;
			});

			// ACT
			Assert.Throws<InvalidOperationException>(() => _instance.OnJobComplete(job));

			// ASSERT
			_jobHistoryService.Received(1).UpdateRdo(Arg.Is<JobHistory>(obj => obj.JobStatus.EqualsToChoice(expectedStatus)));
			_logger.Received(1).LogError(exception, Arg.Any<string>(), Arg.Any<object[]>());
		}

		private void ArrangeJobComplete(Choice expectedStatus, Job job)
		{
			JobHistory history = new JobHistory
			{
				ArtifactId = _jobHistoryArtifactId,
				JobStatus = JobStatusChoices.JobHistoryProcessing
			};

			TaskParameters parameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			_jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(StopState.None));
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
			_jobHistoryService.GetRdo(parameters.BatchInstance).Returns(history);
			_updater.GenerateStatus(history).Returns(expectedStatus);
		}

		[Test]
		public void OnJobComplete_LogErrorWhenGetHistoryFails()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);
			TaskParameters parameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
			_jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns((JobHistory) null);

			// ACT
			Assert.Throws<NullReferenceException>(() => _instance.OnJobComplete(job));

			// ASSERT
			_jobHistoryService.DidNotReceive().UpdateRdo(Arg.Any<JobHistory>());
			_logger.Received(1).LogError(Arg.Any<NullReferenceException>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}