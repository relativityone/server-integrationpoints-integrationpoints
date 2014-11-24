﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
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
												string serializedScheduleRule,
												string jobDetail,
												int jobFlags,
												int SubmittedBy)
		{
			string sql = string.Format(Resources.CreateScheduledJob, qDBContext.QueueTable);

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@WorkspaceID", workspaceID));
			sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactID));
			sqlParams.Add(new SqlParameter("@TaskType", taskType));
			sqlParams.Add(new SqlParameter("@NextRunTime", nextRunTime));
			sqlParams.Add(new SqlParameter("@AgentTypeID", AgentTypeID));
			sqlParams.Add(new SqlParameter("@JobFlags", jobFlags));
			sqlParams.Add(new SqlParameter("@SubmittedBy", SubmittedBy));
			sqlParams.Add(jobDetail == null
											? new SqlParameter("@JobDetail", DBNull.Value)
											: new SqlParameter("@JobDetail", jobDetail));
			sqlParams.Add(string.IsNullOrEmpty(serializedScheduleRule)
											? new SqlParameter("@ScheduleRule", DBNull.Value)
											: new SqlParameter("@ScheduleRule", serializedScheduleRule));

			DataTable dataTable = qDBContext.DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];

			return row;
		}
	}
}
