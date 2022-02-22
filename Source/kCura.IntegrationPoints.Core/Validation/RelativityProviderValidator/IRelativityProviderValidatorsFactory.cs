using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public interface IRelativityProviderValidatorsFactory
	{
		FieldsMappingValidator CreateFieldsMappingValidator(int? federatedInstanceArtifactId, string credentials);

		ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName, int? federatedInstanceArtifactId, string credentials);

		SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId);

		ViewValidator CreateViewValidator(int workspaceArtifactId);

		ProductionValidator CreateProductionValidator(int workspaceArtifactId);

		IRelativityProviderDestinationWorkspaceExistenceValidator CreateDestinationWorkspaceExistenceValidator(int? federatedInstanceArtifactId, string credentials);

		IRelativityProviderDestinationWorkspacePermissionValidator CreateDestinationWorkspacePermissionValidator(int? federatedInstanceArtifactId, string credentials);

		IRelativityProviderDestinationFolderPermissionValidator CreateDestinationFolderPermissionValidator(int workspaceArtifactId, int? federatedInstanceArtifactId, string credentials);

		IRelativityProviderSourceWorkspacePermissionValidator CreateSourceWorkspacePermissionValidator();

		IRelativityProviderSourceProductionPermissionValidator CreateSourceProductionPermissionValidator(int workspaceArtifactId);

		RelativityProviderWorkspaceNameValidator CreateWorkspaceNameValidator(string prefix);

		RelativityProviderWorkspaceNameValidator CreateWorkspaceNameValidator(string prefix, int? federatedInstanceArtifactId, string credentials);

		ImportProductionValidator CreateImportProductionValidator(int workspaceArtifactId, int? federatedInstanceArtifactId, string credentials);
	}
}