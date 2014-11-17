using System;

namespace kCura.Agent.ScheduleQueueAgent
{
	public interface ITimeService
	{
		DateTime UtcNow { get; }
	}
}
