using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
    public class GeneralWithEntityRdoSynchronizerFactory : ISynchronizerFactory
    {
        private readonly RelativityRdoSynchronizerFactory _relativityRdoSynchronizerFactory;
        private readonly ImportProviderRdoSynchronizerFactory _importProviderRdoSynchronizerFactory;

        public GeneralWithEntityRdoSynchronizerFactory(IWindsorContainer container, IObjectTypeRepository objectTypeRepository)
        {
            _relativityRdoSynchronizerFactory = new RelativityRdoSynchronizerFactory(container);
            _importProviderRdoSynchronizerFactory = new ImportProviderRdoSynchronizerFactory(container, objectTypeRepository);
        }

        public ITaskJobSubmitter TaskJobSubmitter { get; set; }

        public SourceProvider SourceProvider { get; set; }

        public IDataSynchronizer CreateSynchronizer(Guid identifier, DestinationConfiguration options)
        {
            return string.Equals(options.Provider, nameof(ProviderType.Relativity), StringComparison.InvariantCultureIgnoreCase)
                ? _relativityRdoSynchronizerFactory.CreateSynchronizer(options, SourceProvider)
                : _importProviderRdoSynchronizerFactory.CreateSynchronizer(options, TaskJobSubmitter);
        }
    }
}
