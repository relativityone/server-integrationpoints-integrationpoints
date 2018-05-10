using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
	internal interface IAgentNotifier
	{
		void NotifyAgent(int level, LogCategory category, string message);
	}
}
