using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.Agent.ScheduleQueueAgent.Properties;

namespace kCura.ScheduleQueueAgent.Data.Queries
{
	public class GetAgentInformation
	{
		private IQueueDBContext qDBContext = null;
		public GetAgentInformation(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public DataRow Execute(int agentID)
		{
			string sql = Resources.GetAgentInformation;

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@AgentID", agentID));

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
