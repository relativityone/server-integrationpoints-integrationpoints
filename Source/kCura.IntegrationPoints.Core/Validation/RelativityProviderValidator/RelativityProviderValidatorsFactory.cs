using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public class RelativityProviderValidatorsFactory : IRelativityProviderValidatorsFactory
	{
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ISerializer _serializer;
		private readonly IServiceFactory _serviceFactory;

		public RelativityProviderValidatorsFactory(ISerializer serializer, IRepositoryFactory repositoryFactory,
			IHelper helper, IHelperFactory helperFactory, IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, IServiceFactory serviceFactory)
		{
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
			_helper = helper;
			_helperFactory = helperFactory;
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_serviceFactory = serviceFactory;
		}

		public FieldsMappingValidator CreateFieldsMappingValidator(int? federatedInstanceArtifactId, string credentials)
		{
			IFieldManager sourceFieldManager = _managerFactory.CreateFieldManager(_contextContainerFactory.CreateContextContainer(_helper));

			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			IFieldManager targetFieldManager = _managerFactory.CreateFieldManager(_contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager()));

			return new FieldsMappingValidator(_serializer, sourceFieldManager, targetFieldManager);
		}

		public ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName, int? federatedInstanceArtifactId, string credentials)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			var artifactService = _serviceFactory.CreateArtifactService(_helper, targetHelper);

			return new ArtifactValidator(artifactService, workspaceArtifactId, artifactTypeName);
		}

		public SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId)
		{
			return new SavedSearchValidator(_repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId), savedSearchArtifactId);
		}

		public RelativityProviderWorkspaceValidator CreateWorkspaceValidator(string prefix, int? federatedInstanceArtifactId, string credentials)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceArtifactId, credentials);
			IWorkspaceManager workspaceManager =
				_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(_helper, targetHelper.GetServicesManager()));
			return new RelativityProviderWorkspaceValidator(workspaceManager, prefix);
		}

		public TransferredObjectValidator CreateTransferredObjectValidator()
		{
			return new TransferredObjectValidator();
		}
	}
}