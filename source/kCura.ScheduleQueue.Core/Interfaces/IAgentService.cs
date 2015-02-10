using System;

namespace kCura.ScheduleQueue.Core
{
	public interface IAgentService
	{
		Guid AgentGuid { get; }
		string QueueTable { get; }
		AgentTypeInformation AgentTypeInformation { get; }
		void CreateQueueTable();
		void CreateQueueTableOnce();
	}
}
