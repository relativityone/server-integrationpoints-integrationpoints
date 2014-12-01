using Relativity.API;

namespace kCura.ScheduleQueueAgent.Data
{
	public interface IQueueDBContext
	{
		string TableName { get; }
		IDBContext DBContext { get; }
	}
}
