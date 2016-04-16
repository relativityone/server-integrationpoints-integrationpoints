using System;
using kCura.ScheduleQueue.Core.Properties;

namespace kCura.ScheduleQueue.Core.Data.Queries
{
	public class CleanupJobQueueTable
	{
		private readonly IQueueDBContext _context;

		public CleanupJobQueueTable(IQueueDBContext context)
		{
			_context = context;
		}

		public void Execute()
		{
			string sql = String.Format(Resources.CleanupJobQueueTable, _context.TableName);
			_context.EddsDBContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}