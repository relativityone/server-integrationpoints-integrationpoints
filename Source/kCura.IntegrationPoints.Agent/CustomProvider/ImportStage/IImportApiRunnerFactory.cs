using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <summary>
    /// The interface for the factory creating ImportAPI runners.
    /// </summary>
    internal interface IImportApiRunnerFactory
    {
        /// <summary>
        /// Builds the ImportAPI runner based on destination configuration.
        /// </summary>
        IImportApiRunner BuildRunner(DestinationConfiguration destinationConfiguration);
    }
}