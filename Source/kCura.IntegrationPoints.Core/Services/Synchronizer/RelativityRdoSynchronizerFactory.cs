using System.Collections.Generic;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
    internal class RelativityRdoSynchronizerFactory
    {
        private readonly IWindsorContainer _container;

        public RelativityRdoSynchronizerFactory(IWindsorContainer container)
        {
            _container = container;
        }

        public IDataSynchronizer CreateSynchronizer(ImportSettings importSettings, SourceProvider sourceProvider)
        {
            Dictionary<string, RelativityFieldQuery> rdoSynchronizerParametersDictionary = CreateRdoSynchronizerParametersDictionary(importSettings);

            IDataSynchronizer synchronizer = _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizer).AssemblyQualifiedName, rdoSynchronizerParametersDictionary);
            RdoSynchronizer syncBase = (RdoSynchronizer)synchronizer;
            syncBase.SourceProvider = sourceProvider;
            return syncBase;
        }

        private Dictionary<string, RelativityFieldQuery> CreateRdoSynchronizerParametersDictionary(ImportSettings importSettings)
        {
            IHelper sourceInstanceHelper = _container.Resolve<IHelper>();
            IRelativityObjectManagerFactory relativityObjectManagerFactory = _container.Resolve<IRelativityObjectManagerFactory>();
            IRelativityObjectManager relativityObjectManager = relativityObjectManagerFactory.CreateRelativityObjectManager(importSettings.CaseArtifactId);

            return new Dictionary<string, RelativityFieldQuery>
            {
                {"fieldQuery", new RelativityFieldQuery(relativityObjectManager, sourceInstanceHelper)}
            };
        }
    }
}
