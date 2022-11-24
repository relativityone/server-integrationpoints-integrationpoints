using System;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Description("This is an event handler that migrates Integration Points that used Processing Source Location to use Data Transfer Location")]
    [Guid("A48963EA-AB72-4B98-A634-7721B6D2BB9E")]
    [RunOnce(true)]
    public class IntegrationPointDataTransferLocationMigrationEventHandler : PostInstallEventHandlerBase
    {
        private IDestinationProviderRepository _destinationProviderRepository;
        private ISourceProviderRepository _sourceProviderRepository;
        private IRepositoryFactory _repositoryFactory;
        private IDataTransferLocationMigrationHelper _dataTransferLocationMigrationHelper;
        private ICaseServiceContext _serviceContext;
        private IDataTransferLocationService _dataTransferLocationService;
        private IResourcePoolManager _resourcePoolManager;
        private IDataTransferLocationMigration _dataTransferLocationMigration;

        internal IRepositoryFactory RepositoryFactory
        {
            get
            {
                if (_repositoryFactory == null)
                {
                    _repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
                }

                return _repositoryFactory;
            }
        }

        internal IDestinationProviderRepository DestinationProviderRepository
        {
            get
            {
                if (_destinationProviderRepository == null)
                {
                    _destinationProviderRepository = RepositoryFactory.GetDestinationProviderRepository(Helper.GetActiveCaseID());
                }

                return _destinationProviderRepository;
            }
        }

        internal ISourceProviderRepository SourceProviderRepository
        {
            get
            {
                if (_sourceProviderRepository == null)
                {
                    _sourceProviderRepository = RepositoryFactory.GetSourceProviderRepository(Helper.GetActiveCaseID());
                }

                return _sourceProviderRepository;
            }
        }

        internal IDataTransferLocationMigrationHelper DataTransferLocationMigrationHelper
        {
            get
            {
                if (_dataTransferLocationMigrationHelper == null)
                {
                    ISerializer serializer = new JSONSerializer();
                    _dataTransferLocationMigrationHelper = new DataTransferLocationMigrationHelper(serializer);
                }

                return _dataTransferLocationMigrationHelper;
            }
        }

        internal ICaseServiceContext CaseServiceContext
        {
            get
            {
                if (_serviceContext == null)
                {
                    _serviceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
                }

                return _serviceContext;
            }
        }

        internal IDataTransferLocationService DataTransferLocationService
        {
            get
            {
                if (_dataTransferLocationService == null)
                {
                    IIntegrationPointTypeService typeService = new IntegrationPointTypeService(Helper, CaseServiceContext);
                    _dataTransferLocationService = new DataTransferLocationService(Helper, typeService, new LongPathDirectory(), new CryptographyHelper());
                }

                return _dataTransferLocationService;
            }
        }

        internal IResourcePoolManager ResourcePoolManager
        {
            get
            {
                if (_resourcePoolManager == null)
                {
                    _resourcePoolManager = new ResourcePoolManager(RepositoryFactory, Helper.GetServicesManager(), Helper);
                }

                return _resourcePoolManager;
            }
        }

        internal IDataTransferLocationMigration DataTransferLocationMigration
        {
            get
            {
                if (_dataTransferLocationMigration == null)
                {
                    _dataTransferLocationMigration = new DataTransferLocationMigration(
                        Logger,
                        DestinationProviderRepository,
                        SourceProviderRepository,
                        DataTransferLocationMigrationHelper,
                        new IntegrationPointRepository(
                            ObjectManager,
                            new SecretsRepository(
                                SecretStoreFacadeFactory_Deprecated.Create(Helper.GetSecretStore, Logger),
                                Logger
                            ),
                            Logger),
                        DataTransferLocationService,
                        ResourcePoolManager,
                        Helper);
                }

                return _dataTransferLocationMigration;
            }
        }

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointDataTransferLocationMigrationEventHandler>();
        }

        protected override string SuccessMessage => "Integration Points Data Transfer Location migrated successfully";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Integration Points Data Transfer Location migration failed";
        }

        protected override void Run()
        {
            DataTransferLocationMigration.Migrate();
        }
    }
}
