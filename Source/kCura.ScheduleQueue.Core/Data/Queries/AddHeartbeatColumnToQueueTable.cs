using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
    public class AddHeartbeatColumnToQueueTable : ICommand
    {
		private readonly IQueueDBContext _queueDbContext;
		public AddHeartbeatColumnToQueueTable(IQueueDBContext queueDbContext)
		{
			_queueDbContext = queueDbContext;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.AddHeartbeatColumnToQueueTable, _queueDbContext.TableName);
			_queueDbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
