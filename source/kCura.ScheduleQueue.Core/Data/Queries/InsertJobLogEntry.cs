using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class InsertJobLogEntry
	{
		private IQueueDBContext qDBContext = null;
		public InsertJobLogEntry(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute(
										long jobID,
										string taskType,
										int jobState,
										Int32? agentID,
										Int32? relatedObjectArtifactID,
										Int32 createdBy,
										string details
									)
		{
			var sql = string.Format(Properties.Resources.InsertJobLogEntry, qDBContext.TableName);
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@JobID", jobID));
			sqlParams.Add(new SqlParameter("@TaskType", taskType));
			sqlParams.Add(new SqlParameter("@Status", jobState));
			sqlParams.Add(agentID == null ? new SqlParameter("@AgentID", DBNull.Value) : new SqlParameter("@AgentID", agentID));
			sqlParams.Add(relatedObjectArtifactID == null ? new SqlParameter("@RelatedObjectArtifactID", DBNull.Value) : new SqlParameter("@RelatedObjectArtifactID", relatedObjectArtifactID));
			sqlParams.Add(new SqlParameter("@CreatedBy", createdBy));
			sqlParams.Add(details == null ? new SqlParameter("@Details", DBNull.Value) : new SqlParameter("@Details", details));

			qDBContext.DBContext.ExecuteNonQuerySQLStatement(sql, sqlParams.ToArray());
		}
	}
}
