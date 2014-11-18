namespace kCura.ScheduleQueueAgent
{
	public interface ITask
	{
		void Execute(Job job);
	}
}
