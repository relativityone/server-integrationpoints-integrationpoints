using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class CreateJobLogTable
	{
		private readonly IQueueDBContext qDBContext = null;
		public CreateJobLogTable(IQueueDBContext qDBContext)
		{
			this.qDBContext = qDBContext;
		}

		public void Execute()
		{
			string sql = string.Format(Resources.CreateJobLogTable, qDBContext.TableName);
			qDBContext.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
