using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
	public interface IScheduleRuleFactory
	{
		IScheduleRule Deserialize(Job job);
	}
}
