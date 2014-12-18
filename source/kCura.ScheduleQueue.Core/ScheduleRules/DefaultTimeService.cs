using System;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
	public class DefaultTimeService : ITimeService
	{
		public DateTime UtcNow
		{
			get { return DateTime.UtcNow; }
		}
	}
}
