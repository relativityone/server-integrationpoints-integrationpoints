using Relativity.API;

namespace kCura.ScheduleQueueAgent.Data
{
	public class QueueDBContext : IQueueDBContext
	{
		public QueueDBContext(IDBContext dbContext, string queueTableName)
		{
			DBContext = dbContext;
			QueueTable = queueTableName;
		}

		public string QueueTable { get; private set; }
		public IDBContext DBContext { get; private set; }
	}
}
