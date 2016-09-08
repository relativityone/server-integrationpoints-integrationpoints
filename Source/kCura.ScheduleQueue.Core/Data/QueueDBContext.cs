using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
	public class QueueDBContext : IQueueDBContext
	{
		public QueueDBContext(IHelper dbHelper, string tableNameName)
		{
			this.DBHelper = dbHelper;
			this.TableName = tableNameName;
		}

		public IHelper DBHelper { get; private set; }
		public string TableName { get; private set; }

		private readonly object _lock = new object();
		public IDBContext EddsDBContext
		{
			get
			{
				lock (_lock)
				{
					return DBHelper.GetDBContext(-1);
				}
			}
		}
	}
}
