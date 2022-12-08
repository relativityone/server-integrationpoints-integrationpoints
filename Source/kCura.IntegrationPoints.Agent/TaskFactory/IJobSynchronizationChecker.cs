using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.AgentBase;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    public interface IJobSynchronizationChecker
    {
        void CheckForSynchronization(Type type, Job job, IntegrationPointDto integrationPointDto, ScheduleQueueAgentBase agentBase);
    }
}
