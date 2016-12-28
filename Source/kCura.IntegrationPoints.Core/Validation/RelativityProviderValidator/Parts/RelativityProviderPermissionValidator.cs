using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class RelativityProviderPermissionValidator : BasePermissionValidator
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public RelativityProviderPermissionValidator(IRepositoryFactory repositoryFactoryFactory, ISerializer serializer, IServiceContextHelper contextHelper)
			: base(serializer, contextHelper)
		{
			_repositoryFactory = repositoryFactoryFactory;
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

			var sourceWorkspacePermissionRepository = _repositoryFactory.GetPermissionRepository(ContextHelper.WorkspaceID);
			//TODO change sourceConfiguration to destinationConfiguration after global model refactoring
			var destinationWorkspacePermissionRepository = _repositoryFactory.GetPermissionRepository(sourceConfiguration.TargetWorkspaceArtifactId);

			if (!sourceWorkspacePermissionRepository.UserCanExport())
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
			}

			if (!destinationWorkspacePermissionRepository.UserHasPermissionToAccessWorkspace())
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS);
			}

			if (!destinationWorkspacePermissionRepository.UserCanImport())
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
			}

			if (!destinationWorkspacePermissionRepository.UserHasArtifactTypePermissions(destinationConfiguration.ArtifactTypeId,
				new[] {ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create}))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
			}

			if (!sourceWorkspacePermissionRepository.UserCanEditDocuments())
			{
				result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			return result;
		}
	}
}