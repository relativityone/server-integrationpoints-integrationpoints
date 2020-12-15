using System;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration.Options;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{
	public interface IDocumentSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IDocumentSyncConfigurationBuilder>
	{
		IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options);

		IDocumentSyncConfigurationBuilder WithFieldsMapping(Action<IFieldsMappingBuilder> fieldsMappingFunc);
	}
}