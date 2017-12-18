using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class JobHistoryBatchUpdateStatusTests : TestBase
	{
		private IJobStatusUpdater _updater;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private ISerializer _serializer;
		private JobHistoryBatchUpdateStatus _instance;

		[SetUp]
		public override void SetUp()
		{
			_updater = Substitute.For<IJobStatusUpdater>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobService = Substitute.For<IJobService>();
			_serializer = Substitute.For<ISerializer>();

			_instance = new JobHistoryBatchUpdateStatus(_updater, _jobHistoryService, _jobService, _serializer);
		}

		[Test]
		public void OnJobStart_DoNotUpdateOnStoppingJob()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
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
			Job job = JobExtensions.CreateJob();
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
			Job job = JobExtensions.CreateJob();
			JobHistory history = new JobHistory();
			var expectedStatus = JobStatusChoices.JobHistoryCompleted;
			TaskParameters parameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			_jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(StopState.None));
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
			_jobHistoryService.GetRdo(parameters.BatchInstance).Returns(history);
			_updater.GenerateStatus(history, job.JobId).Returns(expectedStatus);

			// ACT
			_instance.OnJobComplete(job);

			// ASSERT
			_jobHistoryService.Received(1).UpdateRdo(Arg.Is<JobHistory>(obj => obj.JobStatus.EqualsToChoice(expectedStatus) ));
		}
	}
}