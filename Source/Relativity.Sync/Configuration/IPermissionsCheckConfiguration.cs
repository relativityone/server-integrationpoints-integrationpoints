namespace Relativity.Sync.Configuration
{
	internal interface IPermissionsCheckConfiguration : IConfiguration
	{
		int ExecutingUserId { get; }
	}
}