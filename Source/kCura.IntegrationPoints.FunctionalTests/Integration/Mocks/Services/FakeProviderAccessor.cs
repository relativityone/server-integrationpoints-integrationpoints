using System.Collections.Generic;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeProviderAccessor : IProviderAccessor
    {
        public int GetDestinationProviderArtifactId(int workspaceArtifactId, string destinationProviderGuidIdentifier)
        {
            return 10;
        }

        public int GetSourceProviderArtifactId(int workspaceArtifactId, string sourceProviderGuidIdentifier)
        {
            return 11;
        }

        public IList<ProviderModel> GetSourceProviders(int workspaceArtifactId)
        {
            IList<ProviderModel> providerModels = new List<ProviderModel>
            {
                new ProviderModel
                {
                    ArtifactId = 12,
                    Name = "Adler Sieben"
                }
            };

            return providerModels;
        }

        public IList<ProviderModel> GetDesinationProviders(int workspaceArtifactId)
        {
            IList<ProviderModel> providerModels = new List<ProviderModel>
            {
                new ProviderModel
                {
                    ArtifactId = 13,
                    Name = "Adler Sieben"
                }
            };

            return providerModels;
        }
    }
}
