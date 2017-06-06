﻿using System;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
	public interface IScheduleRule
	{
		ITimeService TimeService { get; set; }
		string Description { get; }
		DateTime? GetNextUTCRunDateTime();
		string ToSerializedString();
	}
}
