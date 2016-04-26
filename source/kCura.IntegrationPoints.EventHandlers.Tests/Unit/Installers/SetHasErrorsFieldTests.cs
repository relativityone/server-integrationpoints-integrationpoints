using System;
using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Installers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Unit.Installers
{
	[TestFixture]
	public class SetHasErrorsFieldTests
	{
		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;

		private SetHasErrorsField _instance;

		[SetUp]
		public void SetUp()
		{
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();

			_instance = new SetHasErrorsField(_integrationPointService, _jobHistoryService);
		}

		[Test]
		public void GetIntegrationPoints_Test()
		{
			// Arrange
			var integrationPointOne = new Data.IntegrationPoint { ArtifactId = 1 };
			var integrationPointTwo = new Data.IntegrationPoint { ArtifactId = 2 };
			IList<Data.IntegrationPoint> expectedIntegrationPoints = new[] { integrationPointTwo, integrationPointOne };

			_integrationPointService.GetAllIntegrationPoints().Returns(expectedIntegrationPoints);

			// Act
			IList<Data.IntegrationPoint> actualIntegrationPoints = _instance.GetIntegrationPoints();

			// Assert
			Assert.IsNotEmpty(actualIntegrationPoints);
			Assert.AreEqual(expectedIntegrationPoints, actualIntegrationPoints);

			_integrationPointService.Received(1).GetAllIntegrationPoints();
		}

		[Test]
		[TestCase(null)]
		[TestCase(new int[0])]
		public void UpdateIntegrationPointHasErrorsField_NoJobHistories_HasErrorsFalse_Tests(int[] jobHistories)
		{
			// Arrange
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint(jobHistories);
			integrationPoint.JobHistory = jobHistories;
			_integrationPointService.SaveIntegration(Arg.Is<IntegrationModel>(x => x.ArtifactID == integrationPoint.ArtifactId)).Returns(1);

			// Act
			_instance.UpdateIntegrationPointHasErrorsField(integrationPoint);

			// Assert
			_integrationPointService.Received(1)
				.SaveIntegration(
					Arg.Is<IntegrationModel>(x => x.HasErrors == false && x.ArtifactID == integrationPoint.ArtifactId));
			_jobHistoryService.Received(0).GetJobHistory(null);
		}

		[Test]
		public void UpdateIntegrationPointHasErrorsField_HasJobHistories_PendingOrProcessingJobs_HasErrorsFalse_Test()
		{
			// Arrange
			var pendingJob = new JobHistory { ArtifactId = 2, Status = JobStatusChoices.JobHistoryPending, EndTimeUTC = DateTime.MinValue };
			var processingJob = new JobHistory { ArtifactId = 3, Status = JobStatusChoices.JobHistoryProcessing, EndTimeUTC = DateTime.MinValue };
			var jobHistories = new[] { pendingJob, processingJob };

			UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, false);
		}

		[Test]
		public void UpdateIntegrationPointHasErrorsField_HasJobHistories_CompletedJob_HasErrorsFalse_Test()
		{
			// Arrange
			var completedJob = new JobHistory { ArtifactId = 2, Status = JobStatusChoices.JobHistoryCompleted, EndTimeUTC = DateTime.MinValue };
			var jobHistories = new[] { completedJob };

			UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, false);
		}

		[Test]
		public void UpdateIntegrationPointHasErrorsField_HasJobHistories_ErroredJob_HasErrorsTrue_Test()
		{
			// Arrange
			var erroredJob = new JobHistory { ArtifactId = 2, Status = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MinValue };
			var jobHistories = new[] { erroredJob };

			UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
		}

		[Test]
		public void UpdateIntegrationPointHasErrorsField_HasJobHistories_ErroredThenCompletedJob_HasErrorsFalse_Test()
		{
			// Arrange
			var erroredJob = new JobHistory { ArtifactId = 2, Status = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MinValue };
			var completedJob = new JobHistory { ArtifactId = 3, Status = JobStatusChoices.JobHistoryCompleted, EndTimeUTC = DateTime.MaxValue };
			var jobHistories = new[] { erroredJob, completedJob };

			UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, false);
		}

		[Test]
		public void UpdateIntegrationPointHasErrorsField_HasJobHistories_CompletedThenErorredJob_HasErrorsTrue_Test()
		{
			// Arrange
			var erroredJob = new JobHistory { ArtifactId = 2, Status = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MaxValue };
			var completedJob = new JobHistory { ArtifactId = 3, Status = JobStatusChoices.JobHistoryCompleted, EndTimeUTC = DateTime.MinValue };
			var jobHistories = new[] { erroredJob, completedJob };

			UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
		}

		[Test]
		public void UpdateIntegrationPointHasErrorsField_HasJobHistories_ErroredThenProcessingJob_HasErrorsTrue_Test()
		{
			// Arrange
			var erroredJob = new JobHistory { ArtifactId = 2, Status = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MinValue };
			var completedJob = new JobHistory { ArtifactId = 3, Status = JobStatusChoices.JobHistoryProcessing, EndTimeUTC = DateTime.MaxValue };
			var jobHistories = new[] { erroredJob, completedJob };

			UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
		}

		[Test]
		public void UpdateIntegrationPointHasErrorsField_HasJobHistories_CompletedWithErrorThenPendingJob_HasErrorsTrue_Test()
		{
			// Arrange
			var erroredJob = new JobHistory { ArtifactId = 2, Status = JobStatusChoices.JobHistoryCompletedWithErrors, EndTimeUTC = DateTime.MinValue };
			var completedJob = new JobHistory { ArtifactId = 3, Status = JobStatusChoices.JobHistoryPending, EndTimeUTC = DateTime.MaxValue };
			var jobHistories = new[] { erroredJob, completedJob };

			UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
		}

		[Test]
		public void ExecuteInstanced_UpdateSuccessful_Test()
		{
			// Arrange
			var completedJob = new JobHistory { ArtifactId = 3, Status = JobStatusChoices.JobHistoryPending, EndTimeUTC = DateTime.MaxValue };
			var jobHistories = new[] { completedJob };
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint(jobHistories.Select(x => x.ArtifactId).ToArray());
			IList<Data.IntegrationPoint> integrationPoints = new[] { integrationPoint };

			_integrationPointService.GetAllIntegrationPoints().Returns(integrationPoints);
			_jobHistoryService.GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory))).Returns(jobHistories);
			_integrationPointService.SaveIntegration(Arg.Is<IntegrationModel>(x => x.ArtifactID == integrationPoint.ArtifactId)).Returns(1);

			// Act
			Response response = _instance.ExecuteInstanced();

			// Assert
			Assert.IsTrue(response.Success);
			Assert.AreEqual("Updated successfully.", response.Message);

			_integrationPointService.Received(1).GetAllIntegrationPoints();
			_jobHistoryService.Received(1).GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory)));
			_integrationPointService.Received(1)
				.SaveIntegration(
					Arg.Is<IntegrationModel>(x => x.HasErrors == false && x.ArtifactID == integrationPoint.ArtifactId));
		}

		[Test]
		public void ExecuteInstanced_ThrowsQueryError_Test()
		{
			// Arrange
			Exception e = new Exception("Query failed");
			_integrationPointService.GetAllIntegrationPoints().Throws(e);

			// Act
			Response response = _instance.ExecuteInstanced();

			// Assert
			Assert.IsFalse(response.Success);
			Assert.AreEqual("Update failed. Exception message: Query failed.", response.Message);

			_integrationPointService.Received(1).GetAllIntegrationPoints();
			_jobHistoryService.Received(0).GetJobHistory(null);
			_integrationPointService.Received(0).SaveIntegration(null);
		}

		private void UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(JobHistory[] jobHistories, bool hasErrorsExpectation)
		{
			// Arrange
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint(jobHistories.Select(x => x.ArtifactId).ToArray());

			_jobHistoryService.GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory))).Returns(jobHistories);
			_integrationPointService.SaveIntegration(Arg.Is<IntegrationModel>(x => x.ArtifactID == integrationPoint.ArtifactId)).Returns(1);

			// Act
			_instance.UpdateIntegrationPointHasErrorsField(integrationPoint);

			// Assert
			_jobHistoryService.Received(1).GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory)));
			_integrationPointService.Received(1)
				.SaveIntegration(
					Arg.Is<IntegrationModel>(x => x.HasErrors == hasErrorsExpectation && x.ArtifactID == integrationPoint.ArtifactId));
		}

		private bool CompareLists(int[] actualValues, int[] expectedValues)
		{
			if (actualValues == null && expectedValues == null)
			{
				return true;
			}
			if (actualValues == null || expectedValues == null)
			{
				return false;
			}
			if (actualValues.Length != expectedValues.Length)
			{
				return false;
			}

			return actualValues.All(expectedValues.Contains);
		}

		private Data.IntegrationPoint CreateIntegrationPoint(int[] jobHistories)
		{
			var integrationPoint = new Data.IntegrationPoint
			{
				ArtifactId = 1,
				Name = "Integration Point One",
				OverwriteFields = null,
				SourceProvider = 312,
				DestinationConfiguration = "Destination Configuration",
				SourceConfiguration = "Source Configuration",
				DestinationProvider = null,
				EnableScheduler = null,
				ScheduleRule = null,
				EmailNotificationRecipients = null,
				LogErrors = null,
				HasErrors = null,
				LastRuntimeUTC = DateTime.UtcNow,
				JobHistory = jobHistories
			};
			return integrationPoint;
		}
	}
}