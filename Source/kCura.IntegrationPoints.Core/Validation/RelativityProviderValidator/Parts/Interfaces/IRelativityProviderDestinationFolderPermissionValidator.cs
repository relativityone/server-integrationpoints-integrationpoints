using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces
{
    public interface IRelativityProviderDestinationFolderPermissionValidator
    {
        ValidationResult Validate(int destinationFolderArtifactId, bool useFolderPathInfo, bool moveExistingDocuments);
    }
}
