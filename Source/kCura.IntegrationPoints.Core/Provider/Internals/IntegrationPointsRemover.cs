using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public class IntegrationPointsRemover : IIntegrationPointsRemover
    {
        private readonly IDeleteHistoryService _deleteHistoryService;
        private readonly IIntegrationPointRepository _integrationPointRepository;

        public IntegrationPointsRemover(
            IDeleteHistoryService deleteHistoryService,
            IIntegrationPointRepository integrationPointRepository)
        {
            _deleteHistoryService = deleteHistoryService;
            _integrationPointRepository = integrationPointRepository;
        }

        public void DeleteIntegrationPointsBySourceProvider(List<int> sourceProvidersIDs)
        {
            IList<IntegrationPoint> integrationPoints = _integrationPointRepository.ReadBySourceProviders(sourceProvidersIDs);
            List<int> integrationPointsArtifactIDs = integrationPoints.Select(x => x.ArtifactId).ToList();
            _deleteHistoryService.DeleteHistoriesAssociatedWithIPs(integrationPointsArtifactIDs);
            foreach (IntegrationPoint ip in integrationPoints)
            {
                _integrationPointRepository.Delete(ip.ArtifactId);
            }
        }
    }
}
