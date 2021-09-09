using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
	public interface IRelativitySyncConstrainsChecker
	{
		bool ShouldUseRelativitySync(Job job);
	}
}
