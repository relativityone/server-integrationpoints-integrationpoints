using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <summary>
    /// The interface describing the ImportAPI settings builder, independent of the ImportAPI flow.
    /// </summary>
    internal interface IRdoImportSettingsBuilder
    {
        /// <summary>
        /// Builds the ImportAPI v2.0 configuration object based on the destination configuration and the fields mappings.
        /// </summary>
        /// <param name="destinationConfiguration">The object defining the destination configuration.</param>
        /// <param name="fieldMappings">List of fields mappings to transfer.</param>
        /// <returns>The ImportAPI v2.0 configuration object.</returns>
        Task<RdoImportConfiguration> BuildAsync(ImportSettings destinationConfiguration, List<FieldMapWrapper> fieldMappings);
    }
}
