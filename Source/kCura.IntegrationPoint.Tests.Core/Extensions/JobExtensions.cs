using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
	public static class JobExtensions
	{
		public static T CreateJob<T>(int workspaceArtifactId, long jobId, Func<DataRow, T> creator)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["WorkspaceID"] = workspaceArtifactId;

			return creator(jobData);
		}

		public static Job CreateJob(int workspaceArtifactId, long jobId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["WorkspaceID"] = workspaceArtifactId;
			return new Job(jobData);
		}

		public static Job CreateJob(int workspaceArtifactId, long integrationPointArtifactId, int submittedByArtifactId, long jobId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["SubmittedBy"] = submittedByArtifactId;
			jobData["WorkspaceID"] = workspaceArtifactId;

			return new Job(jobData);
		}

		public static Job CreateJob(int workspaceArtifactId, long integrationPointArtifactId, int submittedByArtifactId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["SubmittedBy"] = submittedByArtifactId;
			jobData["WorkspaceID"] = workspaceArtifactId;

			return new Job(jobData);
		}

		public static Job CreateJob(int workspaceArtifactId, long integrationPointArtifactId, int submittedByArtifactId, long jobId, long rootJobId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["SubmittedBy"] = submittedByArtifactId;
			jobData["WorkspaceID"] = workspaceArtifactId;
			jobData["RootJobId"] = rootJobId;
			return new Job(jobData);
		}

		public static Job CreateJob(int workspaceArtifactId, long integrationPointArtifactId, string jobDetails)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["JobDetails"] = jobDetails;
			jobData["WorkspaceID"] = workspaceArtifactId;

			return new Job(jobData);
		}

		public static Job CreateJobAgentTypeId(int workspaceArtifactId, long integrationPointArtifactId, long jobId, int agentTypeId, long rootJobId, DateTime dateTime)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["RelatedObjectArtifactID"] = integrationPointArtifactId;
			jobData["WorkspaceID"] = workspaceArtifactId;
			jobData["AgentTypeID"] = agentTypeId;
			jobData["RootJobId"] = rootJobId;
			jobData["NextRunTime"] = dateTime;

			return new Job(jobData);
		}

		public static Job CopyJobWithStopState(this Job job, StopState state)
		{
			DataRow row = ConvertToDataRow(job);
			row["StopState"] = (int)state;
			return new Job(row);
		}

		public static Job CopyJobWithJobId(this Job job, long jobId)
		{
			DataRow row = ConvertToDataRow(job);
			row["JobID"] = jobId;
			return new Job(row);
		}

		public static Job CreateJob()
		{
			DataRow jobData = CreateDefaultJobData();

			return new Job(jobData);
		}

		public static Job CreateJob(long jobId)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			return new Job(jobData);
		}

		public static Job CreateJob(long jobId, string scheduleRuleType)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["ScheduleRuleType"] = scheduleRuleType;
			return new Job(jobData);
		}

		public static Job CreateJob(long jobId, TaskType taskType, int relatedObjectArtifactID)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = jobId;
			jobData["TaskType"] = taskType.ToString();
			jobData["RelatedObjectArtifactID"] = relatedObjectArtifactID;

			return new Job(jobData);
		}

		private static DataRow ConvertToDataRow(Job job)
		{
			DataRow jobData = CreateDefaultJobData();
			jobData["JobID"] = job.JobId;
			jobData["RootJobId"] = job.RootJobId;
			jobData["ParentJobId"] = job.ParentJobId;
			jobData["AgentTypeID"] = job.AgentTypeID;
			jobData["LockedByAgentID"] = job.LockedByAgentID;
			jobData["WorkspaceID"] = job.WorkspaceID;
			jobData["RelatedObjectArtifactID"] = job.RelatedObjectArtifactID;
			jobData["TaskType"] = job.TaskType;
			jobData["NextRunTime"] = job.NextRunTime;
			jobData["LastRunTime"] = (object) job.LastRunTime ?? DBNull.Value;
			jobData["JobDetails"] = job.JobDetails;
			jobData["JobFlags"] = job.JobFlags;
			jobData["SubmittedDate"] = job.SubmittedDate;
			jobData["SubmittedBy"] = job.SubmittedBy;
			jobData["ScheduleRuleType"] = job.ScheduleRuleType;
			jobData["ScheduleRule"] = job.SerializedScheduleRule;
			jobData["StopState"] = (int) job.StopState;

			return jobData;
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
			table.Columns.Add(new DataColumn("StopState", typeof(int)));

			DataRow jobData = table.NewRow();
			jobData["JobID"] = default(long);
			jobData["RootJobId"] = default(long);
			jobData["ParentJobId"] = default(long);
			jobData["AgentTypeID"] = default(int);
			jobData["LockedByAgentID"] = default(int);
			jobData["WorkspaceID"] = default(int);
			jobData["RelatedObjectArtifactID"] = default(int);
			jobData["TaskType"] = TaskType.ExportService.ToString();
			jobData["NextRunTime"] = default(DateTime);
			jobData["LastRunTime"] = default(DateTime);
			jobData["JobDetails"] = new JSONSerializer().Serialize(new TaskParameters() { BatchInstance = Guid.NewGuid()});
			jobData["JobFlags"] = default(int);
			jobData["SubmittedDate"] = default(DateTime);
			jobData["SubmittedBy"] = default(int);
			jobData["ScheduleRuleType"] = default(string);
			jobData["ScheduleRule"] = default(string);
			jobData["StopState"] = default(int);

			return jobData;
		}

		private static string insertJob = @"INSERT INTO [eddsdbo].[{0}]
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
			,[StopState]
		)
		OUTPUT
			Inserted.[JobID]
			,Inserted.[RootJobID]
			,Inserted.[ParentJobID]
			,Inserted.[AgentTypeID]
			,Inserted.[LockedByAgentID]
			,Inserted.[WorkspaceID]
			,Inserted.[RelatedObjectArtifactID]
			,Inserted.[TaskType]
			,Inserted.[NextRunTime]
			,Inserted.[LastRunTime]
			,Inserted.[ScheduleRuleType]
			,Inserted.[ScheduleRule]
			,Inserted.[JobDetails]
			,Inserted.[JobFlags]
			,Inserted.[SubmittedDate]
			,Inserted.[SubmittedBy]
			,Inserted.[StopState]
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
			,@StopState
		)";

		public static Job Execute(IQueueDBContext qDBContext,
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
												long? parentJobID = null,
												int stopState = (int) StopState.None
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
			sqlParams.Add(new SqlParameter("@StopState", stopState));

			using (DataTable dataTable = qDBContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams))
			{
				DataRow row = null;
				if (dataTable?.Rows?.Count > 0)
				{
					row = dataTable.Rows[0];
				}

				Job job = new Job(row);
				return job;
			}
		}
	}
}