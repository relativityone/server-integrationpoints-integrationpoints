using System;

namespace kCura.ScheduleQueueAgent.ScheduleRules
{
	public interface ITimeService
	{
		DateTime UtcNow { get; }
	}
}
