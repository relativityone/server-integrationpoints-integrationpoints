using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Helpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
	[TestFixture, Category("Unit")]
	public class CorrespondingJobDeleteTests : TestBase
	{
		private IJobService _jobService;
		private CorrespondingJobDelete _correspondingJobDelete;

		public override void SetUp()
		{
			_jobService = Substitute.For<IJobService>();

			_correspondingJobDelete = new CorrespondingJobDelete(_jobService);
		}

		[Test]
		public void ItShouldDeleteAllCorrespondingJobs()
		{
			int workspaceId = 401304;
			int integrationPointId = 768943;

			var taskTypes = TaskTypeHelper.GetManagerTypes().Select(taskType => taskType.ToString()).ToList();

			var jobs = new List<Job>
			{
				new Job(CreateMockJobRow(695)),
				new Job(CreateMockJobRow(315))
			};

			_jobService.GetScheduledJobs(workspaceId, integrationPointId, Arg.Is<List<string>>(x => x.SequenceEqual(taskTypes))).Returns(jobs);

			_correspondingJobDelete.DeleteCorrespondingJob(workspaceId, integrationPointId);

			_jobService.Received(1).GetScheduledJobs(workspaceId, integrationPointId, Arg.Is<List<string>>(x => x.SequenceEqual(taskTypes)));
			foreach (var job in jobs)
			{
				_jobService.Received(1).DeleteJob(job.JobId);
			}
		}

		private DataRow CreateMockJobRow(int jobId)
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add();
			dataTable.Columns.Add("JobID", typeof(long));
			dataTable.Columns.Add("RootJobId", typeof(long));
			dataTable.Columns.Add("ParentJobId", typeof(long));
			dataTable.Columns.Add("AgentTypeID", typeof(int));
			dataTable.Columns.Add("LockedByAgentID", typeof(int));
			dataTable.Columns.Add("WorkspaceID", typeof(int));
			dataTable.Columns.Add("RelatedObjectArtifactID", typeof(int));
			dataTable.Columns.Add("TaskType", typeof(string));
			dataTable.Columns.Add("NextRunTime", typeof(DateTime));
			dataTable.Columns.Add("LastRunTime", typeof(DateTime));
			dataTable.Columns.Add("JobDetails", typeof(string));
			dataTable.Columns.Add("JobFlags", typeof(int));
			dataTable.Columns.Add("SubmittedDate", typeof(DateTime));
			dataTable.Columns.Add("SubmittedBy", typeof(int));
			dataTable.Columns.Add("ScheduleRuleType", typeof(string));
			dataTable.Columns.Add("ScheduleRule", typeof(string));
			dataTable.Columns.Add("StopState", typeof(int));

			var row = dataTable.NewRow();
			row["JobID"] = jobId;
			row["RootJobId"] = 907165;
			row["ParentJobId"] = 760852;
			row["AgentTypeID"] = 256540;
			row["LockedByAgentID"] = 753340;
			row["WorkspaceID"] = 111225;
			row["RelatedObjectArtifactID"] = 436213;
			row["TaskType"] = "240604";
			row["NextRunTime"] = new DateTime();
			row["LastRunTime"] = new DateTime();
			row["JobDetails"] = "392212";
			row["JobFlags"] = 587690;
			row["SubmittedDate"] = new DateTime();
			row["SubmittedBy"] = 473355;
			row["ScheduleRuleType"] = "379502";
			row["ScheduleRule"] = "938601";
			row["StopState"] = 168546;

			return row;
		}
	}
}