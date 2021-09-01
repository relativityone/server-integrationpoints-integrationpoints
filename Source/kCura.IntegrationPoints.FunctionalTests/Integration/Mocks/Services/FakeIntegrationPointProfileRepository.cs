using System.Collections.Generic;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeIntegrationPointProfileRepository : FakeIntegrationPointRepository, IIntegrationPointProfileRepository
    {

        public IntegrationPointModel CreateIntegrationPointProfile(CreateIntegrationPointRequest request)
        {
            return IntegrationPointModel;
        }

        public IntegrationPointModel UpdateIntegrationPointProfile(CreateIntegrationPointRequest request)
        {
            return IntegrationPointModel;
        }

        public IntegrationPointModel GetIntegrationPointProfile(int integrationPointProfileArtifactId)
        {
            return IntegrationPointModel;
        }

        public IList<IntegrationPointModel> GetAllIntegrationPointProfiles()
        {
            return GetAllIntegrationPoints();
        }

        public IntegrationPointModel CreateIntegrationPointProfileFromIntegrationPoint(int integrationPointArtifactId,
            string profileName)
        {
            return IntegrationPointModel;
        }
    }
}
