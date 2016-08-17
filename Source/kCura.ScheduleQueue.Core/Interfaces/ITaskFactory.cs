namespace kCura.ScheduleQueue.Core
{
	public interface ITaskFactory
	{
		ITask GetTask(Job job);
	}
}
