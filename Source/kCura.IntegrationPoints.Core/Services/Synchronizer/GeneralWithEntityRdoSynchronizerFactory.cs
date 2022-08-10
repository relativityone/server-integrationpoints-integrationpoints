using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

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

        public IDataSynchronizer CreateSynchronizer(Guid identifier, string options)
        {
            ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(options);

            return importSettings.IsRelativityProvider()
                ? _relativityRdoSynchronizerFactory.CreateSynchronizer(importSettings, SourceProvider, _diagnosticLog)
                : _importProviderRdoSynchronizerFactory.CreateSynchronizer(importSettings, TaskJobSubmitter, _diagnosticLog);
        }
    }
}
