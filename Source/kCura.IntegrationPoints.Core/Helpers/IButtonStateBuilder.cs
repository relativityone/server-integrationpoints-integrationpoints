using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IButtonStateBuilder
    {
        ButtonStateDTO CreateButtonState(int applicationArtifactId, int integrationPointArtifactId);
    }
}