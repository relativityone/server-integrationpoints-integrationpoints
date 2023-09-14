using System.Collections.Generic;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow
{
    /// <summary>
    /// The interface describing the ImportAPI settings builder for rdo flow.
    /// </summary>
    internal interface IRdoImportSettingsBuilder
    {
        /// <summary>
        /// Builds the ImportAPI v2.0 configuration object based on the destination configuration and the fields mappings.
        /// </summary>
        /// <param name="destinationConfiguration">The object defining the destination configuration.</param>
        /// <param name="fieldMappings">List of fields mappings to transfer.</param>
        /// <param name="identifierField">Identifier field</param>
        /// <returns>The ImportAPI v2.0 configuration object.</returns>
        RdoImportConfiguration Build(CustomProviderDestinationConfiguration destinationConfiguration, List<IndexedFieldMap> fieldMappings, IndexedFieldMap identifierField);
    }
}
