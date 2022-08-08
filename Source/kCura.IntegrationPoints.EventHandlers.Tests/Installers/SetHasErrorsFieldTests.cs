using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Installers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class SetHasErrorsFieldTests : TestBase
    {
        private IIntegrationPointRepository _integrationPointRepository;
        private IJobHistoryService _jobHistoryService;

        private SetHasErrorsField _instance;

        [SetUp]
        public override void SetUp()
        {
            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
            _jobHistoryService = Substitute.For<IJobHistoryService>();

            _instance = new SetHasErrorsField(_integrationPointRepository, _jobHistoryService);
            _instance.Helper = Substitute.For<IEHHelper>();
        }

        [Test]
        [TestCase(null)]
        [TestCase(new int[0])]
        public void UpdateIntegrationPointHasErrorsField_NoJobHistories_HasErrorsFalse_Tests(int[] jobHistories)
        {
            // Arrange
            Data.IntegrationPoint integrationPoint = CreateIntegrationPoint(jobHistories);
            integrationPoint.JobHistory = jobHistories;

            // Act
            _instance.UpdateIntegrationPointHasErrorsField(integrationPoint);

            // Assert
            _integrationPointRepository.Received(1)
                .Update(Arg.Is<Data.IntegrationPoint>(x => x.HasErrors.Value == false && x.ArtifactId == integrationPoint.ArtifactId));
            _jobHistoryService.Received(0).GetJobHistory(null);
        }

        [Test]
        public void UpdateIntegrationPointHasErrorsField_HasJobHistories_PendingOrProcessingJobs_HasErrorsFalse_Test()
        {
            // Arrange
            var pendingJob = new JobHistory { ArtifactId = 2, JobStatus = JobStatusChoices.JobHistoryPending, EndTimeUTC = null };
            var processingJob = new JobHistory { ArtifactId = 3, JobStatus = JobStatusChoices.JobHistoryProcessing, EndTimeUTC = null };
            var jobHistories = new[] { pendingJob, processingJob };

            UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, false);
        }

        [Test]
        public void UpdateIntegrationPointHasErrorsField_HasJobHistories_CompletedJob_HasErrorsFalse_Test()
        {
            // Arrange
            var completedJob = new JobHistory { ArtifactId = 2, JobStatus = JobStatusChoices.JobHistoryCompleted, EndTimeUTC = DateTime.MinValue };
            var jobHistories = new[] { completedJob };

            UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, false);
        }

        [Test]
        public void UpdateIntegrationPointHasErrorsField_HasJobHistories_ErroredJob_HasErrorsTrue_Test()
        {
            // Arrange
            var erroredJob = new JobHistory { ArtifactId = 2, JobStatus = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MinValue };
            var jobHistories = new[] { erroredJob };

            UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
        }

        [Test]
        public void UpdateIntegrationPointHasErrorsField_HasJobHistories_ErroredThenCompletedJob_HasErrorsFalse_Test()
        {
            // Arrange
            var erroredJob = new JobHistory { ArtifactId = 2, JobStatus = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MinValue };
            var completedJob = new JobHistory { ArtifactId = 3, JobStatus = JobStatusChoices.JobHistoryCompleted, EndTimeUTC = DateTime.MaxValue };
            var jobHistories = new[] { erroredJob, completedJob };

            UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, false);
        }

        [Test]
        public void UpdateIntegrationPointHasErrorsField_HasJobHistories_CompletedThenErorredJob_HasErrorsTrue_Test()
        {
            // Arrange
            var erroredJob = new JobHistory { ArtifactId = 2, JobStatus = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MaxValue };
            var completedJob = new JobHistory { ArtifactId = 3, JobStatus = JobStatusChoices.JobHistoryCompleted, EndTimeUTC = DateTime.MinValue };
            var jobHistories = new[] { erroredJob, completedJob };

            UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
        }

        [Test]
        public void UpdateIntegrationPointHasErrorsField_HasJobHistories_ErroredThenProcessingJob_HasErrorsTrue_Test()
        {
            // Arrange
            var erroredJob = new JobHistory { ArtifactId = 2, JobStatus = JobStatusChoices.JobHistoryErrorJobFailed, EndTimeUTC = DateTime.MinValue };
            var completedJob = new JobHistory { ArtifactId = 3, JobStatus = JobStatusChoices.JobHistoryProcessing, EndTimeUTC = null };
            var jobHistories = new[] { erroredJob, completedJob };

            UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
        }

        [Test]
        public void UpdateIntegrationPointHasErrorsField_HasJobHistories_CompletedWithErrorThenPendingJob_HasErrorsTrue_Test()
        {
            // Arrange
            var erroredJob = new JobHistory { ArtifactId = 2, JobStatus = JobStatusChoices.JobHistoryCompletedWithErrors, EndTimeUTC = DateTime.MinValue };
            var completedJob = new JobHistory { ArtifactId = 3, JobStatus = JobStatusChoices.JobHistoryPending, EndTimeUTC = null };
            var jobHistories = new[] { erroredJob, completedJob };

            UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(jobHistories, true);
        }

        [Test]
        public void ExecuteInstanced_UpdateSuccessful_Test()
        {
            // Arrange
            var pendingJob = new JobHistory { ArtifactId = 3, JobStatus = JobStatusChoices.JobHistoryPending, EndTimeUTC = null };
            var jobHistories = new[] { pendingJob };
            Data.IntegrationPoint integrationPoint = CreateIntegrationPoint(jobHistories.Select(x => x.ArtifactId).ToArray());
            IList<Data.IntegrationPoint> integrationPoints = new[] { integrationPoint };

            _integrationPointRepository.GetIntegrationPointsWithAllFields().Returns(integrationPoints);
            _jobHistoryService.GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory))).Returns(jobHistories);

            // Act
            _instance.ExecuteInstanced();

            // Assert
            _integrationPointRepository.Received(1).GetIntegrationPointsWithAllFields();
            _jobHistoryService.Received(1).GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory)));
            _integrationPointRepository.Received(1)
                .Update(Arg.Is<Data.IntegrationPoint>(x => x.HasErrors.Value == false && x.ArtifactId == integrationPoint.ArtifactId));
        }

        [Test]
        public void ExecuteInstanced_ThrowsQueryError_Test()
        {
            // Arrange
            Exception e = new Exception("Query failed");
            _integrationPointRepository.GetIntegrationPointsWithAllFields().Throws(e);

            // Act
            Assert.Throws<Exception>(() => _instance.ExecuteInstanced());

            // Assert

            _integrationPointRepository.Received(1).GetIntegrationPointsWithAllFields();
            _jobHistoryService.Received(0).GetJobHistory(null);
            _integrationPointRepository.Received(0).Update(null);
        }

        private void UpdateIntegrationPointHasErrorsField_HasJobHistories_TestsHelper(JobHistory[] jobHistories, bool hasErrorsExpectation)
        {
            // Arrange
            Data.IntegrationPoint integrationPoint = CreateIntegrationPoint(jobHistories.Select(x => x.ArtifactId).ToArray());

            _jobHistoryService.GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory))).Returns(jobHistories);

            // Act
            _instance.UpdateIntegrationPointHasErrorsField(integrationPoint);

            // Assert
            _jobHistoryService.Received(1).GetJobHistory(Arg.Is<int[]>(x => CompareLists(x, integrationPoint.JobHistory)));
            _integrationPointRepository.Received(1)
                .Update(Arg.Is<Data.IntegrationPoint>(x => x.HasErrors.Value == hasErrorsExpectation && x.ArtifactId == integrationPoint.ArtifactId));
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