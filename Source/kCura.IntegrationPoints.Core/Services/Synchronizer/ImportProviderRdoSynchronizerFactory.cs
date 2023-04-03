using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
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

        public IDataSynchronizer CreateSynchronizer(DestinationConfiguration destinationConfiguration, ITaskJobSubmitter taskJobSubmitter, IDiagnosticLog diagnosticLog)
        {
            return IsEntityObjectImport(destinationConfiguration) ?
                CreateEntityImportSynchronizer(taskJobSubmitter) :
                CreateGeneralRdoImportSynchronizer();
        }

        private bool IsEntityObjectImport(DestinationConfiguration destinationConfiguration)
        {
            ObjectTypeDTO rdoObjectType = _objectTypeRepository.GetObjectType(destinationConfiguration.ArtifactTypeId);
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
