using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	public interface IImageSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IImageSyncConfigurationBuilder>
	{
		IImageSyncConfigurationBuilder ProductionImagePrecedence(ProductionImagePrecedenceOptions options);
	}
}
