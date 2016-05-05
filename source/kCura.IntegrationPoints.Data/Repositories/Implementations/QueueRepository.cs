using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class QueueRepository : IQueueRepository
	{
		private readonly IDBContext _dbContext;

		public QueueRepository(IDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		public bool HasJobsExecutingOrInQueue(int workspaceId, int integrationPointId)
		{
			string sql = $@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {workspaceId} AND
										[RelatedObjectArtifactID] = {integrationPointId} AND [ScheduleRuleType] is null";

			int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(sql);

			if (numberOfJobs > 0)
			{
				return true;
			}

			return false;
		}

		public bool HasJobsExecuting(int workspaceId, int integrationPointId)
		{
			string sql = $@"SELECT count(*) FROM [ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]
										WHERE [WorkspaceID] = {workspaceId} AND
										[RelatedObjectArtifactID] = {integrationPointId} AND [LockedByAgentID] is not null";

			int numberOfJobs = _dbContext.ExecuteSqlStatementAsScalar<int>(sql);

			if (numberOfJobs > 0)
			{
				return true;
			}

			return false;
		}
	}
}
