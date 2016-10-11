using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class QueueRepository : IQueueRepository
	{
		private readonly IDBContext _dbContext;
		private string _queueTableName = GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME;

		public QueueRepository(IHelper helper)
		{
			_dbContext = helper.GetDBContext(-1);
		}

		public int GetNumberOfJobsExecutingOrInQueue(int workspaceId, int integrationPointId)
		{
			//excludes scheduled jobs that are pending
			string queuedOrRunningSql = $@"SELECT count(*) FROM [{_queueTableName}] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [ScheduleRuleType] is null";

			IEnumerable<SqlParameter> queuedOrRunningParameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId},
			};

			string scheduledRunningSql = $@"SELECT count(*) FROM [{_queueTableName}] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [ScheduleRuleType] is not null AND [LockedByAgentID] is not null";

			IEnumerable<SqlParameter> scheduledRunningParameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId},
			};


			int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql, queuedOrRunningParameters);
			numberOfJobs += _dbContext.ExecuteSqlStatementAsScalar<int>(scheduledRunningSql, scheduledRunningParameters);

			return numberOfJobs;
		}

		public int GetNumberOfJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime)
		{
			string sql = $@"SELECT count(*) FROM [{_queueTableName}] WHERE [WorkspaceID] = @workspaceId AND [RelatedObjectArtifactID] = @integrationPointId AND [LockedByAgentID] is not null AND [NextRunTime] <= @dateValue AND [JobID] != @jobId";

			IEnumerable<SqlParameter> parameters = new List<SqlParameter>
			{
				new SqlParameter("@workspaceId", SqlDbType.Int) {Value = workspaceId},
				new SqlParameter("@integrationPointId", SqlDbType.Int) {Value = integrationPointId},
				new SqlParameter("@dateValue", SqlDbType.DateTime) {Value = runTime},
				new SqlParameter("@jobId", SqlDbType.BigInt) {Value = jobId}
			};

			int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(sql, parameters);

			return numberOfJobs;
		}
	}
}