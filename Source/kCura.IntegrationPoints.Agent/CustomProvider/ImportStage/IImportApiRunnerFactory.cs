using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage
{
    /// <summary>
    /// The interface for the factory creating ImportAPI runners.
    /// </summary>
    internal interface IImportApiRunnerFactory
    {
        /// <summary>
        /// Builds the ImportAPI runner based on destination configuration.
        /// </summary>
        IImportApiRunner BuildRunner(CustomProviderDestinationConfiguration destinationConfiguration);
    }
}
