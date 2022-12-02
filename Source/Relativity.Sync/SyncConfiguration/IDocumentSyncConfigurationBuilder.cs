using System;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
    /// <summary>
    /// Provides methods for configuring document specific flow.
    /// </summary>
    public interface IDocumentSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IDocumentSyncConfigurationBuilder>
    {
        /// <summary>
        /// Configures destination folder structure.
        /// </summary>
        /// <param name="options">Destination folder structure options.</param>
        IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options);

        /// <summary>
        /// Configures fields mapping.
        /// </summary>
        /// <param name="fieldsMapping">Fields mapping builder options.</param>
        IDocumentSyncConfigurationBuilder WithFieldsMapping(Action<IFieldsMappingBuilder> fieldsMapping);
    }
}
