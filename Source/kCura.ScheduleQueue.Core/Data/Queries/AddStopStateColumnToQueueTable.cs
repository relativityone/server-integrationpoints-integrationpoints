using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class AddStopStateColumnToQueueTable : ICommand
	{
		private readonly IQueueDBContext _queueDbContext;
		public AddStopStateColumnToQueueTable(IQueueDBContext queueDbContext)
		{
			_queueDbContext = queueDbContext;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.AddStopStateColumnToQueueTable, _queueDbContext.TableName);
			_queueDbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}