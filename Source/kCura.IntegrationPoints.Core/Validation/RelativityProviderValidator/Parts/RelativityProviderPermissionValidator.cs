using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class RelativityProviderPermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IHelperFactory _helperFactory;
		private readonly IHelper _helper;

		public RelativityProviderPermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer, IServiceContextHelper contextHelper,
			IHelper helper, IHelperFactory helperFactory, IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory)
			: base(serializer, contextHelper)
		{
			_repositoryFactory = repositoryFactoryFactory;
			_helper = helper;
			_helperFactory = helperFactory;
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
		}

		public override string Key
			=>
			IntegrationPointPermissionValidator.GetProviderValidatorKey(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
				Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();

			SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
			DestinationConfiguration destinationConfiguration = Serializer.Deserialize<DestinationConfiguration>(model.DestinationConfiguration);

			var sourceWorkspacePermissionManager =
				_managerFactory.CreatePermissionManager(_contextContainerFactory.CreateContextContainer(_helper));

			var targetHelper = _helperFactory.CreateTargetHelper(_helper, sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);
			var destinationWorkspacePermissionManager = 
				_managerFactory.CreatePermissionManager(_contextContainerFactory.CreateContextContainer(targetHelper));

			if (!sourceWorkspacePermissionManager.UserCanExport(ContextHelper.WorkspaceID))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
			}

			if (!destinationWorkspacePermissionManager.UserHasPermissionToAccessWorkspace(sourceConfiguration.TargetWorkspaceArtifactId))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS);
			}

			if (!destinationWorkspacePermissionManager.UserCanImport(sourceConfiguration.TargetWorkspaceArtifactId))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
			}

			if (!destinationWorkspacePermissionManager.UserHasArtifactTypePermissions(sourceConfiguration.TargetWorkspaceArtifactId, destinationConfiguration.ArtifactTypeId,
				new[] {ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create}))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			if (!sourceWorkspacePermissionManager.UserCanEditDocuments(ContextHelper.WorkspaceID))
			{
				result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			return result;
		}
	}
}