namespace kCura.Agent.ScheduleQueueAgent
{
	public interface ITask
	{
		void Execute(Job job);
	}
}
