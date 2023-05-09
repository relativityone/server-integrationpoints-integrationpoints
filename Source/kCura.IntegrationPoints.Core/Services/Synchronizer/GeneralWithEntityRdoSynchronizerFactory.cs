using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
    public class GeneralWithEntityRdoSynchronizerFactory : ISynchronizerFactory
    {
        private readonly RelativityRdoSynchronizerFactory _relativityRdoSynchronizerFactory;
        private readonly ImportProviderRdoSynchronizerFactory _importProviderRdoSynchronizerFactory;
        private readonly IDiagnosticLog _diagnosticLog;

        public GeneralWithEntityRdoSynchronizerFactory(IWindsorContainer container, IObjectTypeRepository objectTypeRepository, IDiagnosticLog diagnosticLog)
        {
            _relativityRdoSynchronizerFactory = new RelativityRdoSynchronizerFactory(container);
            _importProviderRdoSynchronizerFactory = new ImportProviderRdoSynchronizerFactory(container, objectTypeRepository);
            _diagnosticLog = diagnosticLog;
        }

        public ITaskJobSubmitter TaskJobSubmitter { get; set; }

        public SourceProvider SourceProvider { get; set; }

        public IDataSynchronizer CreateSynchronizer(Guid identifier, DestinationConfiguration options)
        {
            return string.Equals(options.Provider, nameof(ProviderType.Relativity), StringComparison.InvariantCultureIgnoreCase)
                ? _relativityRdoSynchronizerFactory.CreateSynchronizer(options, SourceProvider, _diagnosticLog)
                : _importProviderRdoSynchronizerFactory.CreateSynchronizer(options, TaskJobSubmitter, _diagnosticLog);
        }
    }
}
