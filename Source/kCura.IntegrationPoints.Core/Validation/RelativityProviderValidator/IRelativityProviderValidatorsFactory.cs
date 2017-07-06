using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public interface IRelativityProviderValidatorsFactory
	{
		FieldsMappingValidator CreateFieldsMappingValidator(int? federatedInstanceArtifactId, string credentials);

		ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName, int? federatedInstanceArtifactId, string credentials);

		SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId);

		ProductionValidator CreateProductionValidator(int workspaceArtifactId);

		RelativityProviderWorkspaceValidator CreateWorkspaceValidator(string prefix);

		RelativityProviderWorkspaceValidator CreateWorkspaceValidator(string prefix, int? federatedInstanceArtifactId, string credentials);

		TransferredObjectValidator CreateTransferredObjectValidator();
        
	    ImportProductionValidator CreateImportProductionValidator(int workspaceArtifactId, int? federatedInstanceArtifactId, string credentials);
	}
}