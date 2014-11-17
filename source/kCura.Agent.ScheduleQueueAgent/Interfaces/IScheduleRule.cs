using System;

namespace kCura.Agent.ScheduleQueueAgent
{
	public interface IScheduleRules
	{
		ITimeService TimeService { get; set; }
		string Description { get; }
		//DateTime? GetNextRunTime();
		DateTime? GetNextUTCRunDateTime(DateTime? LastRunTime, TaskStatusEnum? lastTaskStatus);
		string ToString();
	}
}
