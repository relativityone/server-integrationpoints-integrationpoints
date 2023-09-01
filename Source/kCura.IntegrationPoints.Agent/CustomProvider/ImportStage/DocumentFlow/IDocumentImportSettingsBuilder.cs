using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow
{
    /// <summary>
    /// The interface describing the ImportAPI settings builder for document flow.
    /// </summary>
    internal interface IDocumentImportSettingsBuilder
    {
        /// <summary>
        /// Builds the ImportAPI v2.0 configuration object based on the destination configuration and the fields mappings.
        /// </summary>
        /// <param name="destinationConfiguration">The object defining the destination configuration.</param>
        /// <param name="fieldMappings">List of fields mappings to transfer.</param>
        /// <returns>The ImportAPI v2.0 configuration object.</returns>
        Task<DocumentImportConfiguration> BuildAsync(CustomProviderDestinationConfiguration destinationConfiguration, List<IndexedFieldMap> fieldMappings);
    }
}
