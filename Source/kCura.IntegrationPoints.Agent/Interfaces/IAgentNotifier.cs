using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
    internal interface IAgentNotifier
    {
        void NotifyAgent(LogCategory category, string message);
    }
}
