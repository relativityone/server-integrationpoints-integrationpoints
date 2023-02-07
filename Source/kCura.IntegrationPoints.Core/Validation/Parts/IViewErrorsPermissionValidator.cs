using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public interface IViewErrorsPermissionValidator
    {
        ValidationResult Validate(int workspaceArtifactId);
    }
}
