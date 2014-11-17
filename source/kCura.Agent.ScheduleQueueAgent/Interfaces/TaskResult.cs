using System;
using System.Collections.Generic;

namespace kCura.Agent.ScheduleQueueAgent
{
	public class TaskResult
	{
		public TaskStatusEnum Status { get; set; }
		public IEnumerable<Exception> Exceptions { get; set; }
	}
}
