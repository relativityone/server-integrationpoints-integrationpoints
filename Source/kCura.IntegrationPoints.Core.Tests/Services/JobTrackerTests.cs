using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture]
    public class JobTrackerTests : TestBase
    {
        private Job _goldFlowJob;
        private string _goldFlowExpectedTempTableName;
        private string _goldFlowBatchId;
        private IJobResourceTracker _jobResourceTracker;

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _goldFlowBatchId = "batchID";
            _goldFlowJob = GetJob(11, 22, 33);
            _goldFlowExpectedTempTableName = "RIP_JobTracker_22_33_batchID";
            _jobResourceTracker = Substitute.For<IJobResourceTracker>();
            _jobResourceTracker.RemoveEntryAndCheckStatus(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>()).Returns(
                x => string.Equals((string) x[0], _goldFlowExpectedTempTableName) && (long) x[1] == _goldFlowJob.JobId &&
                     (int) x[2] == _goldFlowJob.WorkspaceID
                    ? 0
                    : 1);
        }

        [SetUp]
        public override void SetUp()
        {
        }

        [Test]
        public void GenerateJobTrackerTempTableName_GoldFlow()
        {
            string tempTableName = JobTracker.GenerateJobTrackerTempTableName(_goldFlowJob, _goldFlowBatchId);

            Assert.AreEqual(tempTableName, _goldFlowExpectedTempTableName);
        }

        [Test]
        public void GenerateJobTrackerTempTableName_JobIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => JobTracker.GenerateJobTrackerTempTableName(null, _goldFlowBatchId));
        }

        [Test]
        public void CreateTrackingEntry_GoldFlow()
        {
            var jobTracker = new JobTracker(_jobResourceTracker);

            jobTracker.CreateTrackingEntry(_goldFlowJob, _goldFlowBatchId);

            _jobResourceTracker.ReceivedWithAnyArgs().CreateTrackingEntry(string.Empty, 0, 0);
        }

        [Test]
        public void CreateTrackingEntry_JobIsNull_ThrowsArgumentNullException()
        {
            var jobTracker = new JobTracker(_jobResourceTracker);

            Assert.Throws<ArgumentNullException>(() => jobTracker.CreateTrackingEntry(null, _goldFlowBatchId));
        }

        [Test]
        public void CheckEntries_ExistingJob_True()
        {
            var jobTracker = new JobTracker(_jobResourceTracker);

            bool checkResult = jobTracker.CheckEntries(_goldFlowJob, _goldFlowBatchId);

            Assert.IsTrue(checkResult);
        }

        [Test]
        public void CheckEntries_NonExistingJob_False()
        {
            Job nonExistingJob = GetJob(1, 1, 1);
            var jobTracker = new JobTracker(_jobResourceTracker);

            bool checkResult = jobTracker.CheckEntries(nonExistingJob, _goldFlowBatchId);

            Assert.IsFalse(checkResult);
        }

        [Test]
        public void CheckEntries_ExistingJobWrongBatchId_False()
        {
            var jobTracker = new JobTracker(_jobResourceTracker);

            bool checkResult = jobTracker.CheckEntries(_goldFlowJob, "wrong batch id");

            Assert.IsFalse(checkResult);
        }

        private Job GetJob(int jobId, int workspaceID, long? rootJobId = null)
        {
            return JobHelper.GetJob(jobId, rootJobId, null, 0, 0, workspaceID, 0, TaskType.None, DateTime.MinValue,
                null, null, 0, DateTime.MinValue, 0, null, null);
        }
    }
}
