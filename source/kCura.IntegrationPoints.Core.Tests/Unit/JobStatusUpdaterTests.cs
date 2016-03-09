using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class JobStatusUpdaterTests
	{
		[Test]
		public void GenerateStatus_NoJobHistoryErrors_ReturnsSuccess()
		{
			//ARRANGE
			var rsapi = NSubstitute.Substitute.For<IRSAPIService>();
			var service = NSubstitute.Substitute.For<JobHistoryErrorQuery>(rsapi);
			service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(null, new JobHistoryError[] { });

			//ACT
			var serviceInTest = new JobStatusUpdater(service, null);
			var choice = serviceInTest.GenerateStatus(new JobHistory { ItemsWithErrors = 0 });
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
			var serviceInTest = new JobStatusUpdater(service, null);
			var choice = serviceInTest.GenerateStatus(new JobHistory());

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
			var serviceInTest = new JobStatusUpdater(service, null);
			var choice = serviceInTest.GenerateStatus(new JobHistory());

			//ASSERT
			Assert.IsTrue(choice.Name.Equals(Data.JobStatusChoices.JobHistoryErrorJobFailed.Name));

		}

	}
}
