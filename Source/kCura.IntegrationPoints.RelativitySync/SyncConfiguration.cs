using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class SyncConfiguration : IUserContextConfiguration
	{
		public SyncConfiguration(int submittedBy)
		{
			ExecutingUserId = submittedBy;
		}

		public int ExecutingUserId { get; }
	}
}