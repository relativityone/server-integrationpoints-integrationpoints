﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueue.Core.Properties;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class GetAgentInformation
	{
		private IDBContext DBContext = null;
		public GetAgentInformation(IDBContext dbContext)
		{
			this.DBContext = dbContext;
		}

		public DataRow Execute(int agentID)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@AgentID", agentID));
			sqlParams.Add(new SqlParameter("@AgentGuid", DBNull.Value));

			return Execute(sqlParams);
		}

		public DataRow Execute(Guid agentGuid)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@AgentID", DBNull.Value));
			sqlParams.Add(new SqlParameter("@AgentGuid", agentGuid));

			return Execute(sqlParams);
		}

		public DataRow Execute(IEnumerable<SqlParameter> sqlParams)
		{
			string sql = Resources.GetAgentInformation;

			var dataTable = DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];
			else
				row = null;

			return row;
		}
	}
}
