using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class AddCustomColumnsToQueueTable : ICommand
	{
		private readonly IQueueDBContext _queueDbContext;
		public AddCustomColumnsToQueueTable(IQueueDBContext queueDbContext)
		{
			_queueDbContext = queueDbContext;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.AddCustomColumnsToQueueTable, _queueDbContext.TableName);
			_queueDbContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}