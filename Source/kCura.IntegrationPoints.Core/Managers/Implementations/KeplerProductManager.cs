using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    class KeplerProductManager : IProductionManagerWrapper
    {
        private readonly IProductionRepository _productionRepository;

        public KeplerProductManager(IProductionRepository productionRepository)
        {
            _productionRepository = productionRepository;
        }

        public IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactId)
        {
            IEnumerable<ProductionDTO> allProductions = Task.Run(() => _productionRepository.RetrieveAllProductionsAsync(workspaceArtifactId))
                .ConfigureAwait(false).GetAwaiter().GetResult();
            return allProductions;
        }

        public IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null,
            string federatedInstanceCredentials = null)
        {
            throw new NotImplementedException();
        }

        public bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId,
            int? federatedInstanceId = null, string federatedInstanceCredentials = null)
        {
            throw new NotImplementedException();
        }
    }
}
