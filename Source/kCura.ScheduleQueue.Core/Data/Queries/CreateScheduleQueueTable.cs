using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class CreateScheduleQueueTable
	{
		private IQueueDBContext qDBContext = null;
		public CreateScheduleQueueTable(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.CreateQueueTable, qDBContext.TableName);
			qDBContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
