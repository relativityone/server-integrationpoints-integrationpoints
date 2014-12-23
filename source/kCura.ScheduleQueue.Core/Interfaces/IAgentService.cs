using System;

namespace kCura.ScheduleQueue.Core
{
	public interface IAgentService
	{
		Guid AgentGuid { get; }
		string QueueTable { get; }
		AgentInformation AgentInformation { get; }
		void CreateQueueTable();
		void CreateQueueTableOnce();
	}
}
