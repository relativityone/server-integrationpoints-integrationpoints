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
			
			//TODO make DataSet nullable
			table.Columns.Add(new DataColumn("JobID", typeof(long)));
			table.Columns.Add(new DataColumn("RootJobId", typeof(long)));
			table.Columns.Add(new DataColumn("ParentJobId", typeof(long)));
			table.Columns.Add(new DataColumn("AgentTypeID", typeof(int)));
			table.Columns.Add(new DataColumn("LockedByAgentID", typeof(int)));
			table.Columns.Add(new DataColumn("WorkspaceID", typeof(int)));
			table.Columns.Add(new DataColumn("RelatedObjectArtifactID", typeof(int)));
			table.Columns.Add(new DataColumn("TaskType", typeof(string)));
			table.Columns.Add(new DataColumn("NextRunTime", typeof(DateTime)));
			table.Columns.Add(new DataColumn("LastRunTime", typeof(DateTime)));
			table.Columns.Add(new DataColumn("JobDetails", typeof(string)));
			table.Columns.Add(new DataColumn("JobFlags", typeof(int)));
			table.Columns.Add(new DataColumn("SubmittedDate", typeof(DateTime)));
			table.Columns.Add(new DataColumn("SubmittedBy", typeof(int)));
			table.Columns.Add(new DataColumn("ScheduleRuleType", typeof(string)));
			table.Columns.Add(new DataColumn("ScheduleRule", typeof(string)));

			DataRow jobData = table.NewRow();
			jobData["JobID"] = default(long);
			jobData["RootJobId"] = default(long);
			jobData["ParentJobId"] = default(long);
			jobData["AgentTypeID"] = default(int);
			jobData["LockedByAgentID"] = default(int);
			jobData["WorkspaceID"] = default(int);
			jobData["RelatedObjectArtifactID"] = default(int);
			jobData["TaskType"] = default(string);
			jobData["NextRunTime"] = default(DateTime);
			jobData["LastRunTime"] = default(DateTime);
			jobData["JobDetails"] = default(string);
			jobData["JobFlags"] = default(int);
			jobData["SubmittedDate"] = default(DateTime);
			jobData["SubmittedBy"] = default(int);
			jobData["ScheduleRuleType"] = default(string);
			jobData["ScheduleRule"] = default(string);

			return jobData;
		}
	}
}