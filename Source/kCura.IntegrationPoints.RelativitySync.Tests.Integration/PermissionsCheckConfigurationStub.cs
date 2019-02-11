using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class PermissionsCheckConfigurationStub : IPermissionsCheckConfiguration
	{
		public int ExecutingUserId { get; set; }
	}
}
