using System.Collections.Generic;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeIntegrationPointTypeRepository : IIntegrationPointTypeRepository
    {
        public IList<IntegrationPointTypeModel> GetIntegrationPointTypes()
        {
            IList<IntegrationPointTypeModel> integrationPointModels = new List<IntegrationPointTypeModel>
            {
                new IntegrationPointTypeModel
                {
                    ArtifactId = 10,
                    Name = "Adler Sieben"
                }
            };

            return integrationPointModels;
        }
    }
}
