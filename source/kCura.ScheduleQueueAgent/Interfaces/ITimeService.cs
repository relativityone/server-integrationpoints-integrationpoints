using System;

namespace kCura.ScheduleQueueAgent
{
	public interface ITimeService
	{
		DateTime UtcNow { get; }
	}
}
