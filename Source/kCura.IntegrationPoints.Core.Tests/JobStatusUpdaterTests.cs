using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class JobStatusUpdaterTests
	{
		private IJobHistoryService _jobHistoryService;
		private IRSAPIService _rsapi;
		private JobHistoryErrorQuery _service;
		private JobStatusUpdater _instance;

		[SetUp]
		public void SetUp()
		{
			_rsapi = Substitute.For<IRSAPIService>();
			_service = Substitute.For<JobHistoryErrorQuery>(_rsapi);
			_jobHistoryService = Substitute.For<IJobHistoryService>();

			_instance = new JobStatusUpdater(_service, _jobHistoryService);
		}

		[Test]
		public void GenerateStatus_NullJobHistory()
		{
			Assert.Throws<ArgumentNullException>(() => _instance.GenerateStatus(null));
		}

		[Test]
		public void GenerateStatus_StoppingHistory()
		{
			// ARRANGE 
			JobHistory jobHistory = new JobHistory() {JobStatus = JobStatusChoices.JobHistoryStopping};

			// ACT
			Relativity.Client.Choice status = _instance.GenerateStatus(jobHistory);

			// ASSERT
			Assert.IsTrue(status.EqualsToChoice(JobStatusChoices.JobHistoryStopped));
		}

		[Test]
		public void GenerateStatus_NoJobHistoryErrors_ReturnsSuccess()
		{
			//ARRANGE
			_service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(null, new JobHistoryError[] { });

			//ACT
			var choice = _instance.GenerateStatus(new JobHistory { JobStatus = JobStatusChoices.JobHistoryProcessing, ItemsWithErrors = 0 });
			
			//ASSERT
			Assert.IsTrue(choice.EqualsToChoice(JobStatusChoices.JobHistoryCompleted));
		}

		[Test]
		public void GenerateStatus_JobHistoryItemError_ReturnsCompletedWithErrors()
		{
			//ARRANGE
			_service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(new JobHistoryError { ErrorType = Data.ErrorTypeChoices.JobHistoryErrorItem });

			//ACT
			var choice = _instance.GenerateStatus(new JobHistory() { JobStatus = JobStatusChoices.JobHistoryProcessing });

			//ASSERT
			Assert.IsTrue(choice.EqualsToChoice(JobStatusChoices.JobHistoryCompletedWithErrors));
		}

		[Test]
		public void GenerateStatus_JobHistoryItemError_NoRecentJobError()
		{
			//ARRANGE
			_service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns((JobHistoryError)null);

			//ACT
			var choice = _instance.GenerateStatus(new JobHistory() { JobStatus = JobStatusChoices.JobHistoryProcessing, ItemsWithErrors = 99 });

			//ASSERT
			Assert.IsTrue(choice.EqualsToChoice(JobStatusChoices.JobHistoryCompletedWithErrors));
		}

		[Test]
		public void GenerateStatus_RecentJobErrorContainsInvalidErrorStatus()
		{
			//ARRANGE
			_service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(new JobHistoryError { ErrorType = null});

			//ACT
			var choice = _instance.GenerateStatus(new JobHistory() { JobStatus = JobStatusChoices.JobHistoryProcessing });

			//ASSERT
			Assert.IsTrue(choice.EqualsToChoice(JobStatusChoices.JobHistoryCompleted));
		}

		[Test]
		public void GenerateStatus_JobHistoryJobError_ReturnsErrorJob()
		{
			//ARRANGE
			_service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(new JobHistoryError { ErrorType = Data.ErrorTypeChoices.JobHistoryErrorJob });

			//ACT
			var choice = _instance.GenerateStatus(new JobHistory() { JobStatus = JobStatusChoices.JobHistoryProcessing});

			//ASSERT
			Assert.IsTrue(choice.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed));
		}

		[Test]
		public void GenerateStatusWithBatchInstanceId()
		{
			// ARRANGE
			Guid guid = Guid.NewGuid();
			JobHistory jobHistory = new JobHistory() {JobStatus = JobStatusChoices.JobHistoryStopping};
			_jobHistoryService.GetRdo(guid).Returns(jobHistory);

			// ACT
			var choice = _instance.GenerateStatus(guid);

			//ASSERT
			Assert.IsTrue(choice.EqualsToChoice(JobStatusChoices.JobHistoryStopped));
		}
	}
}