using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
    /// <summary> 
    /// Provides methods for configuring image specific flow. 
    /// </summary>
    public interface IImageSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IImageSyncConfigurationBuilder>
    {
        /// <summary>
        /// Configures production image precedence.
        /// </summary>
        /// <param name="options">Production image precedence options.</param>
        IImageSyncConfigurationBuilder ProductionImagePrecedence(ProductionImagePrecedenceOptions options);
    }
}
