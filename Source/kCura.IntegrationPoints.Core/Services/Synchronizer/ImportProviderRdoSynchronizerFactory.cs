using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
    internal class ImportProviderRdoSynchronizerFactory
    {
        private readonly IWindsorContainer _container;
        private readonly IObjectTypeRepository _objectTypeRepository;

        public ImportProviderRdoSynchronizerFactory(IWindsorContainer container, IObjectTypeRepository objectTypeRepository)
        {
            _container = container;
            _objectTypeRepository = objectTypeRepository;
        }

        public IDataSynchronizer CreateSynchronizer(ImportSettings importSettings, ITaskJobSubmitter taskJobSubmitter, IDiagnosticLog diagnosticLog)
        {
            return IsEntityObjectImport(importSettings) ?
                CreateEntityImportSynchronizer(taskJobSubmitter) :
                CreateGeneralRdoImportSynchronizer();
        }

        private bool IsEntityObjectImport(ImportSettings importSettings)
        {
            ObjectTypeDTO rdoObjectType = _objectTypeRepository.GetObjectType(importSettings.ArtifactTypeId);
            return rdoObjectType.Guids.Contains(ObjectTypeGuids.Entity);
        }

        private IDataSynchronizer CreateGeneralRdoImportSynchronizer()
        {
            return _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizer).AssemblyQualifiedName);
        }

        private IDataSynchronizer CreateEntityImportSynchronizer(ITaskJobSubmitter taskJobSubmitter)
        {
            var s = (RdoEntitySynchronizer)_container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoEntitySynchronizer).AssemblyQualifiedName);
            s.TaskJobSubmitter = taskJobSubmitter;
            return s;
        }
    }
}
