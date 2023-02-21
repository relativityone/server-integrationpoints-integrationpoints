namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal interface IImportApiRunnerFactory
    {
        IImportApiRunner BuildRunner(ImportApiFlowEnum importFlow);
    }
}
