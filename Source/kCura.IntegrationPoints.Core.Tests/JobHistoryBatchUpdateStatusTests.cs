using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture, Category("Unit")]
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
            Job job = new JobBuilder().WithJobId(_jobID).WithWorkspaceId(_workspaceID).Build();
            _jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(StopState.Stopping));

            // ACT
            _instance.OnJobStart(job);

            // ASSERT
            _jobHistoryService.DidNotReceive().UpdateRdoWithoutDocuments(Arg.Any<JobHistory>());
        }

        [TestCase(StopState.Unstoppable)]
        [TestCase(StopState.None)]
        public void OnJobStart_DoNotUpdateOnNonStoppingJob(StopState state)
        {
            // ARRANGE
            Job job = new JobBuilder().WithJobId(_jobID).WithWorkspaceId(_workspaceID).Build();
            Guid correlationID = Guid.NewGuid();
            job.CorrelationID = correlationID.ToString();
            TaskParameters parameters = new TaskParameters() { BatchInstance = correlationID };
            _jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(state));
            _serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
            _jobHistoryService.GetRdoWithoutDocuments(parameters.BatchInstance).Returns(new JobHistory
            {
                ArtifactId = _jobHistoryArtifactId,
                JobStatus = JobStatusChoices.JobHistoryPending
            });

            // ACT
            _instance.OnJobStart(job);

            // ASSERT
            _jobHistoryService
                .Received(1)
                .UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(obj => obj.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryProcessing)));
        }

        [Test]
        public void OnJobComplete_UpdateTheJobStatus()
        {
            // ARRANGE
            Job job = new JobBuilder().WithJobId(_jobID).WithWorkspaceId(_workspaceID).Build();
            job.CorrelationID = Guid.NewGuid().ToString();

            ChoiceRef expectedStatus = JobStatusChoices.JobHistoryCompleted;
            var expectedEndTimeUtc = new DateTime(2010, 10, 10, 10, 10, 10);

            ArrangeJobComplete(expectedStatus, expectedEndTimeUtc, job);

            // ACT
            _instance.OnJobComplete(job);

            // ASSERT
            _jobHistoryService
                .Received(1)
                .UpdateRdoWithoutDocuments(
                    Arg.Is<JobHistory>(jh => jh.JobStatus.EqualsToChoice(expectedStatus)
                                             && jh.EndTimeUTC == expectedEndTimeUtc));
            _logger
                .DidNotReceive()
                .LogError(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void OnJobComplete_LogErrorWhenJobServiceFails()
        {
            // ARRANGE
            Job job = new JobBuilder().WithJobId(_jobID).WithWorkspaceId(_workspaceID).Build();
            job.CorrelationID = Guid.NewGuid().ToString();

            ChoiceRef expectedStatus = JobStatusChoices.JobHistoryCompleted;
            var expectedEndTimeUtc = new DateTime(2010, 10, 10, 10, 10, 10);

            ArrangeJobComplete(expectedStatus, expectedEndTimeUtc, job);
            InvalidOperationException exception = new InvalidOperationException(_jobHistoryServiceErrorMessage);
            _jobHistoryService.When(x => x.UpdateRdoWithoutDocuments(Arg.Any<JobHistory>())).Do(x =>
            {
                throw exception;
            });

            // ACT
            Assert.Throws<InvalidOperationException>(() => _instance.OnJobComplete(job));

            // ASSERT
            _jobHistoryService
                .Received(1)
                .UpdateRdoWithoutDocuments(
                    Arg.Is<JobHistory>(jh => jh.JobStatus.EqualsToChoice(expectedStatus)
                                             && jh.EndTimeUTC == expectedEndTimeUtc));
            _logger
                .Received(1)
                .LogError(exception, Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void OnJobComplete_LogErrorWhenGetHistoryFails()
        {
            // ARRANGE
            Guid correlationID = Guid.NewGuid();
            Job job = new JobBuilder().WithJobId(_jobID).WithWorkspaceId(_workspaceID).Build();
            job.CorrelationID = correlationID.ToString();
            TaskParameters parameters = new TaskParameters() { BatchInstance = correlationID };
            _serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>())
                .Returns((JobHistory)null);

            // ACT
            Assert.Throws<NullReferenceException>(() => _instance.OnJobComplete(job));

            // ASSERT
            _jobHistoryService.DidNotReceive().UpdateRdoWithoutDocuments(Arg.Any<JobHistory>());
            _logger.Received(1).LogError(Arg.Any<NullReferenceException>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        private void ArrangeJobComplete(ChoiceRef expectedStatus, DateTime expectedEndTimeUtc, Job job)
        {
            JobHistory history = new JobHistory
            {
                ArtifactId = _jobHistoryArtifactId,
                JobStatus = JobStatusChoices.JobHistoryProcessing
            };

            TaskParameters parameters = new TaskParameters() { BatchInstance = Guid.Parse(job.CorrelationID) };
            _jobService.GetJob(job.JobId).Returns(job.CopyJobWithStopState(StopState.None));
            _serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(parameters);
            _jobHistoryService.GetRdoWithoutDocuments(parameters.BatchInstance).Returns(history);
            _updater.GenerateStatus(history, job.JobId).Returns(expectedStatus);
            _dateTimeHelper.Now().Returns(expectedEndTimeUtc);
        }
    }
}
