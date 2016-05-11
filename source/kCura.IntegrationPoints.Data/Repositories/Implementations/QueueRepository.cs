using System;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class QueueRepository : IQueueRepository
	{
		private readonly IDBContext _dbContext;

		public QueueRepository(IHelper helper)
		{
			_dbContext = helper.GetDBContext(-1);
		}

		public int GetNumberOfJobsExecutingOrInQueue(int workspaceId, int integrationPointId)
		{
			//excludes scheduled jobs that are pending
			string queuedOrRunningSql = $@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {workspaceId} AND
										[RelatedObjectArtifactID] = {integrationPointId} AND [ScheduleRuleType] is null";

			string scheduledRunningSql = $@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {workspaceId} AND
										[RelatedObjectArtifactID] = {integrationPointId} AND [ScheduleRuleType] is not null
										AND [LockedByAgentID] is not null";

			int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(queuedOrRunningSql);
			numberOfJobs += _dbContext.ExecuteSqlStatementAsScalar<int>(scheduledRunningSql);

			return numberOfJobs;
		}

		public int GetNumberOfJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime)
		{
			string dateValue = runTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
			string sql = $@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {workspaceId} AND
										[RelatedObjectArtifactID] = {integrationPointId} AND [LockedByAgentID] is not null
										AND [NextRunTime] <= '{dateValue}' AND [JobID] != {jobId}";

			int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(sql);

			return numberOfJobs;
		}
	}
}
