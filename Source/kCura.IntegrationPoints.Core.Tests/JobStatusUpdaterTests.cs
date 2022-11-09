using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class JobStatusUpdaterTests : TestBase
    {
        private IJobHistoryService _jobHistoryService;
        private IRelativityObjectManagerService _relativityObjectManager;
        private JobHistoryErrorQuery _service;
        private IJobService _jobService;
        private JobStatusUpdater _instance;

        [SetUp]
        public override void SetUp()
        {
            _relativityObjectManager = Substitute.For<IRelativityObjectManagerService>();
            _service = Substitute.For<JobHistoryErrorQuery>(_relativityObjectManager);
            _jobHistoryService = Substitute.For<IJobHistoryService>();
            _jobService = Substitute.For<IJobService>();

            _instance = new JobStatusUpdater(_service, _jobHistoryService, _jobService);
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
            ChoiceRef status = _instance.GenerateStatus(jobHistory);

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

        [Test]
        public void GenerateStatus_JobHistoryJobError_ReturnsValidationFailed()
        {
            //ARRANGE
            _service.GetJobErrorFailedStatus(Arg.Any<int>()).Returns(new JobHistoryError { ErrorType = ErrorTypeChoices.JobHistoryErrorJob });

            //ACT
            var choice = _instance.GenerateStatus(new JobHistory { JobStatus = JobStatusChoices.JobHistoryValidationFailed, ItemsWithErrors = 0 });

            //ASSERT
            Assert.IsTrue(choice.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed));
        }
    }
}