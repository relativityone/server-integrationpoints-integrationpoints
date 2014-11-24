using System;

namespace kCura.ScheduleQueueAgent.ScheduleRules
{
	public class DefaultTimeService : ITimeService
	{
		public DateTime UtcNow
		{
			get { return DateTime.UtcNow; }
		}
	}
}
