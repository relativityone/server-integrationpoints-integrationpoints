﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.ScheduleQueueAgent.Properties;
using kCura.ScheduleQueueAgent.TimeMachine;

namespace kCura.ScheduleQueueAgent.Data.Queries
{
	public class GetNextJob
	{
		private IQueueDBContext qDBContext = null;
		public GetNextJob(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public DataRow Execute(int agentID, int agentTypeID, int[] resourceGroupArtifactID)
		{
			string sql = string.Format(Resources.GetNextJob, qDBContext.TableName);
			string ResourceGroupArtifactIDs = string.Join(",", resourceGroupArtifactID);
			sql = sql.Replace("@ResourceGroupArtifactIDs", ResourceGroupArtifactIDs);

#if TIME_MACHINE
			if (AgentTimeMachineProvider.Current.Enabled)
			{
				if (AgentTimeMachineProvider.Current.WorkspaceID > 0)
				{
					sql = sql.Replace("q.[NextRunTime] <= GETUTCDATE()", String.Format("((q.[NextRunTime] <= GETUTCDATE() AND q.[WorkspaceID]<>{0}) OR (q.[NextRunTime] <= CAST('{1}' AS DATETIME) AND q.[WorkspaceID]={0}))", AgentTimeMachineProvider.Current.WorkspaceID, AgentTimeMachineProvider.Current.UtcNow));
				}
				else
				{
					sql = sql.Replace("GETUTCDATE()", String.Format("CAST('{0}' AS DATETIME)", AgentTimeMachineProvider.Current.UtcNow));
				}
			}
#endif

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@AgentID", agentID));
			sqlParams.Add(new SqlParameter("@AgentTypeID", agentTypeID));

			var dataTable = qDBContext.DBContext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());

			DataRow row = null;
			if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
				row = dataTable.Rows[0];

			return row;
		}
	}
}
