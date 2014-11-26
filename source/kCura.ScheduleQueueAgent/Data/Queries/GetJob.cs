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
			string sql = string.Format(Resources.GetJobByID, qDBContext.TableName);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));

			return Execute(sql, sqlParams);
		}

		public DataRow Execute(int workspaceID, int relatedObjectArtifactID, string taskType)
		{
			string sql = string.Format(Resources.GetJobByRelatedObjectID, qDBContext.TableName);
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@WorkspaceID", workspaceID));
			sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactID));
			sqlParams.Add(new SqlParameter("@TaskType", taskType));

			return Execute(sql, sqlParams);
		}

		private DataRow Execute(string sql, List<SqlParameter> sqlParams)
		{
			var dataTable = qDBContext.DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];

			return row;
		}
	}
}
