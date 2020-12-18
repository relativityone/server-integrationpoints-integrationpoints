using Relativity.Sync.SyncConfiguration.Options;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{
	public interface IImageSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IImageSyncConfigurationBuilder>
	{
		IImageSyncConfigurationBuilder ProductionImagePrecedence(ProductionImagePrecedenceOptions options);
	}
}
