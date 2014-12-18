using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
	public class QueueDBContext : IQueueDBContext
	{
		public QueueDBContext(IDBContext dbContext, string tableNameName)
		{
			DBContext = dbContext;
			TableName = tableNameName;
		}

		public string TableName { get; private set; }
		public IDBContext DBContext { get; private set; }
	}
}
