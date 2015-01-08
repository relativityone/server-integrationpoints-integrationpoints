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

		private IDBContext _eddsDBContext;
		public IDBContext EddsDBContext
		{
			get
			{
				if (_eddsDBContext == null)
				{
					_eddsDBContext = DBHelper.GetDBContext(-1);
				}
				return _eddsDBContext;
			}
		}
	}
}
