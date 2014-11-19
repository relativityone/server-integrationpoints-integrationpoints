using System;

namespace kCura.ScheduleQueueAgent
{
	public interface IScheduleRule
	{
		ITimeService TimeService { get; set; }
		string Description { get; }
		DateTime? GetNextUTCRunDateTime(DateTime? LastRunTime, TaskStatusEnum? lastTaskStatus);
		string ToString();
	}
}
