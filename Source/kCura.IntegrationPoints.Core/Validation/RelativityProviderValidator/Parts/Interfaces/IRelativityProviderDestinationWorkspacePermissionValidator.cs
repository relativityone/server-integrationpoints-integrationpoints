using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces
{
    public interface IRelativityProviderDestinationWorkspacePermissionValidator
    {
        ValidationResult Validate(int destinationWorkspaceId, int destinationTypeId, bool createSavedSearch);
    }
}
