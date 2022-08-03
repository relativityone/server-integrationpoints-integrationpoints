using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public interface IIntegrationPointProfileService
    {
        IList<IntegrationPointProfile> GetAllRDOs();
        IList<IntegrationPointProfile> GetAllRDOsWithAllFields();
        IntegrationPointProfile ReadIntegrationPointProfile(int artifactId);
        IntegrationPointProfileModel ReadIntegrationPointProfileModel(int artifactId);
        IList<IntegrationPointProfileModel> ReadIntegrationPointProfiles();
        int SaveIntegration(IntegrationPointProfileModel model);
        void UpdateIntegrationPointProfile(IntegrationPointProfile profile);
        IList<IntegrationPointProfileModel> ReadIntegrationPointProfilesSimpleModel();
    }
}