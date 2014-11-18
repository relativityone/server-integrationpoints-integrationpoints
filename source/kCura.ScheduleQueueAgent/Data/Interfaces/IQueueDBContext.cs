using Relativity.API;

namespace kCura.ScheduleQueueAgent.Data
{
	public interface IQueueDBContext
	{
		string QueueTable { get; }
		IDBContext DBContext { get; }
	}
}
