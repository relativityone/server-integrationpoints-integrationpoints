namespace kCura.ScheduleQueue.Core.Data.Interfaces
{
	public interface IQuery<out T>
	{
		T Execute();
	}
}
