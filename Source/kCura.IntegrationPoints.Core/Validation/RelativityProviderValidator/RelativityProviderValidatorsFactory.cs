using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
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
		private readonly IAPILog _logger;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ISerializer _serializer;
		private readonly IArtifactServiceFactory _artifactServiceFactory;

		public RelativityProviderValidatorsFactory(ISerializer serializer, IRepositoryFactory repositoryFactory,
			IHelper helper, IHelperFactory helperFactory, IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, IArtifactServiceFactory artifactServiceFactory)
		{
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
			_helper = helper;
			_helperFactory = helperFactory;
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_artifactServiceFactory = artifactServiceFactory;

			_logger = _helper.GetLoggerFactory().GetLogger();
		}

		public FieldsMappingValidator CreateFieldsMappingValidator(int? federatedInstanceArtifactId, string credentials)
		{
			IFieldManager sourceFieldManager = _managerFactory.CreateFieldManager(_contextContainerFactory.CreateContextContainer(_helper));

			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			IFieldManager targetFieldManager = _managerFactory.CreateFieldManager(_contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager()));

			return new FieldsMappingValidator(_logger, _serializer, sourceFieldManager, targetFieldManager);
		}

		public ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName, int? federatedInstanceArtifactId, string credentials)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			var artifactService = _artifactServiceFactory.CreateArtifactService(_helper, targetHelper);

			return new ArtifactValidator(artifactService, workspaceArtifactId, artifactTypeName);
		}

		public SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId)
		{
			return new SavedSearchValidator(_logger, _repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId), savedSearchArtifactId);
		}

		public ProductionValidator CreateProductionValidator(int workspaceArtifactId)
		{
			IProductionManager productionManager =
				_managerFactory.CreateProductionManager(_contextContainerFactory.CreateContextContainer(_helper));
			return new ProductionValidator(workspaceArtifactId, productionManager);
		}

		public ImportProductionValidator CreateImportProductionValidator(int workspaceArtifactId, int? federatedInstanceArtifactId, string credentials)
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IProductionManager importProductionManager = _managerFactory.CreateProductionManager(contextContainer);
			IPermissionManager destinationPermissionManager = CreateDestinationPermissionManager(federatedInstanceArtifactId, credentials);

			return new ImportProductionValidator(workspaceArtifactId, importProductionManager, destinationPermissionManager, federatedInstanceArtifactId, credentials);
		}

		public IRelativityProviderDestinationWorkspaceExistenceValidator CreateDestinationWorkspaceExistenceValidator(int? federatedInstanceArtifactId, string credentials)
		{
			IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager()));
			return new RelativityProviderDestinationWorkspaceExistenceValidator(workspaceManager);
		}

		public IRelativityProviderDestinationWorkspacePermissionValidator CreateDestinationWorkspacePermissionValidator(int? federatedInstanceArtifactId, string credentials)
		{
			IPermissionManager destinationWorkspacePermissionManager = CreateDestinationPermissionManager(federatedInstanceArtifactId, credentials);
			return new RelativityProviderDestinationWorkspacePermissionValidator(destinationWorkspacePermissionManager);
		}

		public IRelativityProviderDestinationFolderPermissionValidator CreateDestinationFolderPermissionValidator(int workspaceArtifactId, int? federatedInstanceArtifactId, string credentials)
		{
			IPermissionManager destinationWorkspacePermissionManager = CreateDestinationPermissionManager(federatedInstanceArtifactId, credentials);
			return new RelativityProviderDestinationFolderPermissionValidator(workspaceArtifactId, destinationWorkspacePermissionManager);
		}

		public IRelativityProviderSourceWorkspacePermissionValidator CreateSourceWorkspacePermissionValidator()
		{
			IPermissionManager sourceWorkspacePermissionManager = CreatePermissionManager(_helper);
			return new RelativityProviderSourceWorkspacePermissionValidator(sourceWorkspacePermissionManager);
		}

		public IRelativityProviderSourceProductionPermissionValidator CreateSourceProductionPermissionValidator(int workspaceArtifactId)
		{
			IProductionRepository productionRepository = _repositoryFactory.GetProductionRepository(workspaceArtifactId);
			return new RelativityProviderSourceProductionPermissionValidator(productionRepository, _logger);
		}

		private IPermissionManager CreateDestinationPermissionManager(int? federatedInstanceArtifactId, string credentials)
		{
			IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			return CreatePermissionManager(targetHelper);
		}

		private IPermissionManager CreatePermissionManager(IHelper helper)
		{
			try
			{
				return _managerFactory.CreatePermissionManager(_contextContainerFactory.CreateContextContainer(helper));
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
			IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(_helper, _helper.GetServicesManager()));
			return new RelativityProviderWorkspaceNameValidator(workspaceManager, prefix);
		}

		public RelativityProviderWorkspaceNameValidator CreateWorkspaceNameValidator(string prefix, int? federatedInstanceArtifactId, string credentials)
		{
			IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager()));
			return new RelativityProviderWorkspaceNameValidator(workspaceManager, prefix);
		}

		public TransferredObjectValidator CreateTransferredObjectValidator()
		{
			return new TransferredObjectValidator();
		}
	}
}