using System;

namespace kCura.ScheduleQueueAgent.ScheduleRules
{
	public interface IScheduleRule
	{
		ITimeService TimeService { get; set; }
		string Description { get; }
		DateTime? GetNextUTCRunDateTime(DateTime? LastRunTime = null, TaskStatusEnum? lastTaskStatus = null);
		string ToSerializedString();
	}
}
