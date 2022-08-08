using System;

namespace kCura.ScheduleQueue.Core
{
    public interface IAgentService
    {
        AgentTypeInformation AgentTypeInformation { get; }
        void InstallQueueTable();
        void CreateQueueTableOnce();
    }
}
