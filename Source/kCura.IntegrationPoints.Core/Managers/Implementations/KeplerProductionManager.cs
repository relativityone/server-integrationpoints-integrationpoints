﻿using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    class KeplerProductionManager : IProductionManagerWrapper
    {
        private readonly IProductionRepository _productionRepository;

        public KeplerProductionManager(IProductionRepository productionRepository)
        {
            _productionRepository = productionRepository;
        }

        public IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactId)
        {
            IEnumerable<ProductionDTO> productions = Task.Run(() => _productionRepository.GetProductionsForExport(workspaceArtifactId))
                .ConfigureAwait(false).GetAwaiter().GetResult();
            return productions;
        }

        public IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null,
            string federatedInstanceCredentials = null)
        {
            IEnumerable<ProductionDTO> productions = Task.Run(() => _productionRepository.GetProductionsForImport(workspaceArtifactId))
                .ConfigureAwait(false).GetAwaiter().GetResult();
            return productions;
        }

        public bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId,
            int? federatedInstanceId = null, string federatedInstanceCredentials = null)
        {
            ProductionDTO production = _productionRepository.GetProduction(workspaceArtifactId, productionId);
            return production != null;
        }
    }
}