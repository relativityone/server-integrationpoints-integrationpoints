using System;
using Relativity.Sync.SyncConfiguration.FieldsMapping;

namespace Relativity.Sync.SyncConfiguration
{
    /// <summary>
    /// Provides methods for configuring non-document objects specific flow
    /// </summary>
    public interface INonDocumentSyncConfigurationBuilder : ISyncConfigurationRootBuilder<INonDocumentSyncConfigurationBuilder>
    {
        /// <summary>
        /// Configures fields mapping.
        /// </summary>
        /// <param name="fieldsMappingAction">Fields mapping builder options.</param>
        INonDocumentSyncConfigurationBuilder WithFieldsMapping(Action<IFieldsMappingBuilder> fieldsMappingAction);
    }
}
