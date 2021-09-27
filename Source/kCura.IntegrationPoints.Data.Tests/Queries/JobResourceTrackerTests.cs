using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Queries;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Queries;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Queries
{
	[TestFixture, Category("Unit")]
	public class JobResourceTrackerTests
	{
		private Mock<IQueueQueryManager> _queueQueryManagerFake;
		private Mock<IJobTrackerQueryManager> _jobTrackerQueryManagerFake;

		private IJobResourceTracker _sut;

		[SetUp]
		public void SetUp()
		{
			_queueQueryManagerFake = new Mock<IQueueQueryManager>();
			_jobTrackerQueryManagerFake = new Mock<IJobTrackerQueryManager>();

			_sut = new JobResourceTracker(
				_jobTrackerQueryManagerFake.Object,
				_queueQueryManagerFake.Object);
		}

		[Test]
		public void GetBatchesStatuses_ShouldReturnBatchesResultCount_WhenJobsInQueueAndJobsInTrackingEntryMatches()
		{
			// Arrange
			const long rootJobId = 10;

			const long jobId1 = 11;
			const int lockedByAgentId1 = 101;
			const long jobId2 = 12;
			const int lockedByAgentId2 = 102;

			List<Job> jobs = new List<Job>
			{
				GetJob(jobId1, rootJobId, lockedByAgentId1, StopState.None),
				GetJob(jobId2, rootJobId, lockedByAgentId2, StopState.None)
			};

			List<long> trackingEntryJobIds = new List<long> { jobId1, jobId2 };

			// Act
			BatchStatusQueryResult result = RunGetBatchStatusesTestCase(jobs, trackingEntryJobIds, rootJobId);

			// Assert
			result.ProcessingCount.Should().Be(jobs.Count);
			result.SuspendedCount.Should().Be(0);
		}

		[Test]
		public void GetBatchesStatuses_ShouldReturnBatchesResultCount_WhenJobsInQueueHasMoreRecordsThanJobsInTrackingEntryMatches()
		{
			// Arrange
			const long rootJobId = 10;
			const long notApplicableRootJobId = 20;

			const long jobId1 = 11;
			const int lockedByAgentId1 = 100;
			const long jobId2 = 12;
			const int lockedByAgentId2 = 200;
			const long notApplicableJobId1 = 21;

			List<Job> jobs = new List<Job>
			{
				GetJob(jobId1, rootJobId, lockedByAgentId1, StopState.None),
				GetJob(jobId2, rootJobId, lockedByAgentId2, StopState.None),
				GetJob(notApplicableJobId1, notApplicableRootJobId, null, StopState.None),
			};

			List<long> trackingEntryJobIds = new List<long> { jobId1, jobId2 };

			// Act
			BatchStatusQueryResult result = RunGetBatchStatusesTestCase(jobs, trackingEntryJobIds, rootJobId);

			// Assert
			int expectedProcessingCount = jobs.Count(x => x.RootJobId == rootJobId);

			result.ProcessingCount.Should().Be(expectedProcessingCount);
			result.SuspendedCount.Should().Be(0);
		}

		[Test]
		public void GetBatchesStatuses_ShouldReturnBatchesResultCount_WhenSomeJobsInQueueAreDrainStopped()
		{
			// Arrange
			const long rootJobId = 10;

			const long jobId1 = 11;
			const long jobId2 = 12;
			const int lockedByAgentId2 = 102;

			List<Job> jobs = new List<Job>
			{
				GetJob(jobId1, rootJobId, null, StopState.DrainStopped),
				GetJob(jobId2, rootJobId, lockedByAgentId2, StopState.None)
			};

			List<long> trackingEntryJobIds = new List<long> { jobId1, jobId2 };

			// Act
			BatchStatusQueryResult result = RunGetBatchStatusesTestCase(jobs, trackingEntryJobIds, rootJobId);

			// Assert
			result.ProcessingCount.Should().Be(jobs.Count(x => x.LockedByAgentID != null));
			result.SuspendedCount.Should().Be(jobs.Count(x => x.LockedByAgentID == null && x.StopState == StopState.DrainStopped));
		}

		[Test]
		public void GetBatchesStatuses_ShouldReturnBatchesResultCount_WhenEntryJobIdsIsEmpty()
		{
			// Arrange
			const long rootJobId = 10;

			const long jobId1 = 11;
			const long jobId2 = 12;

			List<Job> jobs = new List<Job>
			{
				GetJob(jobId1, rootJobId, null, StopState.None),
				GetJob(jobId2, rootJobId, null, StopState.None)
			};

			List<long> trackingEntryJobIds = new List<long>();

			// Act
			BatchStatusQueryResult result = RunGetBatchStatusesTestCase(jobs, trackingEntryJobIds, rootJobId);

			// Assert
			result.ProcessingCount.Should().Be(0);
			result.SuspendedCount.Should().Be(0);
		}

		private BatchStatusQueryResult RunGetBatchStatusesTestCase(
			List<Job> jobs, List<long> entryJobIds, long rootJobId)
		{
			SetupQueueQueryManager(jobs);

			SetupJobTrackerQueryManager(rootJobId, entryJobIds);

			// Act
			return _sut.GetBatchesStatuses(It.IsAny<string>(), rootJobId, It.IsAny<int>());
		}

		private void SetupJobTrackerQueryManager(long rootJoBId, List<long> jobIds)
		{
			ValueReturnQuery<DataTable> queryResult =
				new ValueReturnQuery<DataTable>(CreateEntryTrackingJobIdsDataTable(jobIds));

			_jobTrackerQueryManagerFake.Setup(x => x.GetJobIdsFromTrackingEntry(
					It.IsAny<string>(), It.IsAny<int>(), rootJoBId))
				.Returns(queryResult);
		}

		private void SetupQueueQueryManager(List<Job> jobs)
		{
			ValueReturnQuery<DataTable> queryResult =
				new ValueReturnQuery<DataTable>(CreateJobsDataTable(jobs));

			_queueQueryManagerFake.Setup(x => x.GetAllJobs())
				.Returns(queryResult);
		}

		private static Job GetJob(long jobId, long rootJobId, int? lockedByAgentId, StopState stopState)
		{
			return new JobBuilder()
				.WithJobId(jobId)
				.WithRootJobId(rootJobId)
				.WithLockedByAgentId(lockedByAgentId)
				.WithStopState(stopState)
				.Build();
		}

		private static DataTable CreateJobsDataTable(List<Job> jobs)
		{
			DataTable dataTable = JobHelper.CreateEmptyJobDataTable();

			DataRow GetJobDataRow(Job job)
			{
				return JobHelper.CreateJobDataRow(job.JobId, job.RootJobId, It.IsAny<int>(), It.IsAny<int>(), job.LockedByAgentID,
					It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TaskType>(), It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<string>(),
					It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), job.StopState, dataTable);
			}

			foreach (var job in jobs)
			{
				dataTable.Rows.Add(GetJobDataRow(job));
			}

			return dataTable;
		}

		private static DataTable CreateEntryTrackingJobIdsDataTable(List<long> jobIds)
		{
			DataTable dt = new DataTable();
			dt.Columns.AddRange(new DataColumn[]
			{
				new DataColumn() {ColumnName = "JobID", DataType = typeof(long)}
			});

			foreach (var jobId in jobIds)
			{
				DataRow row = dt.NewRow();
				row["JobID"] = jobId;

				dt.Rows.Add(row);
			}

			return dt;
		}
	}
}
