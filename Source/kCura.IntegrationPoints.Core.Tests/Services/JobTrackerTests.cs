using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class JobTrackerTests : TestBase
    {
        private Job _goldFlowJob;
        private string _goldFlowExpectedTempTableName;
        private string _goldFlowBatchId;
        private IJobResourceTracker _jobResourceTracker;
        private Mock<IJobResourceTracker> _jobResourceTrackerMock;
        private Mock<IAPILog> _loggerMock;
        private IAPILog _loggerMockObject;

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _goldFlowBatchId = "batchID";
            _goldFlowJob = GetJob(11, 22, 33);
            _goldFlowExpectedTempTableName = "RIP_JobTracker_22_33_batchID";
            _jobResourceTrackerMock = new Mock<IJobResourceTracker>();
            _loggerMock = new Mock<IAPILog>();
            _loggerMockObject = _loggerMock.Object;
            _jobResourceTracker = _jobResourceTrackerMock.Object;
            _jobResourceTrackerMock
                .Setup(x => x.RemoveEntryAndCheckStatus(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns<string, long, int, bool>(
                    (tableName, jobId, workspaceId, batchIsFinished) =>
            {
                if (string.Equals(tableName, _goldFlowExpectedTempTableName) && jobId == _goldFlowJob.JobId && workspaceId == _goldFlowJob.WorkspaceID)
                {
                    return batchIsFinished ? 0 : 1;
                }

                return 1;
            });
        }

        private JobTracker PrepareJobTracker()
        {
            return new JobTracker(_jobResourceTracker, _loggerMockObject);
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
            JobTracker jobTracker = PrepareJobTracker();

            jobTracker.CreateTrackingEntry(_goldFlowJob, _goldFlowBatchId);

            _jobResourceTrackerMock.Verify(x => x.CreateTrackingEntry(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>()));
        }

        [Test]
        public void CreateTrackingEntry_JobIsNull_ThrowsArgumentNullException()
        {
            JobTracker jobTracker = PrepareJobTracker();

            Assert.Throws<ArgumentNullException>(() => jobTracker.CreateTrackingEntry(null, _goldFlowBatchId));
        }

        [Test]
        public void CheckEntries_ExistingJob_True()
        {
            JobTracker jobTracker = PrepareJobTracker();

            bool checkResult = jobTracker.CheckEntries(_goldFlowJob, _goldFlowBatchId, true);

            Assert.IsTrue(checkResult);
        }

        [Test]
        public void CheckEntries_ExistingJob_False_BatchNotFinished()
        {
            JobTracker jobTracker = PrepareJobTracker();

            bool checkResult = jobTracker.CheckEntries(_goldFlowJob, _goldFlowBatchId, false);

            Assert.IsFalse(checkResult);
        }

        [Test]
        public void CheckEntries_NonExistingJob_False()
        {
            Job nonExistingJob = GetJob(1, 1, 1);
            JobTracker jobTracker = PrepareJobTracker();

            bool checkResult = jobTracker.CheckEntries(nonExistingJob, _goldFlowBatchId, true);

            Assert.IsFalse(checkResult);
        }

        [Test]
        public void CheckEntries_ExistingJobWrongBatchId_False()
        {
            JobTracker jobTracker = PrepareJobTracker();

            bool checkResult = jobTracker.CheckEntries(_goldFlowJob, "wrong batch id", true);

            Assert.IsFalse(checkResult);
        }

        private Job GetJob(int jobId, int workspaceID, long? rootJobId = null)
        {
            return JobHelper.GetJob(jobId, rootJobId, null, 0, 0, workspaceID,
                0, string.Empty, TaskType.None, DateTime.MinValue,
                null, null, 0, DateTime.MinValue, 0, null, null);
        }
    }
}
