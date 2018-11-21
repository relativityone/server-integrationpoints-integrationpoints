using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
	public static class JobExtensions
	{
		public static Job CopyJobWithStopState(this Job job, StopState state)
		{
			return new JobBuilder().WithJob(job).WithStopState(state).Build();
		}

		public static Job CopyJobWithJobId(this Job job, long jobId)
		{
			return new JobBuilder().WithJob(job).WithJobId(jobId).Build();
		}

		public static Job CreateJob()
		{
			return new JobBuilder().Build();
		}

		private const string _INSERT_JOB_QUERY = @"INSERT INTO [eddsdbo].[{0}]
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
												int stopState = (int)StopState.None
												)
		{
			string sql = string.Format(_INSERT_JOB_QUERY, qDBContext.TableName);

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