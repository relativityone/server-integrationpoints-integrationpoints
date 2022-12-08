using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
    public class RelativityIntegrationPointsRepositoryAdminAccess : IRelativityIntegrationPointsRepository
    {
        private readonly IRelativityObjectManagerService _relativityObjectManagerService;
        private readonly IIntegrationPointService _integrationPointService;

        public RelativityIntegrationPointsRepositoryAdminAccess(
            IRelativityObjectManagerService relativityObjectManagerService,
            IIntegrationPointService integrationPointService)
        {
            _relativityObjectManagerService = relativityObjectManagerService;
            _integrationPointService = integrationPointService;
        }

        public List<IntegrationPointSlimDto> RetrieveIntegrationPoints()
        {
            var sourceProviderIds = RetrieveRelativitySourceProviderIds();
            var destinationProviderIds = RetrieveRelativityDestinationProviderIds();

            return _integrationPointService.GetBySourceAndDestinationProvider(sourceProviderIds[0], destinationProviderIds[0]);
        }

        private IList<int> RetrieveRelativitySourceProviderIds()
        {
            QueryRequest request = new QueryRequest()
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{kCura.IntegrationPoints.Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID}'"
            };
            return GetArtifactIds(_relativityObjectManagerService.RelativityObjectManager.Query<SourceProvider>(request));
        }

        private IList<int> RetrieveRelativityDestinationProviderIds()
        {
            QueryRequest request = new QueryRequest()
            {
                Condition = $"'{DestinationProviderFields.Identifier}' == '{kCura.IntegrationPoints.Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID}'"
            };
            return GetArtifactIds(_relativityObjectManagerService.RelativityObjectManager.Query<DestinationProvider>(request));
        }

        private List<int> GetArtifactIds<T>(List<T> results) where T : BaseRdo
        {
            return results.Select(x => x.ArtifactId).ToList();
        }
    }
}
