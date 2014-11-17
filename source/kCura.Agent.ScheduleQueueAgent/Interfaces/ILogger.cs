namespace kCura.Agent.ScheduleQueueAgent
{
	public interface ILogger
	{
		void Log(string message, string detailmessage, LogCategory category);
	}
}
