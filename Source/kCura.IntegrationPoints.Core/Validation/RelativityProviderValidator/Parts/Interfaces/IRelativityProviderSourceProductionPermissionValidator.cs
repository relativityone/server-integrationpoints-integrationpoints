using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces
{
    public interface IRelativityProviderSourceProductionPermissionValidator
    {
        ValidationResult Validate(int sourceWorkspaceId, int sourceProductionArtifactId);
    }
}