using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDocumentSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IDocumentSyncConfigurationBuilder>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options);
	}
}