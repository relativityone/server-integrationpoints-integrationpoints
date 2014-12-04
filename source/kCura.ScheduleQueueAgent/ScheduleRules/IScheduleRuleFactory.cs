namespace kCura.ScheduleQueueAgent.ScheduleRules
{
	public interface IScheduleRuleFactory
	{
		IScheduleRule Deserialize(Job job);
	}
}
