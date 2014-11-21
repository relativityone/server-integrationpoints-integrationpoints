using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
{
	public class GetJob
	{
		private IQueueDBContext qDBContext = null;

		public GetJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public DataRow Execute(long jobID)
		{
			string sql = string.Format(Resources.GetJobByID, qDBContext.QueueTable);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));

			var dataTable = qDBContext.DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];
			else
				row = null;

			return row;
		}

		public DataRow Execute(int workspaceID, int relatedObjectArtifactID, string taskType)
		{
			string sql = string.Format(Resources.GetJobByID, qDBContext.QueueTable);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@WorkspaceID", workspaceID));
			sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactID));
			sqlParams.Add(new SqlParameter("@TaskType", taskType));

			var dataTable = qDBContext.DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];
			else
				row = null;

			return row;
		}
	}
}
