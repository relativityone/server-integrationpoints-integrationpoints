using System;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public interface ITaskExceptionMediator
    {
        void RegisterEvent(ScheduleQueueAgentBase agent);
    }
}
