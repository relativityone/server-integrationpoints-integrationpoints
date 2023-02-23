namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <summary>
    /// The interface for the factory creating ImportAPI runners.
    /// </summary>
    internal interface IImportApiRunnerFactory
    {
        /// <summary>
        /// Builds the ImportAPI runner based on the ImportAPI flow (transferred items type).
        /// </summary>
        IImportApiRunner BuildRunner(ImportApiFlowEnum importFlow);
    }
}
