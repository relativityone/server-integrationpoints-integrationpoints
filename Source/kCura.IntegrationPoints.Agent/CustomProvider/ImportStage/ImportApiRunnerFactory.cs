using System;
using kCura.Vendor.Castle.Windsor;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportApiRunnerFactory : IImportApiRunnerFactory
    {
        private readonly IWindsorContainer _container;

        public ImportApiRunnerFactory(IWindsorContainer container)
        {
            _container = container;
        }

        public IImportApiRunner BuildRunner(ImportApiFlowEnum importFlow)
        {
            switch (importFlow)
            {
                case ImportApiFlowEnum.Document: return _container.Resolve<DocumentImportApiRunner>();
                case ImportApiFlowEnum.Rdo: return _container.Resolve<RdoImportApiRunner>();

                default: throw new NotSupportedException($"Unknown ImportApiFlowEnum: {importFlow}");
            }
        }
    }
}
