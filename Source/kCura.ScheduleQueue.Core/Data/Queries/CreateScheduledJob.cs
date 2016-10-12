using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class CreateScheduledJob
	{
		private IQueueDBContext qDBContext = null;

		public CreateScheduledJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public DataRow Execute(int workspaceID,
			int relatedObjectArtifactID,
			string taskType,
			DateTime nextRunTime,
			int AgentTypeID,
			string scheduleRuleType,
			string serializedScheduleRule,
			string jobDetails,
			int jobFlags,
			int SubmittedBy,
			long? rootJobID,
			long? parentJobID = null)
		{
			string sql = string.Format(Resources.CreateScheduledJob, qDBContext.TableName);

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@WorkspaceID", workspaceID));
			sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactID));
			sqlParams.Add(new SqlParameter("@TaskType", taskType));
			sqlParams.Add(new SqlParameter("@NextRunTime", nextRunTime));
			sqlParams.Add(new SqlParameter("@AgentTypeID", AgentTypeID));
			sqlParams.Add(new SqlParameter("@JobFlags", jobFlags));
			sqlParams.Add(new SqlParameter("@SubmittedBy", SubmittedBy));
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

			DataTable dataTable = qDBContext.EddsDBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];

			return row;
		}
	}
}