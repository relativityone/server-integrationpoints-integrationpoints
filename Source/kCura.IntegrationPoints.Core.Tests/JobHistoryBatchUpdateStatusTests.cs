using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.QueryOptions;
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
		private IDateTimeHelper _dateTimeHelper;
		private JobHistoryBatchUpdateStatus _instance;
		private int _workspaceID = 100001;
		private long _jobID = 10;
		private int _jobHistoryArtifactId = 123456;
		private string _jobHistoryServiceErrorMessage = "JobHistoryService failed";

		private readonly Guid[] _queryOptionsFieldGuids =
		{
			new Guid("d3e791d3-2e21-45f4-b403-e7196bd25eea"),//Integration Point
			new Guid("5c28ce93-c62f-4d25-98c9-9a330a6feb52"),//Job Status
			new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c"),//Items Transferred
			new Guid("c224104f-c1ca-4caa-9189-657e01d5504e"),//Items with Errors
			new Guid("25b7c8ef-66d9-41d1-a8de-29a93e47fb11"),//Start Time (UTC)
			new Guid("4736cf49-ad0f-4f02-aaaa-898e07400f22"),//End Time (UTC)
			new Guid("08ba2c77-a9cd-4faf-a77a-be35e1ef1517"),//Batch Instance
			new Guid("ff01a766-b494-4f2c-9cbb-10a5ab163b8d"),//Destination Workspace
			new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b"),//Total Items
			new Guid("20a24c4e-55e8-4fc2-abbe-f75c07fad91b"),//Destination Workspace Information
			new Guid("e809db5e-5e99-4a75-98a1-26129313a3f5"),//Job Type
			new Guid("6d91ea1e-7b34-46a9-854e-2b018d4e35ef"),//Destination Instance
			new Guid("d81817dc-91cb-44c4-b9b7-7c445da64f5a"),//FilesSize
			new Guid("42d49f5e-b0e7-4632-8d30-1c6ee1d97fa7"),//Overwrite
			new Guid("77d797ef-96c9-4b47-9ef8-33f498b5af0d"),//Job ID
			new Guid("07061466-5fab-4581-979c-c801e8207370") //Name
		};

		[SetUp]
		public override void SetUp()
		{
			_updater = Substitute.For<IJobStatusUpdater>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobService = Substitute.For<IJobService>();
			_serializer = Substitute.For<ISerializer>();
			_logger = Substitute.For<IAPILog>();
			_dateTimeHelper = Substitute.For<IDateTimeHelper>();

			_instance = new JobHistoryBatchUpdateStatus(
				_updater,
				_jobHistoryService,
				_jobService,
				_serializer,
				_logger,
				_dateTimeHelper);
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
			_jobHistoryService.DidNotReceive().UpdateRdo(Arg.Any<JobHistory>(), Arg.Any<IQueryOptions>());
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
			_jobHistoryService.GetRdo(parameters.BatchInstance, Arg.Any<IQueryOptions>()).Returns(new JobHistory
			{
				ArtifactId = _jobHistoryArtifactId,
				JobStatus = JobStatusChoices.JobHistoryPending
			});

			// ACT
			_instance.OnJobStart(job);

			// ASSERT
			_jobHistoryService
				.Received(1)
				.UpdateRdo(
					Arg.Is<JobHistory>(obj => obj.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing)),
					Arg.Is<JobHistoryQueryOptions>(qo => qo.FieldGuids.SequenceEqual(_queryOptionsFieldGuids)));
		}


		[Test]
		public void OnJobComplete_UpdateTheJobStatus()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);

			Choice expectedStatus = JobStatusChoices.JobHistoryCompleted;
			var expectedEndTimeUtc = new DateTime(2010, 10, 10, 10, 10, 10);

			ArrangeJobComplete(expectedStatus, expectedEndTimeUtc, job);

			// ACT
			_instance.OnJobComplete(job);

			// ASSERT
			_jobHistoryService
				.Received(1)
				.UpdateRdo(
					Arg.Is<JobHistory>(jh => jh.JobStatus.EqualsToChoice(expectedStatus) && jh.EndTimeUTC == expectedEndTimeUtc),
					Arg.Is<JobHistoryQueryOptions>(qo => qo.FieldGuids.SequenceEqual(_queryOptionsFieldGuids)));
			_logger
				.DidNotReceive()
				.LogError(Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void OnJobComplete_LogErrorWhenJobServiceFails()
		{
			// ARRANGE
			Choice expectedStatus = JobStatusChoices.JobHistoryCompleted;
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);
			var expectedEndTimeUtc = new DateTime(2010, 10, 10, 10, 10, 10);

			ArrangeJobComplete(expectedStatus, expectedEndTimeUtc, job);
			InvalidOperationException exception = new InvalidOperationException(_jobHistoryServiceErrorMessage);
			_jobHistoryService.When(x => x.UpdateRdo(Arg.Any<JobHistory>(), Arg.Any<IQueryOptions>())).Do(x =>
			{
				throw exception;
			});

			// ACT
			Assert.Throws<InvalidOperationException>(() => _instance.OnJobComplete(job));

			// ASSERT
			_jobHistoryService
				.Received(1)
				.UpdateRdo(
					Arg.Is<JobHistory>(jh => jh.JobStatus.EqualsToChoice(expectedStatus) && jh.EndTimeUTC == expectedEndTimeUtc),
					Arg.Is<JobHistoryQueryOptions>(qo => qo.FieldGuids.SequenceEqual(_queryOptionsFieldGuids)));
			_logger
				.Received(1)
				.LogError(exception, Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void OnJobComplete_LogErrorWhenGetHistoryFails()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob(_workspaceID, _jobID);
			TaskParameters parameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
			_jobHistoryService.GetRdo(Arg.Any<Guid>(), Arg.Any<IQueryOptions>()).Returns((JobHistory)null);

			// ACT
			Assert.Throws<NullReferenceException>(() => _instance.OnJobComplete(job));

			// ASSERT
			_jobHistoryService.DidNotReceive().UpdateRdo(Arg.Any<JobHistory>());
			_logger.Received(1).LogError(Arg.Any<NullReferenceException>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		private void ArrangeJobComplete(Choice expectedStatus, DateTime expectedEndTimeUtc, Job job)
		{
			JobHistory history = new JobHistory
			{
				ArtifactId = _jobHistoryArtifactId,
				JobStatus = JobStatusChoices.JobHistoryProcessing
			};

			TaskParameters parameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
			_jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(StopState.None));
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
			_jobHistoryService.GetRdo(parameters.BatchInstance, Arg.Any<IQueryOptions>()).Returns(history);
			_updater.GenerateStatus(history).Returns(expectedStatus);
			_dateTimeHelper.Now().Returns(expectedEndTimeUtc);
		}
	}
}