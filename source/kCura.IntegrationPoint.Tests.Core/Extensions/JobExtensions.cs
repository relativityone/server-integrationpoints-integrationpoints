using kCura.ScheduleQueue.Core;
using System;
using System.Data;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
	using System.Collections.Generic;
	using System.Data.SqlClient;

	using kCura.IntegrationPoints.Core.Contracts.Agent;
	using kCura.ScheduleQueue.Core.Data;
	using kCura.ScheduleQueue.Core.Properties;

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

		public static Job CreateJob(long workspaceArtifactId, long integrationPointArtifactId, int submittedByArtifactId, int jobId, int rootJobId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["SubmittedBy"] = submittedByArtifactId;
			jobData["WorkspaceID"] = workspaceArtifactId;
			jobData["RootJobId"] = rootJobId;

			return new Job(jobData);
		}

		public static Job CreateJob(long workspaceArtifactId, long integrationPointArtifactId, string jobDetails)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["JobDetails"] = jobDetails;
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
			jobData["TaskType"] = TaskType.SyncManager.ToString();
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

		static string insertJob = @"INSERT INTO [eddsdbo].[{0}] 
		(
			[RootJobID]
			,[ParentJobID]
			,[AgentTypeID]
			,[LockedByAgentID]
			,[WorkspaceID]
			,[RelatedObjectArtifactID]
			,[TaskType]
			,[NextRunTime]
			,[LastRunTime]
			,[ScheduleRuleType]
			,[ScheduleRule]
			,[JobDetails]
			,[JobFlags]
			,[SubmittedDate]
			,[SubmittedBy]
		)
		OUTPUT
			Inserted.[JobID]
		VALUES
		(
			@RootJobID
			, @ParentJobID
			, @AgentTypeID
			, @LockedByAgentID
			, @WorkspaceID
			, @RelatedObjectArtifactID
			, @TaskType
			, @NextRunTime
			, NULL
			, @ScheduleRuleType
			, @ScheduleRule
			, @JobDetails
			, @JobFlags
			, GETUTCDATE()
			,@SubmittedBy
		)";

		public static int Execute(IQueueDBContext qDBContext,
												int workspaceID,
												int relatedObjectArtifactID,
												string taskType,
												DateTime nextRunTime,
												int AgentTypeID,
												string scheduleRuleType,
												string serializedScheduleRule,
												string jobDetails,
												int jobFlags,
												int SubmittedBy,
												int locked,
												long? rootJobID,
												long? parentJobID = null
												)
		{
			string sql = string.Format(insertJob, qDBContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@WorkspaceID", workspaceID));
			sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactID));
			sqlParams.Add(new SqlParameter("@TaskType", taskType));
			sqlParams.Add(new SqlParameter("@NextRunTime", nextRunTime));
			sqlParams.Add(new SqlParameter("@AgentTypeID", AgentTypeID));
			sqlParams.Add(new SqlParameter("@JobFlags", jobFlags));
			sqlParams.Add(new SqlParameter("@SubmittedBy", SubmittedBy));
			sqlParams.Add(new SqlParameter("@LockedByAgentID", locked));
			sqlParams.Add(jobDetails == null
											? new SqlParameter("@JobDetails", DBNull.Value)
											: new SqlParameter("@JobDetails", jobDetails));
			sqlParams.Add(string.IsNullOrEmpty(scheduleRuleType)
											? new SqlParameter("@ScheduleRuleType", DBNull.Value)
											: new SqlParameter("@ScheduleRuleType", scheduleRuleType));
			sqlParams.Add(string.IsNullOrEmpty(serializedScheduleRule)
											? new SqlParameter("@ScheduleRule", DBNull.Value)
											: new SqlParameter("@ScheduleRule", serializedScheduleRule));
			sqlParams.Add(!rootJobID.HasValue || rootJobID.Value == 0
											? new SqlParameter("@RootJobID", DBNull.Value)
											: new SqlParameter("@RootJobID", rootJobID.Value));
			sqlParams.Add(!parentJobID.HasValue || parentJobID.Value == 0
											? new SqlParameter("@ParentJobID", DBNull.Value)
											: new SqlParameter("@ParentJobID", parentJobID.Value));
			int jobId = qDBContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
			return jobId;
		}
	}
}