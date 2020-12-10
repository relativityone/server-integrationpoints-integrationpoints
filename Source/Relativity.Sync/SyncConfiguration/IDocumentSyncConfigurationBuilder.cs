using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	public interface IDocumentSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IDocumentSyncConfigurationBuilder>
	{
		IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options);
	}
}