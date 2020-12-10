using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// 
	/// </summary>
	public interface IImageSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IImageSyncConfigurationBuilder>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		IImageSyncConfigurationBuilder ProductionImagePrecedence(ProductionImagePrecedenceOptions options);
	}
}
