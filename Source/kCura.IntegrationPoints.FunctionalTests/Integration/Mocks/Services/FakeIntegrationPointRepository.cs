using System.Collections.Generic;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeIntegrationPointRepository : IIntegrationPointRepository
    {
        protected readonly IntegrationPointModel IntegrationPointModel;

        public FakeIntegrationPointRepository()
        {
            IntegrationPointModel = new IntegrationPointModel
            {
                ArtifactId = 10,
                DestinationProvider = 11,
                Name = "Adler Sieben",
                EmailNotificationRecipients = "Adler Sieben"
            };
        }
        public IntegrationPointModel CreateIntegrationPoint(CreateIntegrationPointRequest request)
        {
            return IntegrationPointModel;
        }

        public IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request)
        {
            return IntegrationPointModel;
        }

        public IntegrationPointModel GetIntegrationPoint(int integrationPointArtifactId)
        {
            return IntegrationPointModel;
        }

        public object RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId)
        {
            return IntegrationPointModel;
        }

        public IList<IntegrationPointModel> GetAllIntegrationPoints()
        {
            IList<IntegrationPointModel> integrationPointModels = new List<IntegrationPointModel>
            {
                IntegrationPointModel
            };

            return integrationPointModels;
        }

        public int GetIntegrationPointArtifactTypeId()
        {
            return 10;
        }

        public IList<OverwriteFieldsModel> GetOverwriteFieldChoices()
        {
            IList<OverwriteFieldsModel> overwriteFieldsModels = new List<OverwriteFieldsModel>
            {
                new OverwriteFieldsModel
                {
                    Name = "Adler Sieben",
                    ArtifactId = 10
                }
            };

            return overwriteFieldsModels;
        }

        public IntegrationPointModel CreateIntegrationPointFromProfile(int profileArtifactId, string integrationPointName)
        {
            return IntegrationPointModel;
        }

        public object RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, bool switchToAppendOverlayMode)
        {
            return IntegrationPointModel;
        }
    }
}
