namespace kCura.ScheduleQueueAgent
{
	public interface ITaskFactory
	{
		ITask GetTask(Job job);
	}
}
