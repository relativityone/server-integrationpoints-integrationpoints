using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces
{
    public interface IRelativityProviderSourceWorkspacePermissionValidator
    {
        ValidationResult Validate(int sourceWorkspaceId, int artifactTypeId);
    }
}
