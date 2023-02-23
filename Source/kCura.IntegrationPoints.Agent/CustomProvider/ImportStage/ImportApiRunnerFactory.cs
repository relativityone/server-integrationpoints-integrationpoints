using System;
using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
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
        public IImportApiRunner BuildRunner(ImportApiFlowEnum importFlow)
        {
            _logger.LogInformation("Creating ImportApiRunner based on import flow: {ImportFlow}", importFlow);

            switch (importFlow)
            {
                case ImportApiFlowEnum.Document: return _container.Resolve<DocumentImportApiRunner>();
                case ImportApiFlowEnum.Rdo: return _container.Resolve<RdoImportApiRunner>();

                default: throw new NotSupportedException($"Unknown ImportApiFlowEnum: {importFlow}");
            }
        }
    }
}
