using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
    public class RelativityProviderValidatorsFactory : IRelativityProviderValidatorsFactory
    {
        private readonly IAPILog _apiLogger;
        private readonly ILogger<RelativityProviderValidatorsFactory> _logger;
        private readonly IProductionManager _productionManager;
        private readonly IManagerFactory _managerFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ISerializer _serializer;
        private readonly IArtifactServiceFactory _artifactServiceFactory;
        private readonly IRelativityObjectManager _objectManager;

        public RelativityProviderValidatorsFactory(
            ISerializer serializer,
            IRepositoryFactory repositoryFactory,
            IHelper helper,
            IProductionManager productionManager,
            IManagerFactory managerFactory,
            IArtifactServiceFactory artifactServiceFactory,
            IRelativityObjectManager objectManager,
            ILogger<RelativityProviderValidatorsFactory> logger)
        {
            _serializer = serializer;
            _repositoryFactory = repositoryFactory;
            _productionManager = productionManager;
            _managerFactory = managerFactory;
            _artifactServiceFactory = artifactServiceFactory;

            _logger = logger;
            _apiLogger = helper.GetLoggerFactory().GetLogger();
            _objectManager = objectManager;
        }

        public FieldsMappingValidator CreateFieldsMappingValidator(int? federatedInstanceArtifactId, string credentials)
        {
            IFieldManager fieldManager = _managerFactory.CreateFieldManager();
            return new FieldsMappingValidator(_logger.ForContext<FieldsMappingValidator>(), _serializer, fieldManager, fieldManager);
        }

        public ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName, int? federatedInstanceArtifactId, string credentials)
        {
            IArtifactService artifactService = _artifactServiceFactory.CreateArtifactService();
            return new ArtifactValidator(artifactService, workspaceArtifactId, artifactTypeName);
        }

        public SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId)
        {
            return new SavedSearchValidator(_apiLogger, _repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId));
        }

        public ViewValidator CreateViewValidator(int workspaceArtifactId)
        {
            return new ViewValidator(_objectManager, _logger.ForContext<ViewValidator>());
        }

        public ProductionValidator CreateProductionValidator(int workspaceArtifactId)
        {
            return new ProductionValidator(workspaceArtifactId, _productionManager);
        }

        public ImportProductionValidator CreateImportProductionValidator(int workspaceArtifactId, int? federatedInstanceArtifactId, string credentials)
        {
            IPermissionManager destinationPermissionManager = CreatePermissionManager();
            return new ImportProductionValidator(workspaceArtifactId, _productionManager, destinationPermissionManager, federatedInstanceArtifactId, credentials);
        }

        public IRelativityProviderDestinationWorkspaceExistenceValidator CreateDestinationWorkspaceExistenceValidator(int? federatedInstanceArtifactId, string credentials)
        {
            IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();
            return new RelativityProviderDestinationWorkspaceExistenceValidator(workspaceManager);
        }

        public IRelativityProviderDestinationWorkspacePermissionValidator CreateDestinationWorkspacePermissionValidator(int? federatedInstanceArtifactId, string credentials)
        {
            IPermissionManager destinationWorkspacePermissionManager = CreatePermissionManager();

            return new RelativityProviderDestinationWorkspacePermissionValidator(destinationWorkspacePermissionManager, _repositoryFactory);
        }

        public IRelativityProviderDestinationFolderPermissionValidator CreateDestinationFolderPermissionValidator(int workspaceArtifactId, int? federatedInstanceArtifactId, string credentials)
        {
            IPermissionManager destinationWorkspacePermissionManager = CreatePermissionManager();
            return new RelativityProviderDestinationFolderPermissionValidator(workspaceArtifactId, destinationWorkspacePermissionManager);
        }

        public IRelativityProviderSourceWorkspacePermissionValidator CreateSourceWorkspacePermissionValidator()
        {
            IPermissionManager sourceWorkspacePermissionManager = CreatePermissionManager();
            return new RelativityProviderSourceWorkspacePermissionValidator(sourceWorkspacePermissionManager);
        }

        public IRelativityProviderSourceProductionPermissionValidator CreateSourceProductionPermissionValidator(int workspaceArtifactId)
        {
            IProductionRepository productionRepository = _repositoryFactory.GetProductionRepository(workspaceArtifactId);
            return new RelativityProviderSourceProductionPermissionValidator(productionRepository, _logger.ForContext<RelativityProviderSourceProductionPermissionValidator>());
        }

        private IPermissionManager CreatePermissionManager()
        {
            try
            {
                return _managerFactory.CreatePermissionManager();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred creating permission manager.");
                throw new IntegrationPointsException(IntegrationPointsExceptionMessages.ERROR_OCCURED_CONTACT_ADMINISTRATOR, ex)
                {
                    ExceptionSource = IntegrationPointsExceptionSource.VALIDATION,
                    ShouldAddToErrorsTab = false
                };
            }
        }

        public RelativityProviderWorkspaceNameValidator CreateWorkspaceNameValidator(string prefix)
        {
            IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();
            return new RelativityProviderWorkspaceNameValidator(workspaceManager, prefix);
        }

        public RelativityProviderWorkspaceNameValidator CreateWorkspaceNameValidator(string prefix, int? federatedInstanceArtifactId, string credentials)
        {
            IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();
            return new RelativityProviderWorkspaceNameValidator(workspaceManager, prefix);
        }
    }
}
