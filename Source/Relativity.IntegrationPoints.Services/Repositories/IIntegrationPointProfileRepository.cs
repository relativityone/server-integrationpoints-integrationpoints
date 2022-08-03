using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IIntegrationPointProfileRepository
    {
        IntegrationPointModel CreateIntegrationPointProfile(CreateIntegrationPointRequest request);
        IntegrationPointModel UpdateIntegrationPointProfile(CreateIntegrationPointRequest request);
        IntegrationPointModel GetIntegrationPointProfile(int integrationPointProfileArtifactId);
        IList<IntegrationPointModel> GetAllIntegrationPointProfiles();
        IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
        IntegrationPointModel CreateIntegrationPointProfileFromIntegrationPoint(int integrationPointArtifactId, string profileName);
    }
}