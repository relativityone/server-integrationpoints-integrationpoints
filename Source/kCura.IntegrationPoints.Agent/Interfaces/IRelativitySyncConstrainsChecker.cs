using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
	public interface IRelativitySyncConstrainsChecker
	{
		bool ShouldUseRelativitySync(Job job);
	}
}
