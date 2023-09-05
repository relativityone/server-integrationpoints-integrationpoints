using Castle.Windsor;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.DocumentFlow;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage
{
    /// <inheritdoc />
    internal class ImportApiRunnerFactory : IImportApiRunnerFactory
    {
        private readonly IWindsorContainer _container;
        private readonly IAPILog _logger;

        public ImportApiRunnerFactory(IWindsorContainer container, IAPILog logger)
        {
            _container = container;
            _logger = logger;
        }

        /// <inheritdoc />
        public IImportApiRunner BuildRunner(CustomProviderDestinationConfiguration destinationConfiguration)
        {
            _logger.LogInformation("Creating ImportApiRunner based on destination configuration: {@destinationConfiguration}", destinationConfiguration);

            if (IsDocumentFlow(destinationConfiguration.ArtifactTypeId))
            {
                return _container.Resolve<DocumentImportApiRunner>();
            }
            else
            {
                return _container.Resolve<RdoImportApiRunner>();
            }
        }

        private bool IsDocumentFlow(int artifactTypeId)
        {
            return artifactTypeId == (int)ArtifactType.Document;
        }
    }
}
