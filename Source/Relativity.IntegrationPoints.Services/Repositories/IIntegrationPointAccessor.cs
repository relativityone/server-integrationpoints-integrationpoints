using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IIntegrationPointAccessor
    {
        IntegrationPointModel CreateIntegrationPoint(CreateIntegrationPointRequest request);
        IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request);
        IntegrationPointModel GetIntegrationPoint(int integrationPointArtifactId);
        object RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId);
        object RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, bool switchToAppendOverlayMode = false);
        IList<IntegrationPointModel> GetAllIntegrationPoints();
        int GetIntegrationPointArtifactTypeId();
        IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
        IntegrationPointModel CreateIntegrationPointFromProfile(int profileArtifactId, string integrationPointName);
    }
}
