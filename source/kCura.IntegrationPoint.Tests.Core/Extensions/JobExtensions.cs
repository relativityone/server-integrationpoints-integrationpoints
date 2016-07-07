using kCura.ScheduleQueue.Core;
using System;
using System.Data;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
	public static class JobExtensions
	{
		public static Job CreateJob(long workspaceArtifactId, long integrationPointArtifactId, int submittedByArtifactId, int jobId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["SubmittedBy"] = submittedByArtifactId;
			jobData["WorkspaceID"] = workspaceArtifactId;

			return new Job(jobData);
		}

		public static Job CreateJob(long workspaceArtifactId, long integrationPointArtifactId, int submittedByArtifactId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["SubmittedBy"] = submittedByArtifactId;
			jobData["WorkspaceID"] = workspaceArtifactId;

			return new Job(jobData);
		}

		public static Job CreateJob()
		{
			DataRow jobData = CreateDefaultJobData();

			return new Job(jobData);
		}

		private static DataRow CreateDefaultJobData()
		{
			DataTable table = new DataTable();

			table.Columns.Add(new DataColumn("JobID"));
			table.Columns.Add(new DataColumn("RootJobId"));
			table.Columns.Add(new DataColumn("ParentJobId"));
			table.Columns.Add(new DataColumn("AgentTypeID"));
			table.Columns.Add(new DataColumn("LockedByAgentID"));
			table.Columns.Add(new DataColumn("WorkspaceID"));
			table.Columns.Add(new DataColumn("RelatedObjectArtifactID"));
			table.Columns.Add(new DataColumn("TaskType"));
			table.Columns.Add(new DataColumn("NextRunTime"));
			table.Columns.Add(new DataColumn("LastRunTime"));
			table.Columns.Add(new DataColumn("JobDetails"));
			table.Columns.Add(new DataColumn("JobFlags"));
			table.Columns.Add(new DataColumn("SubmittedDate"));
			table.Columns.Add(new DataColumn("SubmittedBy"));
			table.Columns.Add(new DataColumn("ScheduleRuleType"));
			table.Columns.Add(new DataColumn("ScheduleRule"));

			DataRow jobData = table.NewRow();
			jobData["JobID"] = 1;
			jobData["RootJobId"] = 1;
			jobData["ParentJobId"] = 1;
			jobData["AgentTypeID"] = 1;
			jobData["LockedByAgentID"] = 1;
			jobData["WorkspaceID"] = 1;
			jobData["RelatedObjectArtifactID"] = 1;
			jobData["TaskType"] = "1";
			jobData["NextRunTime"] = default(DateTime);
			jobData["LastRunTime"] = default(DateTime);
			jobData["JobDetails"] = "1";
			jobData["JobFlags"] = 1;
			jobData["SubmittedDate"] = default(DateTime);
			jobData["SubmittedBy"] = 1;
			jobData["ScheduleRuleType"] = "1";
			jobData["ScheduleRule"] = "1";

			return jobData;
		}
	}
}