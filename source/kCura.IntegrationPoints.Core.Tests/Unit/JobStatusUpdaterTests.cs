using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class JobStatusUpdaterTests
	{
		private IJobHistoryService _jobHistoryService;

		public void SetUp()
		{
			_jobHistoryService = Substitute.For<IJobHistoryService>();
		}

		[Test]
		public void GenerateStatus_NoJobHistoryErrors_ReturnsSuccess()
		{
			//ARRANGE
			var rsapi = NSubstitute.Substitute.For<IRSAPIService>();
			var service = NSubstitute.Substitute.For<JobHistoryErrorQuery>(rsapi);
			service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(null, new JobHistoryError[] { });

			//ACT
			var serviceInTest = new JobStatusUpdater(service, _jobHistoryService);
			var choice = serviceInTest.GenerateStatus(new JobHistory { JobStatus = JobStatusChoices.JobHistoryProcessing, ItemsWithErrors = 0 });
			//ASSERT
			Assert.IsTrue(choice.Name.Equals(Data.JobStatusChoices.JobHistoryCompleted.Name));
		}

		[Test]
		public void GenerateStatus_JobHistoryItemError_ReturnsCompletedWithErrors()
		{
			//ARRANGE
			var rsapi = NSubstitute.Substitute.For<IRSAPIService>();
			var service = NSubstitute.Substitute.For<JobHistoryErrorQuery>(rsapi);
			service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(new JobHistoryError { ErrorType = Data.ErrorTypeChoices.JobHistoryErrorItem });

			//ACT
			var serviceInTest = new JobStatusUpdater(service, _jobHistoryService);
			var choice = serviceInTest.GenerateStatus(new JobHistory() { JobStatus = JobStatusChoices.JobHistoryProcessing });

			//ASSERT
			Assert.IsTrue(choice.Name.Equals(Data.JobStatusChoices.JobHistoryCompletedWithErrors.Name));
		}

		[Test]
		public void GenerateStatus_JobHistoryJobError_ReturnsErrorJob()
		{
			//ARRANGE
			var rsapi = NSubstitute.Substitute.For<IRSAPIService>();
			var service = NSubstitute.Substitute.For<JobHistoryErrorQuery>(rsapi);
			service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(new JobHistoryError { ErrorType = Data.ErrorTypeChoices.JobHistoryErrorJob });

			//ACT
			var serviceInTest = new JobStatusUpdater(service, _jobHistoryService);
			var choice = serviceInTest.GenerateStatus(new JobHistory() { JobStatus = JobStatusChoices.JobHistoryProcessing});

			//ASSERT
			Assert.IsTrue(choice.Name.Equals(Data.JobStatusChoices.JobHistoryErrorJobFailed.Name));
		}
	}
}